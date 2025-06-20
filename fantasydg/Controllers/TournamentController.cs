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
        [HttpPost]
        public async Task<IActionResult> Input(int TournamentId, int leagueId)
        {
            string[] divisions = { "MPO", "FPO" };

            foreach (var division in divisions)
            {
                await _dataService.FetchTournaments(TournamentId, division);
            }

            TempData["TournamentInputSuccess"] = "Tournament successfully added or updated!";
            return RedirectToAction("Settings", "League", new { id = leagueId });
        }
    }
}
