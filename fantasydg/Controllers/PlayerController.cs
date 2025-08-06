using fantasydg.Data;
using fantasydg.Models;
using fantasydg.Models.ViewModels;
using fantasydg.Models.Repository;
using fantasydg.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using System.Security.Claims;

namespace fantasydg.Controllers
{
    public class PlayerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly DatabaseRepository _repository;

        public PlayerController(ApplicationDbContext db, DatabaseRepository repository)
        {
            _db = db;
            _repository = repository;
        }

        // Fetch season stats for each player
        [HttpGet]
        public async Task<IActionResult> GetStats(int pdgaNumber)
        {
            var playerTournaments = await _db.PlayerTournaments
                .Where(pt => pt.PDGANumber == pdgaNumber)
                .ToListAsync();

            if (!playerTournaments.Any())
                return Json(new { totalPoints = 0, avgPlace = 0, tournamentCount = 0 });

            var totalPoints = playerTournaments.Sum(pt => pt.FantasyPoints);
            var avgPlace = playerTournaments.Average(pt => pt.Place);
            var tournamentCount = playerTournaments.Count;

            return Json(new
            {
                totalPoints,
                avgPlace,
                tournamentCount
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetWorldRanking(int pdgaNumber)
        {
            var player = await _db.Players.FirstOrDefaultAsync(p => p.PDGANumber == pdgaNumber);
            if (player == null) return NotFound();

            return Json(new { worldRanking = player.WorldRanking });
        }
    }
}
