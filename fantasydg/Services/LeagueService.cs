using fantasydg.Data;
using fantasydg.Models;
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
        public async Task UpdateFantasyPointsForLeagueAsync(int leagueId)
        {
            var league = await _db.Leagues
                .Include(l => l.Teams)
                    .ThenInclude(t => t.TeamPlayers)
                .FirstOrDefaultAsync(l => l.LeagueId == leagueId);

            if (league == null) return;

            var PDGANumbers = await _db.PlayerTournaments
                .Where(pt => pt.Tournament != null) // defensive
                .Select(pt => pt.PDGANumber)
                .Distinct()
                .ToListAsync();

            // Fetch all PlayerTournament entries for those players
            var playerTournaments = await _db.PlayerTournaments
                .Where(pt => PDGANumbers.Contains(pt.PDGANumber))
                .Include(pt => pt.Tournament)
                .ToListAsync();

            foreach (var pt in playerTournaments)
            {
                float score = CalculateFantasyPoints(pt, league);

                var existing = await _db.LeaguePlayerFantasyPoints.FirstOrDefaultAsync(f =>
                    f.LeagueId == leagueId &&
                    f.PDGANumber == pt.PDGANumber &&
                    f.TournamentId == pt.TournamentId &&
                    f.Division == pt.Tournament.Division);

                if (existing == null)
                {
                    _db.LeaguePlayerFantasyPoints.Add(new LeaguePlayerFantasyPoints
                    {
                        LeagueId = leagueId,
                        PDGANumber = pt.PDGANumber,
                        TournamentId = pt.TournamentId,
                        Division = pt.Tournament.Division,
                        Points = score
                    });
                }
                else
                {
                    existing.Points = score;
                }
            }

            await _db.SaveChangesAsync();
        }

        public float CalculateFantasyPoints(PlayerTournament pt, League league)
        {
            double score = 0;

            score += pt.Place * league.PlacementWeight;
            score += pt.Fairway * league.FairwayWeight;
            score += pt.C1InReg * league.C1InRegWeight;
            score += pt.C2InReg * league.C2InRegWeight;
            score += pt.Parked * league.ParkedWeight;
            score += pt.Scramble * league.ScrambleWeight;
            score += pt.C1Putting * league.C1PuttWeight;
            score += pt.C1xPutting * league.C1xPuttWeight;
            score += pt.C2Putting * league.C2PuttWeight;
            score += pt.ObRate * league.OBWeight;
            score += pt.Birdie * league.BirdieWeight;
            score += pt.BirdieMinus * league.BirdieMinusWeight;
            score += pt.EagleMinus * league.EagleMinusWeight;
            score += pt.Par * league.ParWeight;
            score += pt.BogeyPlus * league.BogeyPlusWeight;
            score += pt.DoubleBogeyPlus * league.DoubleBogeyPlusWeight;
            score += pt.TotalPuttDistance * league.TotalPuttDistWeight;
            score += pt.AvgPuttDistance * league.AvgPuttDistWeight;
            score += pt.LongThrowIn * league.LongThrowInWeight;
            score += pt.StrokesGainedTotal * league.TotalSGWeight;
            score += pt.StrokesGainedPutting * league.PuttingSGWeight;
            score += pt.StrokesGainedTeeToGreen * league.TeeToGreenSGWeight;
            score += pt.StrokesGainedC1xPutting * league.C1xSGWeight;
            score += pt.StrokesGainedC2Putting * league.C2SGWeight;

            return (float)Math.Round(score, 2);
        }
        public async Task<Dictionary<(int PDGANumber, int tournamentId, string division), float>> GetFantasyPointsMapAsync(int leagueId)
        {
            return await _db.LeaguePlayerFantasyPoints
                .Where(fp => fp.LeagueId == leagueId)
                .ToDictionaryAsync(
                    fp => (fp.PDGANumber, fp.TournamentId, fp.Division),
                    fp => fp.Points
                );
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
