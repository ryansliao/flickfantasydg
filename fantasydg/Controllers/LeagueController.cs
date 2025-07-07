using fantasydg.Data;
using fantasydg.Models;
using fantasydg.Models.Repository;
using fantasydg.Models.ViewModels;
using fantasydg.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Protocol.Core.Types;
using System.Reflection;
using System.Security.Claims;

namespace fantasydg.Controllers
{
    [Authorize]
    public class LeagueController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly LeagueService _leagueService;
        private readonly PlayerService _playerService;
        private readonly DatabaseRepository _repository;

        public LeagueController(ApplicationDbContext db, LeagueService leagueService, PlayerService playerService, DatabaseRepository repository)
        {
            _db = db;
            _leagueService = leagueService;
            _playerService = playerService;
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

            foreach (var kvp in ModelState)
            {
                foreach (var error in kvp.Value.Errors)
                {
                    Console.WriteLine($"Model error in '{kvp.Key}': {error.ErrorMessage}");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("Create", league);
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

        // League View
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
            ViewBag.LeagueId = league.LeagueId;
            ViewBag.Standings = await GetStandings(id);
            ViewBag.ScoringMode = league.LeagueScoringMode;

            return View("LeagueView", league);
        }

        // Calculate total fantasy points for a team
        private double CalculateTeamPoints(
            League league,
            Team team,
            List<Tournament> tournaments,
            Dictionary<(int TournamentId, string Division), double> leagueWeights)
        {
            double total = 0;

            foreach (var tournament in tournaments)
            {
                leagueWeights.TryGetValue((tournament.Id, tournament.Division), out double weight);
                if (weight == 0) weight = 1;

                total += league.LeagueScoringMode switch
                {
                    League.ScoringMode.TotalPoints =>
                        GetTeamScoreForTournament(league, team, tournament) * weight,

                    League.ScoringMode.WinsPerTournament =>
                        GetWinScoreForTeam(league, team, tournament) * weight,

                    _ => 0
                };
            }

            return total;
        }

        // Calculate tournament fantasy points for a team
        private double GetTeamScoreForTournament(League league, Team team, Tournament tournament)
        {
            var lockedStarters = team.TeamPlayerTournaments
                .Where(tpt => tpt.TournamentId == tournament.Id && tpt.IsLocked && tpt.Status == nameof(RosterStatus.Starter))
                .Select(tpt => tpt.Player.PlayerTournaments
                    .FirstOrDefault(pt => pt.TournamentId == tpt.TournamentId && pt.PDGANumber == tpt.PDGANumber))
                .Where(pt => pt != null)
                .ToList();

            return lockedStarters.Sum(pt => _playerService.CalculatePoints(league, pt, tournament));
        }

        // Calculate tournament wins based on fantasy points for a team
        private double GetWinScoreForTeam(League league, Team team, Tournament tournament)
        {
            var teamScores = league.Teams.ToDictionary(
                otherTeam => otherTeam.TeamId,
                otherTeam =>
                {
                    return otherTeam.TeamPlayerTournaments
                        .Where(tpt => tpt.TournamentId == tournament.Id && tpt.IsLocked && tpt.Status == nameof(RosterStatus.Starter))
                        .Select(tpt => tpt.Player.PlayerTournaments
                            .FirstOrDefault(pt => pt.TournamentId == tpt.TournamentId && pt.PDGANumber == tpt.PDGANumber))
                        .Where(pt => pt != null)
                        .Sum(pt => _playerService.CalculatePoints(league, pt, tournament));
                });

            if (!teamScores.TryGetValue(team.TeamId, out var thisScore))
                return 0;

            return teamScores
                .Where(kvp => kvp.Key != team.TeamId && kvp.Value < thisScore)
                .Count();
        }

        // Calculate league standings
        private async Task<List<LeagueStandingsViewModel>> GetStandings(int leagueId)
        {
            var league = await _db.Leagues
                .Include(l => l.Teams).ThenInclude(t => t.Owner)
                .Include(l => l.Teams).ThenInclude(t => t.TeamPlayerTournaments)
                    .ThenInclude(tpt => tpt.Player)
                    .ThenInclude(p => p.PlayerTournaments)
                .FirstOrDefaultAsync(l => l.LeagueId == leagueId);

            if (league == null) return new List<LeagueStandingsViewModel>();

            var tournaments = await _db.Tournaments
                .Where(t => (league.IncludeMPO && t.Division == "MPO") || (league.IncludeFPO && t.Division == "FPO"))
                .ToListAsync();

            var leagueWeights = await _db.LeagueTournaments
                .Where(lt => lt.LeagueId == leagueId)
                .ToDictionaryAsync(
                    lt => (lt.TournamentId, lt.Division),
                    lt => lt.Weight
                );

            var teamScores = new Dictionary<int, double>();
            var teamWins = new Dictionary<int, int>();

            foreach (var team in league.Teams)
            {
                double totalPoints = 0;
                int totalWins = 0;

                foreach (var tournament in tournaments)
                {
                    leagueWeights.TryGetValue((tournament.Id, tournament.Division), out double weight);
                    if (weight == 0) weight = 1;

                    double score = GetTeamScoreForTournament(league, team, tournament);
                    double wins = GetWinScoreForTeam(league, team, tournament);

                    totalPoints += score * weight;
                    totalWins += (int)wins;
                }

                teamScores[team.TeamId] = totalPoints;
                teamWins[team.TeamId] = totalWins;

                team.Points = totalPoints;
            }

            await _db.SaveChangesAsync();

            var sortedTeams = league.LeagueScoringMode == League.ScoringMode.WinsPerTournament
                ? league.Teams.OrderByDescending(t => teamWins[t.TeamId])
                : league.Teams.OrderByDescending(t => teamScores[t.TeamId]);

            return sortedTeams.Select((team, index) => new LeagueStandingsViewModel
            {
                Placement = index + 1,
                MemberName = team.Owner?.UserName ?? "Unknown",
                TeamName = team.Name,
                FantasyPoints = teamScores[team.TeamId],
                Wins = teamWins[team.TeamId]
            }).ToList();
        }


        // Leaderboard View
        [HttpGet]
        public async Task<IActionResult> LeagueLeaderboardView(int leagueId, string? division = null)
        {
            var league = await _db.Leagues
                .Include(l => l.Teams).ThenInclude(t => t.Owner)
                .Include(l => l.Teams).ThenInclude(t => t.TeamPlayerTournaments)
                    .ThenInclude(tpt => tpt.Player)
                    .ThenInclude(p => p.PlayerTournaments)
                .FirstOrDefaultAsync(l => l.LeagueId == leagueId);

            if (league == null) return NotFound();

            var leagueTournamentIds = await _db.TeamPlayerTournaments
                .Where(tpt => tpt.Team.LeagueId == leagueId)
                .Select(tpt => tpt.TournamentId)
                .Distinct()
                .ToListAsync();

            var divisions = await _db.PlayerTournaments
                .Where(pt => leagueTournamentIds.Contains(pt.TournamentId) && pt.Division != null)
                .Select(pt => pt.Division)
                .Distinct()
                .OrderByDescending(d => d)
                .ToListAsync();

            division ??= divisions.Contains("MPO") ? "MPO" : divisions.FirstOrDefault();

            var enabledDivisions = new List<string>();
            if (league.IncludeMPO) enabledDivisions.Add("MPO");
            if (league.IncludeFPO) enabledDivisions.Add("FPO");

            var tournaments = await _db.Tournaments
                .Where(t => enabledDivisions.Contains(t.Division))
                .OrderBy(t => t.StartDate)
                .ToListAsync();

            tournaments = tournaments.DistinctBy(t => t.Id).ToList();

            var leagueWeights = await _db.LeagueTournaments
                .Where(lt => lt.LeagueId == leagueId)
                .ToDictionaryAsync(
                    lt => (lt.TournamentId, lt.Division),
                    lt => lt.Weight
                );

            var model = new LeagueLeaderboardViewModel
            {
                League = league,
                Tournaments = tournaments,
                Teams = new List<LeagueLeaderboardViewModel.TeamRow>()
            };

            foreach (var team in league.Teams)
            {
                var row = new LeagueLeaderboardViewModel.TeamRow
                {
                    TeamName = team.Name,
                    OwnerName = team.Owner?.UserName ?? "Unknown",
                    PointsByTournament = new Dictionary<int, double>(),
                    WinsByTournament = new Dictionary<int, int>()
                };

                foreach (var tournament in tournaments)
                {
                    leagueWeights.TryGetValue((tournament.Id, tournament.Division), out var weight);
                    if (weight == 0) weight = 1;

                    // Always calculate raw points and raw wins
                    double rawPoints = GetTeamScoreForTournament(league, team, tournament);
                    double rawWins = GetWinScoreForTeam(league, team, tournament);

                    // Assign both regardless of scoring mode
                    row.PointsByTournament[tournament.Id] = rawPoints * weight;
                    row.WinsByTournament[tournament.Id] = (int)rawWins;
                }

                row.TotalPoints = row.PointsByTournament.Values.Sum();
                row.TotalWins = row.WinsByTournament.Values.Sum();
                model.Teams.Add(row);
            }

            ViewBag.NoFantasyPoints = !model.Teams.Any(tr => tr.PointsByTournament.Any());
            ViewBag.LeagueName = league.Name;
            ViewBag.LeagueId = league.LeagueId;
            ViewBag.TeamId = league.Teams.FirstOrDefault(t => t.OwnerId == User.FindFirstValue(ClaimTypes.NameIdentifier))?.TeamId;
            ViewBag.SelectedDivision = division;
            ViewBag.Divisions = divisions;

            return View("~/Views/Leaderboard/LeagueLeaderboardView.cshtml", model);
        }

        // Tournament Results View
        [HttpGet]
        public async Task<IActionResult> TeamTournamentResultsView(int leagueId, int? tournamentId = null, string? division = null, int? round = null)
        {
            // Load league and teams
            var league = await _db.Leagues
                .Include(l => l.Teams)
                .ThenInclude(t => t.TeamPlayers)
                .FirstOrDefaultAsync(l => l.LeagueId == leagueId);
            if (league == null) return NotFound();

            // Load tournaments and set default selection
            var allTournaments = await _repository.GetAllTournamentsAsync();
            var orderedTournaments = allTournaments.OrderByDescending(t => t.StartDate).ToList();
            ViewBag.Tournaments = orderedTournaments;

            tournamentId ??= orderedTournaments.FirstOrDefault()?.Id;
            var selectedTournament = orderedTournaments.FirstOrDefault(t => t.Id == tournamentId);
            if (selectedTournament == null) return NotFound();

            ViewBag.SelectedTournamentId = selectedTournament.Id;

            // Load divisions and set default
            var divisions = await _repository.GetDivisionsForTournamentAsync(selectedTournament.Id);
            divisions = divisions
                .Where(d => (d == "MPO" && league.IncludeMPO) || (d == "FPO" && league.IncludeFPO))
                .OrderByDescending(d => d)
                .ToList();

            ViewBag.Divisions = divisions;
            division ??= divisions.Contains("MPO") ? "MPO" : divisions.FirstOrDefault();
            ViewBag.SelectedDivision = division;

            // Get team given league, owner
            var teamId = await _db.Teams
                .Where(t => t.LeagueId == leagueId && t.OwnerId == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .Select(t => (int?)t.TeamId)
                .FirstOrDefaultAsync() ?? 0;

            // Get all assigned PDGA numbers in the league
            var assignedPDGAs = await _db.TeamPlayers
                .Where(tp => tp.LeagueId == leagueId)
                .Select(tp => tp.PDGANumber)
                .ToListAsync();

            // Get all rostered players for selected tournament and division
            var allRosteredPlayers = await _db.TeamPlayerTournaments
                .Where(tpt =>
                    tpt.TournamentId == selectedTournament.Id &&
                    tpt.IsLocked &&
                    assignedPDGAs.Contains(tpt.PDGANumber) &&
                    tpt.Division == division)
                .Include(tpt => tpt.Player)
                    .ThenInclude(p => p.PlayerTournaments)
                .ToListAsync();

            // Sort players
            var playerTeamMap = await _db.TeamPlayers
                .Where(tp => tp.LeagueId == leagueId)
                .ToDictionaryAsync(tp => tp.PDGANumber, tp => tp.Team.Name);

            allRosteredPlayers = allRosteredPlayers
                .OrderBy(tpt => playerTeamMap.TryGetValue(tpt.PDGANumber, out var teamName) ? teamName : "")
                .ThenBy(tpt => tpt.Status == "Starter" ? 0 : (tpt.Status == "Bench" ? 1 : 2))
                .ThenBy(tpt => tpt.Player?.PlayerTournaments?.FirstOrDefault(pt => pt.TournamentId == tpt.TournamentId)?.Place ?? int.MaxValue)
                .ToList();

            // Ensure fantasy points are up to date
            await _leagueService.UpdateFantasyPointsForLeagueAsync(leagueId);
            var fantasyMap = await _leagueService.GetFantasyPointsMapAsync(leagueId);

            // Prepare view model
            var model = new LeagueTournamentPlayersViewModel
            {
                League = league,
                TeamPlayerTournaments = allRosteredPlayers,
                Tournament = selectedTournament
            };

            // ViewBag values
            ViewBag.LeagueId = league.LeagueId;
            ViewBag.LeagueName = league.Name;
            ViewBag.TeamId = teamId;
            ViewBag.SelectedDivision = division;
            ViewBag.FantasyMap = fantasyMap;
            ViewBag.PlayerTeamMap = playerTeamMap;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("~/Views/Tournaments/TeamPlayersTable.cshtml", model.TeamPlayerTournaments);

            return View("~/Views/Tournaments/TeamTournamentResultsView.cshtml", model);
        }

        // Available Players View
        [HttpGet]
        public async Task<IActionResult> Players(int leagueId)
        {
            var league = await _db.Leagues
                .Include(l => l.Teams)
                .ThenInclude(t => t.TeamPlayers)
                .FirstOrDefaultAsync(l => l.LeagueId == leagueId);

            if (league == null) return NotFound();

            await _playerService.UpdateFantasyPoints(leagueId);
            var assignedPDGANumbers = await GetAssignedPDGAs(leagueId);
            var allowedDivisions = GetEnabledDivisions(league);

            var allPlayerTournaments = await _db.PlayerTournaments
                .Include(pt => pt.Player)
                .Include(pt => pt.Tournament)
                .Where(pt =>
                    pt.Tournament != null &&
                    pt.Player != null &&
                    allowedDivisions.Contains(pt.Tournament.Division))
                .ToListAsync();

            var playersWithSeasonStats = allPlayerTournaments
                .GroupBy(pt => pt.PDGANumber)
                .Select(g =>
                {
                    var first = g.First();
                    return new PlayerSeasonStatsViewModel
                    {
                        Player = first.Player,
                        PDGANumber = (int)g.Key,
                        FantasyPoints = g.Sum(pt => pt.FantasyPoints),
                        Place = g.Average(pt => pt.Place),
                        Fairway = g.Average(pt => pt.Fairway),
                        C1InReg = g.Average(pt => pt.C1InReg),
                        C2InReg = g.Average(pt => pt.C2InReg),
                        Parked = g.Average(pt => pt.Parked),
                        Scramble = g.Average(pt => pt.Scramble),
                        C1Putting = g.Average(pt => pt.C1Putting),
                        C1xPutting = g.Average(pt => pt.C1xPutting),
                        C2Putting = g.Average(pt => pt.C2Putting),
                        ObRate = g.Average(pt => pt.ObRate),
                        Par = g.Average(pt => pt.Par),
                        Birdie = g.Average(pt => pt.Birdie),
                        BirdieMinus = g.Average(pt => pt.BirdieMinus),
                        EagleMinus = g.Average(pt => pt.EagleMinus),
                        BogeyPlus = g.Average(pt => pt.BogeyPlus),
                        DoubleBogeyPlus = g.Average(pt => pt.DoubleBogeyPlus),
                        TotalPuttDistance = g.Sum(pt => pt.TotalPuttDistance),
                        AvgPuttDistance = g.Average(pt => pt.AvgPuttDistance),
                        LongThrowIn = g.Sum(pt => pt.LongThrowIn),
                        StrokesGainedTotal = g.Sum(pt => pt.StrokesGainedTotal),
                        StrokesGainedPutting = g.Sum(pt => pt.StrokesGainedPutting),
                        StrokesGainedTeeToGreen = g.Sum(pt => pt.StrokesGainedTeeToGreen),
                        StrokesGainedC1xPutting = g.Sum(pt => pt.StrokesGainedC1xPutting),
                        StrokesGainedC2Putting = g.Sum(pt => pt.StrokesGainedC2Putting)
                    };
                })
                .OrderByDescending(p => p.FantasyPoints)
                .ToList();

            ViewBag.LeagueId = leagueId;
            ViewBag.LeagueName = league.Name;
            ViewBag.TeamId = await GetUserTeamId(leagueId);
            ViewBag.AssignedPDGANumbers = assignedPDGANumbers;
            ViewBag.FantasyMap = await _leagueService.GetFantasyPointsMapAsync(leagueId);

            var model = new LeagueAvailablePlayersViewModel
            {
                League = league,
                Players = playersWithSeasonStats
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("~/Views/Players/PlayersTable.cshtml", playersWithSeasonStats);

            return View("~/Views/Players/LeaguePlayersView.cshtml", model);
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

            var tournaments = await _db.Tournaments
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();

            var leagueTournaments = await _db.LeagueTournaments
                .Where(lt => lt.LeagueId == id)
                .ToListAsync();

            // 💡 Make sure every tournament shows, even without LeagueTournament entry
            var tournamentWeights = tournaments
                .Where(t => !string.IsNullOrEmpty(t.Division))
                .Select(t => new
                {
                    Tournament = t,
                    Division = t.Division,
                    Weight = leagueTournaments
                        .FirstOrDefault(lt => lt.TournamentId == t.Id && lt.Division == t.Division)?.Weight ?? 1
                })
                .ToList();

            ViewBag.TournamentWeights = tournamentWeights;
            ViewBag.LeagueId = league.LeagueId;
            ViewBag.LeagueName = league.Name;

            return View(league);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateActiveTournaments(int id)
        {
            var nowPT = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));

            var db = HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var dataService = HttpContext.RequestServices.GetRequiredService<DataService>();

            var service = new TournamentService(HttpContext.RequestServices, NullLogger<TournamentService>.Instance);
            var method = typeof(TournamentService).GetMethod("UpdateActiveTournamentsAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            if (method != null)
                await (Task)method.Invoke(service, new object[] { nowPT, db, dataService })!;

            TempData["ActiveTournamentsUpdated"] = "✅ All active tournaments were updated successfully.";
            return RedirectToAction("Settings", new { id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DiscoverNewTournaments(int id)
        {
            var nowPT = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));

            var db = HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var dataService = HttpContext.RequestServices.GetRequiredService<DataService>();
            var httpClient = HttpContext.RequestServices.GetRequiredService<HttpClient>();

            var service = new TournamentService(HttpContext.RequestServices, NullLogger<TournamentService>.Instance);
            var method = typeof(TournamentService).GetMethod("DiscoverNewTournamentsAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            if (method != null)
                await (Task)method.Invoke(service, new object[] { nowPT, db, dataService, httpClient })!;

            TempData["NewTournamentsDiscovered"] = "🧭 New Elite Series and Major tournaments discovered and added!";
            return RedirectToAction("Settings", new { id });
        }

        // Change included divisions in league settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveIncludedDivisions(IFormCollection form)
        {
            if (!int.TryParse(form["LeagueId"], out int leagueId))
                return BadRequest("Missing or invalid LeagueId");

            bool includeMPO = bool.TryParse(form["IncludeMPO"], out var mpo) && mpo;
            bool includeFPO = bool.TryParse(form["IncludeFPO"], out var fpo) && fpo;

            var league = await _db.Leagues.FindAsync(leagueId);
            if (league == null) return NotFound();

            league.IncludeMPO = includeMPO;
            league.IncludeFPO = includeFPO;

            await _db.SaveChangesAsync();
            TempData["IncludedDivisionsSaved"] = "Division settings updated!";
            return RedirectToAction("Settings", new { id = leagueId });
        }

        // Change scoring mode in league settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveScoringMode(int id, League.ScoringMode LeagueScoringMode)
        {
            var league = await _db.Leagues.FindAsync(id);
            if (league == null) return NotFound();

            league.LeagueScoringMode = LeagueScoringMode;
            await _db.SaveChangesAsync();

            TempData["ScoringModeSaved"] = "Scoring mode updated!";
            return RedirectToAction("Settings", new { id = league.LeagueId });
        }

        // Change tournament weights in league settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTournamentWeights(int leagueId, Dictionary<string, double> Weights)
        {
            foreach (var pair in Weights)
            {
                var keyParts = pair.Key.Split('|');
                if (keyParts.Length != 2) continue;

                if (!int.TryParse(keyParts[0], out int tournamentId)) continue;
                string division = keyParts[1];

                var leagueTournament = await _db.LeagueTournaments
                    .FirstOrDefaultAsync(lt => lt.LeagueId == leagueId && lt.TournamentId == tournamentId && lt.Division == division);

                if (leagueTournament != null)
                {
                    leagueTournament.Weight = pair.Value;
                }
                else
                {
                    // Add new entry if it doesn't exist
                    _db.LeagueTournaments.Add(new LeagueTournament
                    {
                        LeagueId = leagueId,
                        TournamentId = tournamentId,
                        Division = division,
                        Weight = pair.Value
                    });
                }
            }

            await _db.SaveChangesAsync();

            TempData["TournamentWeightsSaved"] = "Tournament weights updated!";
            return RedirectToAction("Settings", new { id = leagueId });
        }

        // Change scoring settings in league settings
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveScoringSettings(League model, int id)
        {
            var league = await _db.Leagues.FindAsync(model.LeagueId);
            if (league == null) return NotFound();

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

        // Save roster size in league settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRosterSettings(int leagueId, int starterCount, int benchCount)
        {
            var league = await _db.Leagues.FindAsync(leagueId);
            if (league == null) return NotFound();

            if (starterCount < 3 || starterCount > 25 ||
                benchCount < 3 || benchCount > 25 )
            {
                TempData["RosterSettingsError"] = "Invalid roster settings.";
                return RedirectToAction("Settings", new { id = league.LeagueId });
            }

            league.StarterCount = starterCount;
            league.BenchCount = benchCount;

            await _db.SaveChangesAsync();

            TempData["RosterSettingsSaved"] = "Roster settings updated!";
            return RedirectToAction("Settings", new { id = league.LeagueId });
        }

        // Change league name in league settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveLeagueName(int LeagueId, string LeagueName)
        {
            var league = await _db.Leagues.FindAsync(LeagueId);
            if (league == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(LeagueName) && LeagueName.Length <= 50)
            {
                league.Name = LeagueName;
                await _db.SaveChangesAsync();
                TempData["LeagueNameSaved"] = "League name updated!";
            }
            else
            {
                TempData["LeagueNameSaved"] = "Invalid league name.";
            }

            return RedirectToAction("Settings", new { id = LeagueId });
        }

        // Changes owner of league
        [HttpGet]
        public async Task<IActionResult> TransferOwnership(int leagueId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var league = await _db.Leagues
                .Include(l => l.Members).ThenInclude(m => m.User)
                .FirstOrDefaultAsync(l => l.LeagueId == leagueId);

            if (league == null || league.OwnerId != userId)
                return Forbid();

            return View(league);
        }

        // Change league owner in league settings
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

        // Accept ownership of league from another user
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

        // Invite new users to league in league settings
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

            var isMember = await _db.LeagueMembers
                .AnyAsync(lm => lm.LeagueId == leagueId && lm.UserId == user.Id);

            var hasTeam = await _db.Teams
                .AnyAsync(t => t.LeagueId == leagueId && t.OwnerId == user.Id);

            if (isMember && hasTeam)
            {
                TempData["InviteResult"] = "User is already a member and has a team.";
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

        // Shows invitations to leagues
        [HttpGet]
        public async Task<IActionResult> Invitations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var invites = await _db.LeagueInvitations
                .Include(i => i.League)
                .Where(i => i.UserId == userId)
                .ToListAsync();

            return View(invites);
        }

        // Accept invite to league
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

        // Leave league
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

        // Delete league
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

        private async Task<List<int>> GetAssignedPDGAs(int leagueId)
        {
            return await _db.TeamPlayers
                .Where(tp => tp.LeagueId == leagueId)
                .Select(tp => tp.PDGANumber)
                .ToListAsync();
        }

        private async Task<int?> GetUserTeamId(int leagueId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _db.Teams
                .Where(t => t.LeagueId == leagueId && t.OwnerId == userId)
                .Select(t => (int?)t.TeamId)
                .FirstOrDefaultAsync();
        }

        private List<string> GetEnabledDivisions(League league)
        {
            var divisions = new List<string>();
            if (league.IncludeMPO) divisions.Add("MPO");
            if (league.IncludeFPO) divisions.Add("FPO");
            return divisions;
        }

    }
}
