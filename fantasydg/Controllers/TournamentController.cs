using fantasydg.Data;
using fantasydg.Models;
using fantasydg.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace fantasydg.Controllers
{
    public class TournamentController : Controller
    {
        private readonly DGDbContext _db;
        private readonly DataService _dataService;
        private readonly ILogger<TournamentController> _logger;
        private readonly HttpClient _httpClient;

        public TournamentController(DGDbContext db, DataService dataService, ILogger<TournamentController> logger)
        {
            _db = db;
            _dataService = dataService;
            _logger = logger;
            _httpClient = new HttpClient();
        }

        [HttpGet]
        public IActionResult Input()
        {
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

            // Fetch player data
            var players = await _dataService.FetchAveragedTournamentStatsAsync(model.TournamentId, model.Division);
            _logger.LogInformation("Fetched {Count} players", players.Count);

            // Validate conflict on name/id mismatch
            var conflicting = await _db.Tournaments
                .Where(t => t.Id == model.TournamentId || t.Name == tournamentName)
                .ToListAsync();

            var conflict = conflicting.FirstOrDefault();
            if (conflict != null)
            {
                bool idMatch = conflict.Id == model.TournamentId;
                bool nameMatch = conflict.Name.Equals(tournamentName, StringComparison.OrdinalIgnoreCase);

                if (!(idMatch && nameMatch))
                {
                    _logger.LogWarning("Tournament ID/Name mismatch");
                    ModelState.AddModelError("", "Tournament ID and Name must refer to the same tournament.");
                    model.DivisionOptions = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "MPO", Text = "MPO" },
                        new SelectListItem { Value = "FPO", Text = "FPO" }
                    };
                    return View("Input", model);
                }
            }

            // Load or create tournament
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
            }

            if (players.Any())
            {
                var round = new Round
                {
                    RoundNumber = 1,
                    Exists = true,
                    TournamentId = tournament.Id,
                    Players = new List<Player>()
                };

                foreach (var p in players)
                {
                    p.Round = round;

                    var existingPlayer = await _db.Players.FindAsync(p.Id);
                    if (existingPlayer != null)
                    {
                        existingPlayer.Name = p.Name;
                        existingPlayer.Place = p.Place;
                        existingPlayer.RoundScore = p.RoundScore;
                        existingPlayer.TournamentScore = p.TournamentScore;
                        existingPlayer.Fairway = p.Fairway;
                        existingPlayer.C1InReg = p.C1InReg;
                        existingPlayer.C2InReg = p.C2InReg;
                        existingPlayer.Parked = p.Parked;
                        existingPlayer.Scramble = p.Scramble;
                        existingPlayer.C1Putting = p.C1Putting;
                        existingPlayer.C1xPutting = p.C1xPutting;
                        existingPlayer.C2Putting = p.C2Putting;
                        existingPlayer.ObRate = p.ObRate;
                        existingPlayer.BirdiePlus = p.BirdiePlus;
                        existingPlayer.DoubleBogeyPlus = p.DoubleBogeyPlus;
                        existingPlayer.BogeyPlus = p.BogeyPlus;
                        existingPlayer.Par = p.Par;
                        existingPlayer.Birdie = p.Birdie;
                        existingPlayer.EaglePlus = p.EaglePlus;
                        existingPlayer.PuttDistance = p.PuttDistance;
                    }
                    else
                    {
                        round.Players.Add(p);
                    }
                }

                tournament.RoundList.Add(round);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("DatabaseResults", new { name = tournament.Name, division = model.Division });
        }

        [HttpGet]
        public async Task<IActionResult> DatabaseResults(string name, string division)
        {
            var query = _db.Players
                .Include(p => p.Round)
                .ThenInclude(r => r.Tournament)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(p => p.Round.Tournament.Name == name);

            if (!string.IsNullOrEmpty(division))
                query = query.Where(p => p.Round.Tournament.Division == division);

            var players = await query
                .OrderBy(p => p.Round.Tournament.Date)
                .ThenBy(p => p.TournamentScore)
                .ToListAsync();

            var tournaments = await _db.Tournaments
                .Select(t => new TournamentDropdownItem
                {
                    Name = t.Name,
                    Date = t.Date
                })
                .ToListAsync();

            ViewBag.UniqueTournaments = tournaments
                .GroupBy(t => t.Name)
                .Select(g => g.OrderByDescending(t => t.Date).First())
                .OrderByDescending(t => t.Date)
                .ToList();

            ViewBag.UniqueDivisions = await _db.Tournaments
                .Select(t => t.Division)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            return View("DatabaseResults", players);
        }
    }
}
