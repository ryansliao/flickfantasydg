using fantasydg.Data;
using fantasydg.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace fantasydg.Models.Repository
{
    public class DatabaseRepository
    {
        private readonly ApplicationDbContext _db;

        public DatabaseRepository(ApplicationDbContext db)
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

        // Get Tournaments table
        public async Task<List<Tournament>> GetAllTournamentsAsync()
        {
            var all = await _db.Tournaments
                .OrderByDescending(t => t.StartDate)
                .ToListAsync(); // Execute SQL first

            return all
                .GroupBy(t => t.Name)
                .Select(g => g.OrderByDescending(t => t.StartDate).First())
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
    }
}