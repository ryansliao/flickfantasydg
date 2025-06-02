using fantasydg.Data;
using fantasydg.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace fantasydg.Controllers
{
    public class TeamController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeamController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
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

        public async Task<IActionResult> View(int teamId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var team = await _db.Teams
                .Include(t => t.League)
                .Include(t => t.TeamPlayers)
                .ThenInclude(tp => tp.Player)
                .FirstOrDefaultAsync(t => t.TeamId == teamId && t.OwnerId == userId);

            if (team == null) return NotFound();

            ViewBag.LeagueName = team.League?.Name;
            ViewBag.TeamId = team.TeamId;

            return View(team);
        }
    }

}
