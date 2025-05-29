using fantasydg.Data;
using fantasydg.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace fantasydg.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Notifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var invites = await _db.LeagueInvitations
                .Include(i => i.League)
                .Where(i => i.UserId == userId)
                .ToListAsync();

            var ownershipTransfers = await _db.LeagueOwnershipTransfers
                .Include(t => t.League)
                .Where(t => t.NewOwnerId == userId)
                .ToListAsync();

            ViewBag.Invitations = invites;
            ViewBag.OwnershipTransfers = ownershipTransfers;

            return View();
        }

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

            return RedirectToAction("Notifications");
        }
    }
}
