using fantasydg.Data;
using fantasydg.Models;
using fantasydg.Models.Repository;
using fantasydg.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace fantasydg.Controllers
{
    public class TournamentController : Controller
    {
        private readonly DGDbContext _db;
        private readonly DataService _dataService;
        private readonly DatabaseRepository _repo;
        private readonly ILogger<TournamentController> _logger;
        private readonly HttpClient _httpClient;

        public TournamentController(DGDbContext db, DataService dataService, DatabaseRepository repo, ILogger<TournamentController> logger)
        {
            _db = db;
            _dataService = dataService;
            _repo = repo;
            _logger = logger;
            _httpClient = new HttpClient();
        }

        private async Task SetUniqueTournamentsDropdown()
        {
            var tournaments = await _db.Tournaments
                .Select(t => new TournamentDropdownItem
                {
                    Id = t.Id,
                    Name = t.Name,
                    Division = t.Division,
                    Date = t.Date
                })
                .ToListAsync();

            ViewBag.Tournaments = tournaments
                .GroupBy(t => t.Name)
                .Select(g => g.OrderByDescending(t => t.Date).First())
                .OrderByDescending(t => t.Date)
                .ToList();
        }

        [HttpGet]
        public async Task<IActionResult> Input()
        {
            await SetUniqueTournamentsDropdown();

            var model = new TournamentInputView
            {
                DivisionOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "MPO", Text = "MPO" },
                    new SelectListItem { Value = "FPO", Text = "FPO" }
                }
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(TournamentInputView model)
        {
            if (!ModelState.IsValid)
            {
                model.DivisionOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "MPO", Text = "MPO" },
                    new SelectListItem { Value = "FPO", Text = "FPO" }
                };
                return View("Input", model);
            }

            // Fetch tournament metadata
            string infoUrl = $"https://www.pdga.com/apps/tournament/live-api/live_results_fetch_event?TournID={model.TournamentId}";
            string infoJson = await _httpClient.GetStringAsync(infoUrl);
            var infoData = JObject.Parse(infoJson)["data"];

            string startDateStr = infoData?["StartDate"]?.ToString();
            DateTime startDate = DateTime.TryParse(startDateStr, out var parsedDate) ? parsedDate : DateTime.MinValue;

            string tournamentName = infoData?["SimpleName"]?.ToString() ?? $"Tournament {model.TournamentId}";
            string tier = infoData?["TierPro"]?.ToString();

            // Only accept pro tournaments
            if (tier != "M" && tier != "ES")
            {
                _logger.LogWarning("Rejected tournament ID {Id}: Tier = {tier}", model.TournamentId, tier);
                ModelState.AddModelError("", $"Tournament entered was not a valid tournament.");

                model.DivisionOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "MPO", Text = "MPO" },
                    new SelectListItem { Value = "FPO", Text = "FPO" }
                };

                return View("Input", model);
            }

            var roundList = infoData?["RoundList"] as JArray;
            int roundCount = roundList?.Count ?? 0;
            double weight = 1.0 + Math.Max(0, roundCount - 3) * 0.5;

            _logger.LogInformation("Tournament ID {Id} has {Rounds} rounds. Assigned Weight: {Weight}", model.TournamentId, roundCount, weight);

            // Initiate tournament object
            var tournament = await _db.Tournaments
            .Include(t => t.RoundList)
            .ThenInclude(r => r.Players)
            .FirstOrDefaultAsync(t => t.Id == model.TournamentId && t.Division == model.Division);

            if (tournament == null)
            {
                tournament = new Tournament
                {
                    Id = model.TournamentId,
                    Date = startDate,
                    Name = tournamentName,
                    Division = model.Division,
                    Weight = weight,
                    RoundList = new List<Round>()
                };
                _db.Tournaments.Add(tournament);
                await _db.SaveChangesAsync();
            }
            else
            {
                tournament.Name = tournamentName;
                tournament.Weight = weight;
                _db.Tournaments.Update(tournament);
                await _db.SaveChangesAsync();
            }

            var roundDataMap = await _dataService.FetchRounds(model.TournamentId, model.Division);

            foreach (var kvp in roundDataMap)
            {
                int roundNumber = kvp.Key;
                var players = kvp.Value;

                if (!players.Any()) continue;

                var round = await _db.Rounds.FirstOrDefaultAsync(r =>
                    r.RoundNumber == roundNumber &&
                    r.TournamentId == tournament.Id &&
                    r.Division == model.Division);

                if (round == null)
                {
                    round = new Round
                    {
                        RoundNumber = roundNumber,
                        Exists = true,
                        TournamentId = tournament.Id,
                        Division = model.Division,
                        Tournament = tournament
                    };

                    if (tournament.RoundList == null)
                        tournament.RoundList = new List<Round>();

                    tournament.RoundList.Add(round);
                    _db.Rounds.Add(round);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    if (round.Tournament == null)
                        round.Tournament = tournament;
                }

                foreach (var player in players)
                {
                    player.RoundId = round.RoundId;

                    var existing = await _db.Players
                        .FirstOrDefaultAsync(p => p.Id == player.Id && p.RoundId == round.RoundId);

                    if (existing == null)
                    {
                        _db.Players.Add(player);
                    }
                    else
                    {
                        _db.Entry(existing).CurrentValues.SetValues(player);
                    }
                }

                await _db.SaveChangesAsync();
            }

            var latestRound = await _db.Rounds
                .Where(r => r.TournamentId == tournament.Id && r.Division == model.Division)
                .OrderByDescending(r => r.RoundNumber)
                .Select(r => r.RoundNumber)
                .FirstOrDefaultAsync();

            return RedirectToAction("TournamentView", new { name = tournament.Name, division = model.Division, round = latestRound });
        }

        [HttpGet]
        public async Task<IActionResult> TournamentView(string name, string division, int? round)
        {
            await SetUniqueTournamentsDropdown();
            ViewBag.Divisions = await _repo.GetDistinctDivisionsAsync();
            var players = await _repo.GetFilteredPlayersAsync(name, division, round);
            return View("TournamentView", players);
        }

        [HttpGet]
        public async Task<IActionResult> TournamentViewPartial(string name, string division, int? round)
        {
            var players = await _repo.GetFilteredPlayersAsync(name, division, round);
            return PartialView("PlayerView", players);
        }

        [HttpGet]
        public async Task<IActionResult> DatabaseView(string name, string division, int? round)
        {
            await SetUniqueTournamentsDropdown();
            ViewBag.Divisions = await _repo.GetDistinctDivisionsAsync();
            var players = await _repo.GetFilteredPlayersAsync(name, division, round);
            return View("DatabaseView", players);
        }

        [HttpGet]
        public async Task<IActionResult> GetRounds(string name, string division)
        {
            var rounds = await _repo.GetRoundsForTournamentAsync(name, division);
            return Json(rounds);
        }

        private async Task SetTournamentsDropdown()
        {
            var tournaments = await _db.Tournaments
                .Select(t => new TournamentDropdownItem
                {
                    Name = t.Name,
                    Division = t.Division,
                    Date = t.Date
                })
                .ToListAsync();

            ViewBag.Tournaments = tournaments
                .GroupBy(t => new { t.Name, t.Division })
                .Select(g => g.OrderByDescending(t => t.Date).First())
                .OrderByDescending(t => t.Date)
                .ToList();
        }
    }
}
