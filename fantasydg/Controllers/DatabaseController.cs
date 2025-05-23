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
    public class DatabaseController : Controller
    {
        private readonly DGDbContext _db;
        private readonly DataService _dataService;
        private readonly DatabaseRepository _repository;
        private readonly ILogger<DataService> _logger;

        public DatabaseController(DGDbContext db, DataService dataService, DatabaseRepository repository, ILogger<DataService> logger)
        {
            _db = db;
            _dataService = dataService;
            _repository = repository;
            _logger = logger;
        }

        public IActionResult Input()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Input(TournamentInputView model)
        {
            string[] divisions = { "MPO", "FPO" };

            foreach (var division in divisions)
            {
                await _dataService.FetchTournaments(model.TournamentId, division);
                await _dataService.FetchRounds(model.TournamentId, division);

            }

            return RedirectToAction("DatabaseView", new { name = await GetTournamentName(model.TournamentId), division = "MPO" });
        }

        // Helper method to map ID to name
        private async Task<string?> GetTournamentName(int tournamentId)
        {
            return await _db.Tournaments
                .Where(t => t.Id == tournamentId && t.Division == "MPO")
                .Select(t => t.Name)
                .FirstOrDefaultAsync();
        }

        // GET: /DatabaseView
        public async Task<IActionResult> DatabaseView(string? name = null, string? division = null, int? round = null)
        {
            var allTournaments = await _repository.GetAllTournamentsAsync();
            _logger.LogInformation("Found {Count} tournaments", allTournaments.Count);

            ViewBag.Tournaments = allTournaments;

            if (string.IsNullOrEmpty(name))
                name = allTournaments.FirstOrDefault()?.Name;

            if (!string.IsNullOrEmpty(name))
                ViewBag.Divisions = await _repository.GetDivisionsForTournamentAsync(name);
            else
                ViewBag.Divisions = new List<string>();

            if (string.IsNullOrEmpty(division))
                division = (ViewBag.Divisions as List<string>)?.FirstOrDefault();

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(division))
            {
                ViewBag.Rounds = await _repository.GetRoundsForTournamentAsync(name, division);
            }
            else
            {
                ViewBag.Rounds = new List<int>();
            }

            if (round.HasValue)
            {
                var roundScores = await _repository.GetRoundScoresAsync(name, division, round.Value);
                return View("DatabaseView", roundScores);
            }
            else
            {
                var playerTournaments = await _repository.GetPlayerTournamentsAsync(name, division);
                return View("DatabaseView", playerTournaments);
            }
        }
    }
}
