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
        private readonly ApplicationDbContext _db;
        private readonly DataService _dataService;
        private readonly LeagueService _leagueService;
        private readonly DatabaseRepository _repository;
        private readonly ILogger<DataService> _logger;

        public DatabaseController(ApplicationDbContext db, DataService dataService, LeagueService leagueService, DatabaseRepository repository, ILogger<DataService> logger)
        {
            _db = db;
            _dataService = dataService;
            _leagueService = leagueService;
            _repository = repository;
            _logger = logger;
        }

        public async Task<IActionResult> Database()
        {
            var allTournaments = await _repository.GetAllTournamentsAsync();
            var latest = allTournaments.FirstOrDefault();

            if (latest == null)
            {
                // No tournaments yet — show a message or send to Input form
                TempData["Message"] = "No tournaments found. Please enter a tournament ID to get started.";
                return RedirectToAction("Input");
            }

            var divisions = await _repository.GetDivisionsForTournamentAsync(latest.Id);
            var defaultDivision = divisions.Contains("MPO") ? "MPO" : divisions.FirstOrDefault();

            return Redirect($"/tournaments/{latest.Id}/{defaultDivision}");
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

                // Find the id of the tournament just entered
                var tournamentId = await GetTournamentName(model.TournamentId);
                var defaultDivision = "MPO";
                return Redirect($"/tournaments/{model.TournamentId}/{defaultDivision}");
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

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _leagueService.DeleteLeagueCascadeAsync(id);
            return RedirectToAction("Index"); // or wherever your list of leagues is
        }

        // Access the database and populates the views
        [HttpGet("/tournaments/{tournamentId:int}/{division?}/{round?}")]
        public async Task<IActionResult> DatabaseView(int tournamentId, string? division = null, int? round = null)
        {            
            // Select all tournaments
            var allTournaments = await _repository.GetAllTournamentsAsync();
            ViewBag.Tournaments = allTournaments // Tournament dropdown
                .OrderByDescending(t => t.Date)
                .ToList();

            var selectedTournament = allTournaments.FirstOrDefault(t => t.Id == tournamentId);
            if (selectedTournament == null)
            {
                _logger.LogWarning("Invalid tournament ID: {TournamentId}", tournamentId);
                return View("DatabaseView", new List<PlayerTournament>());
            }

            var tournamentName = selectedTournament.Name;
            ViewBag.SelectedTournamentId = tournamentId; // Selected tournament dropdown item

            // Select all divisions for each tournament if it exists in database
            ViewBag.Divisions = (await _repository.GetDivisionsForTournamentAsync(tournamentId)) // Division name dropdown for existing tournaments
                .OrderByDescending(d => d) 
                .ToList();

            if (string.IsNullOrEmpty(division))
                division = (ViewBag.Divisions as List<string>)?.Contains("MPO") == true ? "MPO" : (ViewBag.Divisions as List<string>)?.FirstOrDefault();
            
            ViewBag.SelectedDivision = division; // Selected division dropdown item 

            // Select all existing rounds for each tournament and division
            List<int> validRounds = new();

            if (!string.IsNullOrEmpty(division))
            {
                validRounds = await _repository.GetRoundsForTournamentAsync(tournamentId, division);
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
                var roundScores = await _repository.GetRoundScoresAsync(tournamentId, division, round.Value);
                return View("DatabaseView", roundScores);
            }
            else
            {
                var playerTournaments = await _repository.GetPlayerTournamentsAsync(tournamentId, division);
                return View("DatabaseView", playerTournaments);
            }
        }
    }
}
