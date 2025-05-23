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

        // Get tournament-level stats
        public async Task<List<PlayerTournament>> GetPlayerTournamentsAsync(string name, string division)
        {
            return await _db.PlayerTournaments
                .Include(pt => pt.Player)
                .Include(pt => pt.Tournament)
                .Where(pt => pt.Tournament.Name == name && pt.Division == division)
                .OrderBy(pt => pt.TotalToPar)
                .ToListAsync();
        }

        // Get round-level stats
        public async Task<List<RoundScore>> GetRoundScoresAsync(string name, string division, int round)
        {
            return await _db.RoundScores
                .Include(rs => rs.Player)
                .Include(rs => rs.Round)
                    .ThenInclude(r => r.Tournament)
                .Where(rs => rs.Round.Tournament.Name == name
                          && rs.Division == division
                          && rs.Round.RoundNumber == round)
                .OrderBy(rs => rs.RunningPlace)
                .ToListAsync();
        }

        public async Task<List<Tournament>> GetAllTournamentsAsync()
        {
            var all = await _db.Tournaments
                .OrderByDescending(t => t.Date)
                .ToListAsync(); // Execute SQL first

            return all
                .GroupBy(t => t.Name)
                .Select(g => g.OrderByDescending(t => t.Date).First())
                .ToList(); // Now this is safe in-memory
        }

        public async Task<List<string>> GetDivisionsForTournamentAsync(string name)
        {
            return await _db.Tournaments
                .Where(t => t.Name == name)
                .Select(t => t.Division)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<RoundScore>> GetRoundsForTournamentAsync(string name, string division)
        {
            return await _db.RoundScores
                .Include(rs => rs.Player)
                .Include(rs => rs.Round)
                    .ThenInclude(r => r.Tournament)
                .Where(rs => rs.Round.Tournament.Name == name &&
                             rs.Division == division)
                .OrderBy(r => r)
                .ToListAsync();
        }
    }
}