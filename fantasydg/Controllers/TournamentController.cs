using fantasydg.Data;
using fantasydg.Models;
using fantasydg.Models.Repository;
using fantasydg.Models.ViewModels;
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
        private readonly ApplicationDbContext _db;
        private readonly DataService _dataService;
        private readonly LeagueService _leagueService;
        private readonly DatabaseRepository _repository;
        private readonly ILogger<DataService> _logger;

        public TournamentController(ApplicationDbContext db, DataService dataService, LeagueService leagueService, DatabaseRepository repository, ILogger<DataService> logger)
        {
            _db = db;
            _dataService = dataService;
            _leagueService = leagueService;
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

                // Find the id of the tournament just entered
                var tournamentId = await GetTournamentName(model.TournamentId);
                var defaultDivision = "MPO";
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        // Return the name of the tournament given its ID
        private async Task<string?> GetTournamentName(int tournamentId)
        {
            return await _db.Tournaments
                .Where(t => t.Id == tournamentId && t.Division == "MPO")
                .Select(t => t.Name)
                .FirstOrDefaultAsync();
        }
    }
}
