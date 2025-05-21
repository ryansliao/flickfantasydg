using fantasydg.Data;
using fantasydg.Models;
using Microsoft.EntityFrameworkCore;

namespace fantasydg.Models.Repository
{
    public class DatabaseRepository
    {
        private readonly DGDbContext _db;

        public DatabaseRepository(DGDbContext db)
        {
            _db = db;
        }

        public async Task<List<Player>> GetFilteredPlayersAsync(string name, string division, int? round)
        {
            var query = _db.Players
                .Include(p => p.Round)
                .ThenInclude(r => r.Tournament)
                .AsQueryable();

            if (!string.IsNullOrEmpty(division) && division != "All")
                query = query.Where(p => p.Round.Tournament.Division == division);

            if (!string.IsNullOrEmpty(name))
                query = query.Where(p => p.Round.Tournament.Name == name);

            if (!string.IsNullOrEmpty(division))
                query = query.Where(p => p.Round.Tournament.Division == division);

            if (round.HasValue)
                query = query.Where(p => p.Round.RoundNumber == round);

            return await query
                .OrderBy(p => p.Round.Tournament.Date)
                .ThenBy(p => p.Round.RoundNumber)
                .ThenBy(p => p.TournamentScore)
                .ToListAsync();
        }

        public async Task<List<string>> GetDistinctDivisionsAsync()
        {
            return await _db.Tournaments
                .Select(t => t.Division)
                .Distinct()
                .OrderByDescending(d => d)
                .ToListAsync();
        }

        public async Task<List<int>> GetRoundsForTournamentAsync(string name, string division)
        {
            return await _db.Rounds
                .Where(r => r.Tournament!.Name == name && r.Division == division)
                .Select(r => r.RoundNumber)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();
        }
    }
}