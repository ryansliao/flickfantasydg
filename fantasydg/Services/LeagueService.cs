using fantasydg.Data;
using Microsoft.EntityFrameworkCore;

namespace fantasydg.Services
{
    public class LeagueService
    {
        private readonly ApplicationDbContext _db;

        public LeagueService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task DeleteLeagueCascadeAsync(int leagueId)
        {
            // Delete team players → teams → members → league
            var teams = await _db.Teams.Where(t => t.LeagueId == leagueId).ToListAsync();
            var teamIds = teams.Select(t => t.TeamId).ToList();

            var teamPlayers = _db.TeamPlayers.Where(tp => teamIds.Contains(tp.TeamId));
            _db.TeamPlayers.RemoveRange(teamPlayers);
            _db.Teams.RemoveRange(teams);

            var members = _db.LeagueMembers.Where(lm => lm.LeagueId == leagueId);
            _db.LeagueMembers.RemoveRange(members);

            var league = await _db.Leagues
                .Include(l => l.Members)
                .Include(l => l.Teams)
                    .ThenInclude(t => t.TeamPlayers)
                .FirstOrDefaultAsync(l => l.LeagueId == leagueId);

            if (league != null)
                _db.Leagues.Remove(league);

            await _db.SaveChangesAsync();
        }
    }
}
