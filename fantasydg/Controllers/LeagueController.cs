using fantasydg.Data;
using fantasydg.Models;
using fantasydg.Models.ViewModels;
using fantasydg.Models.Repository;
using fantasydg.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using System.Security.Claims;

namespace fantasydg.Controllers
{
    [Authorize]
    public class LeagueController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly LeagueService _leagueService;
        private readonly DatabaseRepository _repository;

        public LeagueController(ApplicationDbContext db, LeagueService leagueService, DatabaseRepository repository)
        {
            _db = db;
            _leagueService = leagueService;
            _repository = repository;
        }

        // View all leagues the current user is part of
        public async Task<IActionResult> Index()
        {
            var userId = User?.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            var leagues = await _db.LeagueMembers
                .Include(lm => lm.League)
                .ThenInclude(l => l.Members)
                .Where(lm => lm.UserId == userId)
                .Select(lm => lm.League)
                .ToListAsync();

            return View(leagues);
        }

        
        // GET: /League/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(League league)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            league.OwnerId = userId;
            league.PlayerNumber = 0;

            ModelState.Remove(nameof(league.OwnerId)); // fix lingering error

            if (!ModelState.IsValid)
            {
                return View("LeagueView", league);
            }

            // First save the league
            _db.Leagues.Add(league);
            await _db.SaveChangesAsync();

            // Now add the creator as a member using the generated LeagueId
            _db.LeagueMembers.Add(new LeagueMember
            {
                LeagueId = league.LeagueId,
                UserId = userId
            });

            await _db.SaveChangesAsync();

