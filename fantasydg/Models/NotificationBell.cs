using fantasydg.Data;
using fantasydg.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fantasydg.ViewComponents
{
    public class NotificationBell : ViewComponent
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationBell(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
                return View(0);

            var inviteCount = await _db.LeagueInvitations.CountAsync(i => i.UserId == user.Id);
            var transferCount = await _db.LeagueOwnershipTransfers.CountAsync(t => t.NewOwnerId == user.Id);

            var total = inviteCount + transferCount;
            return View(total);
        }
    }
}
