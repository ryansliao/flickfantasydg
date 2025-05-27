using fantasydg.Data;
using fantasydg.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace fantasydg.Models.Repository
{
    public class DatabaseRepository
    {
        private readonly DGDbContext _db;

        public DatabaseRepository(DGDbContext db)
        {
            _db = db;
        }

        // Get PlayerTournament table
        public async Task<List<PlayerTournament>> GetPlayerTournamentsAsync(int id, string division)
        {
            return await _db.PlayerTournaments
                .Include(pt => pt.Player)
                .Include(pt => pt.Tournament)
                .Where(pt => pt.Tournament.Id == id && pt.Division == division)
                .OrderBy(pt => pt.TotalToPar)
                .ToListAsync();
        }

        // Get RoundScores table
        public async Task<List<RoundScore>> GetRoundScoresAsync(int id, string division, int round)
        {
            return await _db.RoundScores
                .Include(rs => rs.Player)
                .Include(rs => rs.Round)
                    .ThenInclude(r => r.Tournament)
                .Where(rs => rs.Round.Tournament.Id == id
                          && rs.Division == division
                          && rs.Round.RoundNumber == round)
                .OrderBy(rs => rs.RunningPlace)
                    .ThenBy(rs => rs.RoundToPar)
                .ToListAsync();
        }

        // Get Tournaments table
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

        // Get divisions in the Tournament table
        public async Task<List<string>> GetDivisionsForTournamentAsync(int id)
        {
            return await _db.Tournaments
                .Where(t => t.Id == id)
                .Select(t => t.Division)
                .Distinct()
                .ToListAsync();
        }

        // Get Rounds table
        public async Task<List<int>> GetRoundsForTournamentAsync(int id, string division)
        {
            return await _db.Rounds
                .Include(r => r.Tournament)
                .Where(r => r.Tournament.Id == id && r.Division == division)
                .Select(r => r.RoundNumber)
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync();
        }
    }
}