            return RedirectToAction("Create", "Team", new { leagueId = league.LeagueId });
        }

        public async Task<IActionResult> View(int id)
        {
            var league = await _db.Leagues
                .Include(l => l.Members).ThenInclude(m => m.User)
                .Include(l => l.Teams)
                .FirstOrDefaultAsync(l => l.LeagueId == id);

            if (league == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var team = await _db.Teams.FirstOrDefaultAsync(t => t.LeagueId == id && t.OwnerId == userId);
            ViewBag.IsOwner = league.OwnerId == userId;
            ViewBag.MemberCount = league.Members?.Count ?? 0;
            ViewBag.TeamId = team?.TeamId;
            ViewBag.LeagueName = league.Name;

            ViewBag.Standings = await GetStandings(id);

            return View("LeagueView", league);
        }

        private async Task<List<LeagueStandingView>> GetStandings(int leagueId)
        {
            var league = await _db.Leagues.FirstOrDefaultAsync(l => l.LeagueId == leagueId);
            if (league == null) return new List<LeagueStandingView>();

            var standings = await _db.Teams
                .Where(t => t.LeagueId == leagueId)
                .Include(t => t.Owner)
                .Include(t => t.TeamPlayerTournaments)
                    .ThenInclude(tpt => tpt.Player)
                    .ThenInclude(p => p.PlayerTournaments)
                .ToListAsync();

            var ordered = standings
                .Select(t => new
                {
                    Team = t,
                    OwnerName = t.Owner.UserName,
                    Points = t.TeamPlayerTournaments
                        .Where(tpt => tpt.Status == nameof(RosterStatus.Starter) && tpt.IsLocked)
                        .Select(tpt =>
                            tpt.Player.PlayerTournaments
                                .FirstOrDefault(pt => pt.TournamentId == tpt.TournamentId))
                        .Where(pt => pt != null)
                        .Sum(pt =>
                            pt.Place * league.PlacementWeight +
                            pt.Fairway * league.FairwayWeight +
                            pt.C1InReg * league.C1InRegWeight +
                            pt.C2InReg * league.C2InRegWeight +
                            pt.Parked * league.ParkedWeight +
                            pt.Scramble * league.ScrambleWeight +
                            pt.C1Putting * league.C1PuttWeight +
                            pt.C1xPutting * league.C1xPuttWeight +
                            pt.C2Putting * league.C2PuttWeight +
                            pt.ObRate * league.OBWeight +
                            pt.Par * league.ParWeight +
                            pt.Birdie * league.BirdieWeight +
                            pt.BirdieMinus * league.BirdieMinusWeight +
                            pt.EagleMinus * league.EagleMinusWeight +
                            pt.BogeyPlus * league.BogeyPlusWeight +
                            pt.DoubleBogeyPlus * league.DoubleBogeyPlusWeight +
                            pt.TotalPuttDistance * league.TotalPuttDistWeight +
                            pt.AvgPuttDistance * league.AvgPuttDistWeight +
                            pt.LongThrowIn * league.LongThrowInWeight +
                            pt.StrokesGainedTotal * league.TotalSGWeight +
                            pt.StrokesGainedPutting * league.PuttingSGWeight +
                            pt.StrokesGainedTeeToGreen * league.TeeToGreenSGWeight +
                            pt.StrokesGainedC1xPutting * league.C1xSGWeight +
                            pt.StrokesGainedC2Putting * league.C2SGWeight
                        )
                })
                .OrderByDescending(t => t.Points)
                .Select((s, index) => new LeagueStandingView
                {
                    Placement = index + 1,
                    MemberName = s.OwnerName,
                    TeamName = s.Team.Name,
                    FantasyPoints = s.Points
                })
                .ToList();

            return ordered;
        }

        [HttpGet]
        public async Task<IActionResult> TeamTournamentResultsView(int leagueId, int? tournamentId = null, string? division = null, int? round = null)
        {
            // Load tournaments and set default selection
            var allTournaments = await _repository.GetAllTournamentsAsync();
            var orderedTournaments = allTournaments.OrderByDescending(t => t.Date).ToList();
            ViewBag.Tournaments = orderedTournaments;

            tournamentId ??= orderedTournaments.FirstOrDefault()?.Id;
            var selectedTournament = orderedTournaments.FirstOrDefault(t => t.Id == tournamentId);
            if (selectedTournament == null) return NotFound();

            ViewBag.SelectedTournamentId = selectedTournament.Id;

            // Load divisions and set default
            var divisions = await _repository.GetDivisionsForTournamentAsync(selectedTournament.Id);
            ViewBag.Divisions = divisions.OrderByDescending(d => d).ToList();

            division ??= divisions.Contains("MPO") ? "MPO" : divisions.FirstOrDefault();
            ViewBag.SelectedDivision = division;

            // Get all assigned players in league
            var assignedPDGAs = await _db.TeamPlayers
                .Where(tp => tp.LeagueId == leagueId)
                .Select(tp => tp.PDGANumber)
                .ToListAsync();

            // Filter players who participated in the selected tournament
            var participatingPlayers = await _db.PlayerTournaments
                .Include(pt => pt.Player)
                .Include(pt => pt.Tournament)
                .Where(pt =>
                    pt.TournamentId == selectedTournament.Id &&
                    pt.Tournament.Division == division &&
                    assignedPDGAs.Contains(pt.PDGANumber))
                .ToListAsync();

            // Load league and teams
            var league = await _db.Leagues
                .Include(l => l.Teams)
                .ThenInclude(t => t.TeamPlayers)
                .FirstOrDefaultAsync(l => l.LeagueId == leagueId);
            if (league == null) return NotFound();

            // Ensure fantasy points are up to date
            await _leagueService.UpdateFantasyPointsForLeagueAsync(leagueId);
            var fantasyMap = await _leagueService.GetFantasyPointsMapAsync(leagueId);

            // Prepare view model
            var model = new LeaguePlayersViewModel
            {
                League = league,
                Players = participatingPlayers
            };

            var playerTeamMap = await _db.TeamPlayers
                .Where(tp => tp.LeagueId == leagueId)
                .ToDictionaryAsync(tp => tp.PDGANumber, tp => tp.Team.Name);

            // ViewBag values
            ViewBag.LeagueId = league.LeagueId;
            ViewBag.LeagueName = league.Name;
            ViewBag.FantasyMap = fantasyMap;
            ViewBag.PlayerTeamMap = playerTeamMap;
            ViewBag.TeamId = await _db.Teams
                .Where(t => t.LeagueId == leagueId && t.OwnerId == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .Select(t => (int?)t.TeamId)
                .FirstOrDefaultAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("~/Views/Tournament/TeamPlayerTable.cshtml", model);

            return View("~/Views/Tournament/TeamTournamentResultsView.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> Players(int leagueId, int? tournamentId = null, string? division = null, int? round = null)
        {
            var allTournaments = await _repository.GetAllTournamentsAsync();
            ViewBag.Tournaments = allTournaments.OrderByDescending(t => t.Date).ToList();

            if (!tournamentId.HasValue)
                tournamentId = allTournaments.FirstOrDefault()?.Id;

            var selectedTournament = allTournaments.FirstOrDefault(t => t.Id == tournamentId);

            tournamentId = selectedTournament.Id;
            ViewBag.SelectedTournamentId = tournamentId;

            var divisions = await _repository.GetDivisionsForTournamentAsync(tournamentId.Value);
            ViewBag.Divisions = divisions.OrderByDescending(d => d).ToList();

            if (string.IsNullOrEmpty(division))
                division = divisions.Contains("MPO") ? "MPO" : divisions.FirstOrDefault();

            ViewBag.SelectedDivision = division;

            var league = await _db.Leagues
                .Include(l => l.Teams)
                .ThenInclude(t => t.TeamPlayers)
                .FirstOrDefaultAsync(l => l.LeagueId == leagueId);

            if (league == null) return NotFound();

            await _leagueService.UpdateFantasyPointsForLeagueAsync(leagueId);

            var fantasyMap = await _leagueService.GetFantasyPointsMapAsync(leagueId);
            ViewBag.FantasyMap = fantasyMap;

            var assignedPDGANumbers = await _db.TeamPlayers
                .Where(tp => tp.LeagueId == leagueId)
                .Select(tp => tp.PDGANumber)
                .ToListAsync();

            var unassigned = await _db.PlayerTournaments
                .Include(pt => pt.Player)
                .Include(pt => pt.Tournament)
                .Where(pt =>
                    pt.Tournament.Division == division &&
                    pt.TournamentId == tournamentId &&
                    !assignedPDGANumbers.Contains(pt.PDGANumber))
                .ToListAsync();

            var model = new LeaguePlayersViewModel
            {
                League = league,
                Players = unassigned
            };

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var team = await _db.Teams
                .FirstOrDefaultAsync(t => t.LeagueId == leagueId && t.OwnerId == userId);

            ViewBag.LeagueId = leagueId;
            ViewBag.LeagueName = league.Name;
            ViewBag.TeamId = team.TeamId;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("~/Views/Players/PlayerTable.cshtml", model);
            }

            return View("~/Views/Players/LeaguePlayers.cshtml", model);
        }

        // View and manage settings for a specific league
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Settings(int id)
        {
            var league = await _db.Leagues
                .Include(l => l.Owner)
                .Include(l => l.Members).ThenInclude(m => m.User)
                .FirstOrDefaultAsync(l => l.LeagueId == id);

            if (league == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (league.OwnerId != userId)
            {
                return RedirectToAction("View", "League", new { id = league.LeagueId });
            }

            ViewBag.LeagueName = league.Name;
            ViewBag.TeamId = await _db.Teams
                .Where(t => t.LeagueId == league.LeagueId && t.OwnerId == userId)
                .Select(t => (int?)t.TeamId)
                .FirstOrDefaultAsync();

            return View(league);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveScoringSettings(League model, int id)
        {
            var league = await _db.Leagues.FindAsync(model.LeagueId);
            if (league == null) return NotFound();

            // Update weights
            league.PlacementWeight = model.PlacementWeight;
            league.FairwayWeight = model.FairwayWeight;
            league.C1InRegWeight = model.C1InRegWeight;
            league.C2InRegWeight = model.C2InRegWeight;
            league.ParkedWeight = model.ParkedWeight;
            league.ScrambleWeight = model.ScrambleWeight;
            league.C1PuttWeight = model.C1PuttWeight;
            league.C1xPuttWeight = model.C1xPuttWeight;
            league.C2PuttWeight = model.C2PuttWeight;
            league.OBWeight = model.OBWeight;
            league.BirdieWeight = model.BirdieWeight;
            league.BirdieMinusWeight = model.BirdieMinusWeight;
            league.EagleMinusWeight = model.EagleMinusWeight;
            league.ParWeight = model.ParWeight;
            league.BogeyPlusWeight = model.BogeyPlusWeight;
            league.DoubleBogeyPlusWeight = model.DoubleBogeyPlusWeight;
            league.TotalPuttDistWeight = model.TotalPuttDistWeight;
            league.AvgPuttDistWeight = model.AvgPuttDistWeight;
            league.LongThrowInWeight = model.LongThrowInWeight;
            league.TotalSGWeight = model.TotalSGWeight;
            league.PuttingSGWeight = model.PuttingSGWeight;
            league.TeeToGreenSGWeight = model.TeeToGreenSGWeight;
            league.C1xSGWeight = model.C1xSGWeight;
            league.C2SGWeight = model.C2SGWeight;

            await _db.SaveChangesAsync();
            await _leagueService.UpdateFantasyPointsForLeagueAsync(league.LeagueId);

            TempData["ScoringSaved"] = "Scoring settings updated!";
            return RedirectToAction("Settings", new { id = league.LeagueId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRosterSettings(int leagueId, int starterCount, int rosterLimit, int injuryReserveLimit)
        {
            var league = await _db.Leagues.FindAsync(leagueId);
            if (league == null) return NotFound();

            // Server-side validation
            if (starterCount < 3 || starterCount > 20 ||
                rosterLimit < 5 || rosterLimit > 25 ||
                injuryReserveLimit < 0 || injuryReserveLimit > 10)
            {
                TempData["RosterSettingsError"] = "Invalid roster settings.";
                return RedirectToAction("Settings", new { id = league.LeagueId });
            }

            league.StarterCount = starterCount;
            league.RosterLimit = rosterLimit;
            league.InjuryReserveLimit = injuryReserveLimit;

            await _db.SaveChangesAsync();

            TempData["RosterSettingsSaved"] = "Roster settings updated!";
            return RedirectToAction("Settings", new { id = league.LeagueId });
        }

        [HttpGet]
        public async Task<IActionResult> TransferOwnership(int leagueId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var league = await _db.Leagues
                .Include(l => l.Members).ThenInclude(m => m.User)
                .FirstOrDefaultAsync(l => l.LeagueId == leagueId);

            if (league == null || league.OwnerId != userId)
                return Forbid();

            return View(league); // This expects /Views/League/TransferOwnership.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Invite(int leagueId, string usernameOrEmail)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.UserName == usernameOrEmail || u.Email == usernameOrEmail);

            if (user == null)
            {
                TempData["InviteResult"] = "User not found.";
                return RedirectToAction("Settings", new { id = leagueId });
            }

            var alreadyMember = await _db.LeagueMembers
                .AnyAsync(lm => lm.LeagueId == leagueId && lm.UserId == user.Id);

            if (alreadyMember)
            {
                TempData["InviteResult"] = "User is already a member.";
            }
            else
            {
                _db.LeagueInvitations.Add(new LeagueInvitation
                {
                    LeagueId = leagueId,
                    UserId = user.Id,
                    SentAt = DateTime.UtcNow
                });

                await _db.SaveChangesAsync();
                TempData["InviteResult"] = "Invitation sent.";
            }

            return RedirectToAction("Settings", new { id = leagueId });
        }

        public async Task<IActionResult> Invitations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var invites = await _db.LeagueInvitations
                .Include(i => i.League)
                .Where(i => i.UserId == userId)
                .ToListAsync();

            return View(invites);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptInvite(int invitationId)
        {
            var invite = await _db.LeagueInvitations
                .FirstOrDefaultAsync(i => i.LeagueInvitationId == invitationId);

            if (invite == null)
                return NotFound();

            _db.LeagueMembers.Add(new LeagueMember
            {
                LeagueId = invite.LeagueId,
                UserId = invite.UserId
            });

            _db.LeagueInvitations.Remove(invite);
            await _db.SaveChangesAsync();

            // Redirect to team creation if team doesn't exist
            bool hasTeam = await _db.Teams.AnyAsync(t => t.LeagueId == invite.LeagueId && t.OwnerId == invite.UserId);
            if (!hasTeam)
            {
                return RedirectToAction("Create", "Team", new { leagueId = invite.LeagueId });
            }

            return RedirectToAction("Notifications", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestOwnershipTransfer(int leagueId, string newOwnerId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var league = await _db.Leagues.FindAsync(leagueId);
            if (league == null || league.OwnerId != userId)
                return Forbid();

            var exists = await _db.LeagueOwnershipTransfers
                .AnyAsync(r => r.LeagueId == leagueId && r.NewOwnerId == newOwnerId);

            if (!exists)
            {
                var transfer = new LeagueOwnershipTransfer
                {
                    LeagueId = leagueId,
                    NewOwnerId = newOwnerId
                };
                _db.LeagueOwnershipTransfers.Add(transfer);
                await _db.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Ownership transfer request sent.";
            return RedirectToAction("Settings", new { id = leagueId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptOwnershipTransfer(int transferId)
        {
            var transfer = await _db.LeagueOwnershipTransfers
                .Include(t => t.League)
                .FirstOrDefaultAsync(t => t.LeagueOwnershipTransferId == transferId);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (transfer == null || transfer.NewOwnerId != userId)
                return Forbid();

            transfer.League.OwnerId = userId;
            _db.LeagueOwnershipTransfers.Remove(transfer);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"You are now the owner of {transfer.League.Name}.";
            return RedirectToAction("View", "League", new { id = transfer.LeagueId });
        }

        [HttpPost]
        public async Task<IActionResult> LeaveLeague(int leagueId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var league = await _db.Leagues
                .Include(l => l.Members)
                .FirstOrDefaultAsync(l => l.LeagueId == leagueId);

            if (league == null)
                return NotFound();

            if (league.OwnerId == userId)
            {
                TempData["ErrorMessage"] = "You must transfer ownership before leaving the league.";
                return RedirectToAction("View", "League", new { id = league.LeagueId });
            }

            var membership = await _db.LeagueMembers
                .FirstOrDefaultAsync(m => m.LeagueId == leagueId && m.UserId == userId);

            if (membership == null)
                return NotFound();

            _db.LeagueMembers.Remove(membership);
            await _db.SaveChangesAsync();

            var team = await _db.Teams.FirstOrDefaultAsync(t => t.LeagueId == leagueId && t.OwnerId == userId);
            if (team != null)
            {
                var teamPlayers = _db.TeamPlayers.Where(tp => tp.TeamId == team.TeamId);
                _db.TeamPlayers.RemoveRange(teamPlayers);
                _db.Teams.Remove(team);
                await _db.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "You have left the league.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLeague(int leagueId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var league = await _db.Leagues
                .Include(l => l.Members)
                .Include(l => l.Teams)
                .FirstOrDefaultAsync(l => l.LeagueId == leagueId && l.OwnerId == userId);

            if (league == null)
                return NotFound();

            // Cascade delete members and teams
            var allTeamPlayers = league.Teams
                ?.SelectMany(t => t.TeamPlayers ?? new List<TeamPlayer>())
                .ToList();

            if (allTeamPlayers != null && allTeamPlayers.Any())
            {
                _db.TeamPlayers.RemoveRange(allTeamPlayers);
            }

            _db.LeagueMembers.RemoveRange(league.Members);
            _db.Teams.RemoveRange(league.Teams);
            Console.WriteLine($"Deleting league: {league?.Name}, ID: {league?.LeagueId}");
            _db.Leagues.Remove(league);

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "League deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
