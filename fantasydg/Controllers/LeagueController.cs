using fantasydg.Data;
using fantasydg.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace fantasydg.Controllers
{
    [Authorize]
    public class LeagueController : Controller
    {
        private readonly ApplicationDbContext _db;

        public LeagueController(ApplicationDbContext db)
        {
            _db = db;
        }

        // View all leagues the current user is part of
        public async Task<IActionResult> Index()
        {
            var userId = User?.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            var leagues = await _db.LeagueMembers
                .Include(lm => lm.League)
                .ThenInclude(l => l.Members)
                .Where(lm => lm.UserId == userId)
                .Select(lm => lm.League)
                .ToListAsync();

            return View(leagues);
        }

        // View and manage settings for a specific league
        [Authorize]
        public async Task<IActionResult> Settings(int id)
        {
            var league = await _db.Leagues
                .Include(l => l.Owner)
                .Include(l => l.Members).ThenInclude(m => m.User)
                .FirstOrDefaultAsync(l => l.LeagueId == id);

            if (league == null)
                return NotFound();

            var userId = User?.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (league.OwnerId != userId)
            {
                return RedirectToAction("View", new { id = league.LeagueId });
            }

            return View(league);
        }

        // GET: /League/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /League/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(League league)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            league.OwnerId = userId;

            league.Members = new List<LeagueMember>
            {
                new LeagueMember { UserId = userId }
            };
            league.Teams = new List<Team>();
            league.PlayerNumber = 0;

            ModelState.Remove(nameof(league.OwnerId)); // fix lingering error

            if (!ModelState.IsValid)
            {
                // log errors
                return View(league);
            }

            _db.Leagues.Add(league);
            await _db.SaveChangesAsync();

            return RedirectToAction("View", new { id = league.LeagueId });
        }

        public async Task<IActionResult> View(int id)
        {
            var league = await _db.Leagues
                .Include(l => l.Members).ThenInclude(m => m.User)
                .Include(l => l.Teams)
                .FirstOrDefaultAsync(l => l.LeagueId == id);

            if (league == null)
                return NotFound();

            return View(league);
        }

        [HttpGet]
        public async Task<IActionResult> TransferOwnership(int leagueId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var league = await _db.Leagues
                .Include(l => l.Members).ThenInclude(m => m.User)
                .FirstOrDefaultAsync(l => l.LeagueId == leagueId);

            if (league == null || league.OwnerId != userId)
                return Forbid();

            return View(league); // This expects /Views/League/TransferOwnership.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Invite(int leagueId, string usernameOrEmail)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.UserName == usernameOrEmail || u.Email == usernameOrEmail);

            if (user == null)
            {
                TempData["InviteResult"] = "User not found.";
                return RedirectToAction("Settings", new { id = leagueId });
            }

            var alreadyMember = await _db.LeagueMembers
                .AnyAsync(lm => lm.LeagueId == leagueId && lm.UserId == user.Id);

            if (alreadyMember)
            {
                TempData["InviteResult"] = "User is already a member.";
            }
            else
            {
                _db.LeagueInvitations.Add(new LeagueInvitation
                {
                    LeagueId = leagueId,
                    UserId = user.Id,
                    SentAt = DateTime.UtcNow
                });

                await _db.SaveChangesAsync();
                TempData["InviteResult"] = "Invitation sent.";
            }

            return RedirectToAction("Settings", new { id = leagueId });
        }

        public async Task<IActionResult> Invitations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var invites = await _db.LeagueInvitations
                .Include(i => i.League)
                .Where(i => i.UserId == userId)
                .ToListAsync();

            return View(invites);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestOwnershipTransfer(int leagueId, string newOwnerId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var league = await _db.Leagues.FindAsync(leagueId);
            if (league == null || league.OwnerId != userId)
                return Forbid();

            var exists = await _db.LeagueOwnershipTransfers
                .AnyAsync(r => r.LeagueId == leagueId && r.NewOwnerId == newOwnerId);

            if (!exists)
            {
                var transfer = new LeagueOwnershipTransfer
                {
                    LeagueId = leagueId,
                    NewOwnerId = newOwnerId
                };
                _db.LeagueOwnershipTransfers.Add(transfer);
                await _db.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Ownership transfer request sent.";
            return RedirectToAction("Settings", new { id = leagueId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptOwnershipTransfer(int transferId)
        {
            var transfer = await _db.LeagueOwnershipTransfers
                .Include(t => t.League)
                .FirstOrDefaultAsync(t => t.LeagueOwnershipTransferId == transferId);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (transfer == null || transfer.NewOwnerId != userId)
                return Forbid();

            transfer.League.OwnerId = userId;
            _db.LeagueOwnershipTransfers.Remove(transfer);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"You are now the owner of {transfer.League.Name}.";
            return RedirectToAction("View", "League", new { id = transfer.LeagueId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var league = await _db.Leagues
                .Include(l => l.Members)
                .Include(l => l.Teams)
                .ThenInclude(t => t.TeamPlayers)
                .FirstOrDefaultAsync(l => l.LeagueId == id);

            if (league == null)
                return NotFound();

            // Cascade delete members and teams
            _db.TeamPlayers.RemoveRange(league.Teams.SelectMany(t => t.TeamPlayers));
            _db.Teams.RemoveRange(league.Teams);
            _db.LeagueMembers.RemoveRange(league.Members);
            _db.Leagues.Remove(league);

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
