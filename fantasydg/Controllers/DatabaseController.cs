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

        // Prompt the tournament input view
        public async Task<IActionResult> Input(TournamentInputView? model)
        {
            if (HttpContext.Request.Method == "POST" && model != null)
            {
                string[] divisions = { "MPO", "FPO" };

                // Request API endpoints for tournament URLs in each division and enters them into the database
                foreach (var division in divisions)
                {
                    await _dataService.FetchTournaments(model.TournamentId, division);
                }

                // Find the name of the tournament just entered
                var name = await GetTournamentName(model.TournamentId);
                return RedirectToAction("DatabaseView", new { name = name, division = "MPO" });
            }

            return View();
        }

        // Return the name of the tournament given its ID
        private async Task<string?> GetTournamentName(int tournamentId)
        {
            return await _db.Tournaments
                .Where(t => t.Id == tournamentId && t.Division == "MPO")
                .Select(t => t.Name)
                .FirstOrDefaultAsync();
        }

        // Access the database and populates the views
        public async Task<IActionResult> DatabaseView(string? name = null, string? division = null, int? round = null)
        {
            // Select all tournaments
            var allTournaments = await _repository.GetAllTournamentsAsync();
            ViewBag.Tournaments = allTournaments // Tournament name dropdown
                .OrderByDescending(t => t.Date)
                .ToList();

            if (string.IsNullOrEmpty(name))
                name = allTournaments.FirstOrDefault()?.Name;

            ViewBag.SelectedTournament = name; // Selected tournament dropdown item

            // Select all divisions for each tournament if it exists in database
            if (!string.IsNullOrEmpty(name))
                ViewBag.Divisions = (await _repository.GetDivisionsForTournamentAsync(name)) // Division name dropdown for existing tournaments
                    .OrderByDescending(d => d) 
                    .ToList();
            else
                ViewBag.Divisions = new List<string>(); // Division name dropdown for null or empty tournaments

            if (string.IsNullOrEmpty(division))
                division = (ViewBag.Divisions as List<string>)?.Contains("MPO") == true ? "MPO" : (ViewBag.Divisions as List<string>)?.FirstOrDefault();
            
            ViewBag.SelectedDivision = division; // Selected division dropdown item 

            // Select all existing rounds for each tournament and division
            List<int> validRounds = new();

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(division))
            {
                validRounds = await _repository.GetRoundsForTournamentAsync(name, division);
            }
            ViewBag.Rounds = validRounds; // Round number dropdown for null or empty tournaments

            if (round.HasValue && !validRounds.Contains(round.Value))
            {
                round = null; // Round dropdown reverts to "All" when the selected round doesn't exist
            }
            ViewBag.SelectedRound = round; // Selected round number dropdown item 

            // Selects RoundScore and PlayerTournament views and displays them in the web app
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
