using fantasydg.Data;
using fantasydg.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using flickfantasydg.Models;

namespace fantasydg.Controllers
{
    [Authorize]
    public class LeagueController : Controller
    {
        private readonly ApplicationDbContext _db;

        public LeagueController(ApplicationDbContext db)
        {
            _db = db;
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

        // View and manage settings for a specific league
        [Authorize]
        public async Task<IActionResult> Settings(int id)
        {
            var league = await _db.Leagues
                .Include(l => l.Owner)
                .Include(l => l.Members).ThenInclude(m => m.User)
                .FirstOrDefaultAsync(l => l.LeagueId == id);

            if (league == null)
                return NotFound();

            var userId = User?.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (league.OwnerId != userId)
            {
                return RedirectToAction("View", "League", new { id = league.LeagueId });
            }

            return View(league);
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

            var standings = await _db.Teams
                .Where(t => t.LeagueId == id)
                .Include(t => t.Owner)
                .Include(t => t.TeamPlayers)
                    .ThenInclude(tp => tp.Player)
                    .ThenInclude(p => p.PlayerTournaments)
                .ToListAsync();

            var ordered = standings
                .Select(t => new
                {
                    Team = t,
                    OwnerName = t.Owner.UserName,
                    Points = t.TeamPlayers
                        .SelectMany(tp => tp.Player.PlayerTournaments)
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
                            pt.LongThrowIn * league.LongThrowInWeight
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

            ViewBag.Standings = ordered;

            return View("LeagueView", league);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateScoringSettings(
            int leagueId,
            double PlacementWeight,
            double FairwayWeight, 
            double C1InRegWeight, 
            double C2InRegWeight, 
            double ParkedWeight, 
            double ScrambleWeight,
            double C1PuttWeight,
            double C1xPuttWeight,
            double C2PuttWeight,
            double OBWeight,
            double ParWeight,
            double BirdieWeight,
            double BirdieMinusWeight,
            double EagleMinusWeight,
            double BogeyPlusWeight,
            double DoubleBogeyPlusWeight,
            double TotalPuttDistWeight,
            double AvgPuttDistWeight,
            double LongThrowInWeight
            )
        {
            var league = await _db.Leagues.FindAsync(leagueId);
            if (league == null) return NotFound();
            league.PlacementWeight = PlacementWeight;
            league.FairwayWeight = FairwayWeight;
            league.C1InRegWeight = C1InRegWeight;
            league.C2InRegWeight = C2InRegWeight;
            league.ParkedWeight = ParkedWeight;
            league.ScrambleWeight = ScrambleWeight;
            league.C1PuttWeight = C1PuttWeight;
            league.C1xPuttWeight = C1xPuttWeight;
            league.C2PuttWeight = C2PuttWeight;
            league.OBWeight = OBWeight;
            league.ParWeight = ParWeight;
            league.BirdieWeight = BirdieWeight;
            league.BirdieMinusWeight = BirdieMinusWeight;
            league.EagleMinusWeight = EagleMinusWeight;
            league.BogeyPlusWeight = BogeyPlusWeight;
            league.DoubleBogeyPlusWeight = DoubleBogeyPlusWeight;
            league.TotalPuttDistWeight = TotalPuttDistWeight;
            league.AvgPuttDistWeight = AvgPuttDistWeight;
            league.LongThrowInWeight = LongThrowInWeight;

            await _db.SaveChangesAsync();
            TempData["ScoringSaved"] = "Scoring settings updated!";
            return RedirectToAction("Settings", new { id = leagueId });
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
