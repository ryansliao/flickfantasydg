using fantasydg.Data;
using fantasydg.Models;
using fantasydg.Models.Repository;
using fantasydg.Models.ViewModels;
using fantasydg.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using System.Security.Claims;

namespace fantasydg.Controllers
{
    public class TeamController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DatabaseRepository _repository;
        private readonly LeagueService _leagueService;

        public TeamController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, DatabaseRepository repository, LeagueService leagueService)
        {
            _db = db;
            _userManager = userManager;
            _repository = repository;
            _leagueService = leagueService;
        }

        [HttpGet]
        public IActionResult Create(int leagueId)
        {
            ViewBag.LeagueId = leagueId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name, int leagueId)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 20)
            {
                TempData["InvalidTeamName"] = "Your team name must be 20 characters or less.";
                ViewBag.LeagueId = leagueId;
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Prevent duplicate team creation
            var hasTeam = await _db.Teams.AnyAsync(t => t.LeagueId == leagueId && t.OwnerId == user.Id);
            if (hasTeam)
            {
                var existing = await _db.Teams.FirstOrDefaultAsync(t => t.LeagueId == leagueId && t.OwnerId == user.Id);
                return RedirectToAction("View", new { teamId = existing.TeamId });
            }

            var team = new Team
            {
                Name = name,
                LeagueId = leagueId,
                OwnerId = user.Id
            };

            _db.Teams.Add(team);
            await _db.SaveChangesAsync();

            return RedirectToAction("View", new { teamId = team.TeamId });
        }

        [HttpGet]
        public async Task<IActionResult> View(int teamId, int? tournamentId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var team = await _db.Teams
                .Include(t => t.League)
                .Include(t => t.TeamPlayers)
                    .ThenInclude(tp => tp.Player)
                .FirstOrDefaultAsync(t => t.TeamId == teamId && t.OwnerId == userId);

            if (team == null) return NotFound();

            ViewBag.LeagueName = team.League?.Name;
            ViewBag.TeamId = team.TeamId;

            // Load all tournaments
            var allTournaments = await _repository.GetAllTournamentsAsync();
            var orderedTournaments = allTournaments.OrderByDescending(t => t.Date).ToList();
            ViewBag.Tournaments = orderedTournaments;

            // Select the most recent tournament if none is specified
            var selectedTournament = tournamentId.HasValue
                ? orderedTournaments.FirstOrDefault(t => t.Id == tournamentId)
                : orderedTournaments.FirstOrDefault();

            if (selectedTournament == null)
            {
                return View("TeamView", new TeamViewModel { Team = team, Roster = new List<TeamPlayerTournament>() });
            }

            tournamentId = selectedTournament.Id;
            ViewBag.SelectedTournamentId = tournamentId;

            // Get available divisions
            var division = await _db.Tournaments
                .Where(t => t.Id == tournamentId)
                .Select(t => t.Division)
                .FirstOrDefaultAsync();

            var tournamentRoster = await _db.TeamPlayerTournaments
                .Include(tpt => tpt.Player)
                .Include(tpt => tpt.Tournament)
                .Where(tpt => tpt.TeamId == teamId && tpt.TournamentId == tournamentId)
                .ToListAsync();

            // fallback if no locked snapshot exists
            List<TeamPlayerTournament> roster;
            bool isLocked = false;

            if (tournamentRoster.Any())
            {
                roster = tournamentRoster;
                isLocked = tournamentRoster.All(tpt => tpt.IsLocked);
            }
            else
            {
                // fallback to current team players
                var teamPlayers = await _db.TeamPlayers
                    .Include(tp => tp.Player)
                    .ThenInclude(p => p.PlayerTournaments)
                    .Where(tp => tp.TeamId == teamId)
                    .ToListAsync();

                roster = teamPlayers
                    .Where(tp => tp.Player != null)
                    .Select(tp => new TeamPlayerTournament
                    {
                        TeamId = tp.TeamId,
                        PDGANumber = tp.PDGANumber,
                        TournamentId = tournamentId.Value,
                        Division = tp.Player.PlayerTournaments
                            .FirstOrDefault(p => p.TournamentId == tournamentId)?.Division ?? "MPO",
                        Player = tp.Player,
                        Status = tp.Status.ToString(),
                        IsLocked = false
                    }).ToList();
            }

            ViewBag.IsLocked = isLocked;

            return View("TeamView", new TeamViewModel
            {
                Team = team,
                Roster = roster
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPlayer(int teamId, int PDGANumber, int leagueId, int tournamentId, string division)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var team = await _db.Teams.FirstOrDefaultAsync(t => t.TeamId == teamId && t.OwnerId == userId);
            if (team == null) return Unauthorized();

            bool alreadyAdded = await _db.TeamPlayers
                .AnyAsync(tp => tp.TeamId == teamId && tp.PDGANumber == PDGANumber);

            if (!alreadyAdded)
            {
                _db.TeamPlayers.Add(new TeamPlayer
                {
                    TeamId = teamId,
                    PDGANumber = PDGANumber,
                    LeagueId = leagueId
                });
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("View", "Team", new { teamId, tournamentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DropPlayer(int teamId, int pdgaNumber, int tournamentId, string division)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var team = await _db.Teams
                .FirstOrDefaultAsync(t => t.TeamId == teamId && t.OwnerId == userId);
            if (team == null) return Unauthorized();

            // Fetch the record first
            var tpt = await _db.TeamPlayerTournaments.FirstOrDefaultAsync(t =>
                t.TeamId == teamId &&
                t.PDGANumber == pdgaNumber &&
                t.TournamentId == tournamentId &&
                t.Division == division);

            // Now check if it's locked
            if (tpt != null && !tpt.IsLocked)
            {
                _db.TeamPlayerTournaments.Remove(tpt);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("View", new
            {
                teamId,
                tournamentId,
                division
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockRoster(int teamId, int tournamentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var team = await _db.Teams
                .Include(t => t.TeamPlayers)
                    .ThenInclude(tp => tp.Player)
                    .ThenInclude(p => p.PlayerTournaments)
                .FirstOrDefaultAsync(t => t.TeamId == teamId && t.OwnerId == userId);

            if (team == null)
                return Unauthorized();

            // Get all starter TeamPlayers
            var starters = team.TeamPlayers.Where(tp => tp.Status == RosterStatus.Starter).ToList();

            // Check if already locked
            var alreadyLocked = await _db.TeamPlayerTournaments
                .Where(tpt => tpt.TeamId == teamId && tpt.TournamentId == tournamentId && tpt.IsLocked)
                .AnyAsync();

            if (alreadyLocked)
            {
                // Unlock: remove existing entries
                var existing = await _db.TeamPlayerTournaments
                    .Where(tpt => tpt.TeamId == teamId && tpt.TournamentId == tournamentId)
                    .ToListAsync();

                _db.TeamPlayerTournaments.RemoveRange(existing);
                await _db.SaveChangesAsync();
            }
            else
            {
                if (!starters.Any())
                {
                    TempData["RosterLockError"] = "You must assign at least one starter before locking the roster.";
                    return RedirectToAction("View", new { teamId, tournamentId });
                }

                var missingStarters = starters
                    .Where(tp => !tp.Player.PlayerTournaments.Any(pt => pt.TournamentId == tournamentId))
                    .ToList();

                if (missingStarters.Any())
                {
                    string names = string.Join(", ", missingStarters.Select(p => p.Player.Name));
                    TempData["RosterLockError"] = $"Cannot lock roster. The following starters are not in this tournament: {names}";
                    return RedirectToAction("View", new { teamId, tournamentId });
                }

                // Lock: create snapshot
                foreach (var tp in starters)
                {
                    // Try to find the matching player tournament record for this tournament
                    var pt = tp.Player.PlayerTournaments
                        .FirstOrDefault(p => p.TournamentId == tournamentId);

                    if (pt != null)
                    {
                        _db.TeamPlayerTournaments.Add(new TeamPlayerTournament
                        {
                            TeamId = teamId,
                            PDGANumber = tp.PDGANumber,
                            TournamentId = tournamentId,
                            Division = pt.Division, // ✅ Derive division from player tournament stats
                            Status = tp.Status.ToString(),
                            IsLocked = true
                        });
                    }
                }

                await _db.SaveChangesAsync();
            }

            return RedirectToAction("View", new { teamId, tournamentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var teamPlayer = await _db.TeamPlayers
                .FirstOrDefaultAsync(tp => tp.PDGANumber == id && tp.Team.OwnerId == userId);

            if (teamPlayer == null)
                return NotFound();

            if (!Enum.TryParse<RosterStatus>(request.NewStatus, out var status))
                return BadRequest();

            teamPlayer.Status = status;
            await _db.SaveChangesAsync();

            return Ok();
        }

        public class ChangeStatusRequest
        {
            public string NewStatus { get; set; }
        }
    }
}
