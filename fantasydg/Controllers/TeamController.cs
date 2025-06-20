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
        public async Task<IActionResult> View(int teamId, int? viewTeamId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var mainTeam = await _db.Teams
                .Include(t => t.League)
                .FirstOrDefaultAsync(t => t.TeamId == teamId && t.OwnerId == userId);

            if (mainTeam == null) return NotFound();

            var leagueId = mainTeam.LeagueId;
            var allTeams = await _db.Teams
                .Where(t => t.LeagueId == leagueId)
                .ToListAsync();

            var selectedTeamId = viewTeamId ?? teamId;

            var selectedTeam = await _db.Teams
                .Include(t => t.TeamPlayers).ThenInclude(tp => tp.Player)
                .ThenInclude(p => p.PlayerTournaments)
                .FirstOrDefaultAsync(t => t.TeamId == selectedTeamId);

            if (selectedTeam == null) return NotFound();

            // Prepare ViewBags
            ViewBag.LeagueName = mainTeam.League?.Name;
            ViewBag.TeamId = teamId;                   // user's team ID
            ViewBag.SelectedTeamId = selectedTeamId;   // current team being viewed
            ViewBag.OtherTeams = allTeams;             // for dropdown

            // Load all tournaments and locked status
            var allTournaments = await _repository.GetAllTournamentsAsync();
            var lockedTournamentIds = await _db.TeamPlayerTournaments
                .Where(tpt => tpt.TeamId == selectedTeamId && tpt.IsLocked)
                .Select(tpt => tpt.TournamentId)
                .Distinct()
                .ToListAsync();

            ViewBag.AllTournaments = allTournaments
                 .OrderByDescending(t => t.Date)
                 .Select(t => new TournamentLockView
                 {
                     Id = t.Id,
                     Name = t.Name,
                     Date = t.Date,
                     IsLocked = lockedTournamentIds.Contains(t.Id)
                 }).ToList();

            // Show latest locked or unlocked tournament
            var latestTournamentId = allTournaments.OrderByDescending(t => t.Date).FirstOrDefault()?.Id ?? -1;

            var lockedRoster = await _db.TeamPlayerTournaments
                .Include(tpt => tpt.Player)
                .Include(tpt => tpt.Tournament)
                .Where(tpt => tpt.TeamId == selectedTeamId && tpt.TournamentId == latestTournamentId)
                .ToListAsync();

            var roster = selectedTeam.TeamPlayers
                 .Where(tp => tp.Player != null)
                 .Select(tp => new TeamPlayerTournament
                 {
                     TeamId = tp.TeamId,
                     PDGANumber = tp.PDGANumber,
                     Player = tp.Player,
                     TournamentId = latestTournamentId,
                     Status = tp.Status.ToString(),
                     IsLocked = lockedRoster.Any(r => r.PDGANumber == tp.PDGANumber) // Flag locked players
                 }).ToList();

            var isLocked = lockedRoster.All(r => r.IsLocked); // Or use a separate field to check
            ViewBag.IsLocked = isLocked;

            return View("TeamView", new TeamViewModel
            {
                Team = selectedTeam,
                Roster = roster
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetLockOptions(int teamId)
        {
            var tournaments = await _repository.GetAllTournamentsAsync();
            var lockedIds = await _db.TeamPlayerTournaments
                .Where(tpt => tpt.TeamId == teamId && tpt.IsLocked)
                .Select(tpt => tpt.TournamentId)
                .Distinct()
                .ToListAsync();

            var results = tournaments
                .OrderByDescending(t => t.Date)
                .Select(t => new {
                    t.Id,
                    t.Name,
                    t.Date,
                    IsLocked = lockedIds.Contains(t.Id)
                });

            return Json(results);
        }

        [HttpGet]
        public async Task<IActionResult> GetStarterPreview(int teamId, int tournamentId)
        {
            var isLocked = await _db.TeamPlayerTournaments
                .AnyAsync(tpt => tpt.TeamId == teamId && tpt.TournamentId == tournamentId && tpt.IsLocked);

            List<int> tournamentPlayerIds = await _db.PlayerTournaments
                .Where(pt => pt.TournamentId == tournamentId)
                .Select(pt => pt.PDGANumber)
                .ToListAsync();

            List<(string Name, bool IsParticipating)> starters;

            if (isLocked)
            {
                // Show locked starters for this tournament
                var lockedStarters = await _db.TeamPlayerTournaments
                    .Include(tpt => tpt.Player)
                    .Where(tpt => tpt.TeamId == teamId && tpt.TournamentId == tournamentId && tpt.Status == "Starter")
                    .ToListAsync();

                starters = lockedStarters
                    .Select(tpt => (tpt.Player?.Name ?? "Unknown", tournamentPlayerIds.Contains(tpt.PDGANumber)))
                    .ToList();
            }
            else
            {
                // Show current roster starters
                var currentStarters = await _db.TeamPlayers
                    .Include(tp => tp.Player)
                    .Where(tp => tp.TeamId == teamId && tp.Status == RosterStatus.Starter)
                    .ToListAsync();

                starters = currentStarters
                    .Select(tp => (tp.Player?.Name ?? "Unknown", tournamentPlayerIds.Contains(tp.PDGANumber)))
                    .ToList();
            }

            if (!starters.Any())
                return Content("<div class='text-danger'>No starters assigned.</div>", "text/html");

            var html = "<ul class='list-group'>";
            foreach (var (name, inTournament) in starters)
            {
                var badge = inTournament
                    ? "<span class='badge bg-success ms-2'>✓ In Tournament</span>"
                    : "<span class='badge bg-danger ms-2'>✗ Not in Tournament</span>";

                html += $"<li class='list-group-item d-flex justify-content-between align-items-center'>{name}{badge}</li>";
            }
            html += "</ul>";

            return Content(html, "text/html");
        }

        [HttpGet]
        public async Task<IActionResult> Settings(int teamId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var team = await _db.Teams
                .Include(t => t.League)
                .FirstOrDefaultAsync(t => t.TeamId == teamId && t.OwnerId == userId);
            if (team == null)
                return Unauthorized();

            ViewBag.TeamId = team.TeamId;
            ViewBag.TeamName = team.Name;
            return View(team.League);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSettings(int TeamId, string Name)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var team = await _db.Teams.FirstOrDefaultAsync(t => t.TeamId == TeamId && t.OwnerId == userId);
            if (team == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(Name) || Name.Length > 20)
            {
                TempData["TeamSettingsSaved"] = "Team name must be between 1 and 20 characters.";
                return View("Settings", team);
            }

            team.Name = Name;
            await _db.SaveChangesAsync();

            TempData["TeamSettingsSaved"] = "Team name updated successfully!";
            return RedirectToAction("Settings", new { teamId = TeamId });
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
            var team = await _db.Teams.FirstOrDefaultAsync(t => t.TeamId == teamId);

            // Ensure user owns the team
            if (team == null || team.OwnerId != userId)
                return Unauthorized();

            // Prevent drop if roster is locked for this tournament
            bool isLocked = await _db.TeamPlayerTournaments
                .AnyAsync(tpt => tpt.TeamId == teamId && tpt.TournamentId == tournamentId && tpt.IsLocked);

            if (isLocked)
            {
                TempData["RosterLockError"] = "You cannot drop players from a locked roster.";
                return RedirectToAction("View", new { teamId, tournamentId });
            }

            // Drop from active roster
            var player = await _db.TeamPlayers
                .FirstOrDefaultAsync(tp => tp.TeamId == teamId && tp.PDGANumber == pdgaNumber);

            if (player != null)
            {
                _db.TeamPlayers.Remove(player);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("View", new { teamId, tournamentId, division });
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
