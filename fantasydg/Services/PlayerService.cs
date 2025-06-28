using fantasydg.Data;
using fantasydg.Models;
using Microsoft.EntityFrameworkCore;
public class PlayerService
{
    private readonly ApplicationDbContext _db;

    public PlayerService(ApplicationDbContext db)
    {
        _db = db;
    }

    public double CalculatePoints(League league, PlayerTournament pt, Tournament tournament)
    {
        if (pt == null || tournament == null) return 0;

        return
            pt.Place * league.PlacementWeight +
            pt.Fairway * league.FairwayWeight +
            pt.C1InReg * league.C1InRegWeight +
            pt.C2InReg * league.C2InRegWeight +
            pt.Parked * league.ParkedWeight +
            pt.Scramble * league.ScrambleWeight +
            pt.C1Putting * league.C1PuttWeight +
            pt.C1xPutting * league.C1xPuttWeight +
            pt.C2Putting * league.C2PuttWeight +
            pt.ObRate * league.OBWeight +
            pt.Par * league.ParWeight +
            pt.Birdie * league.BirdieWeight +
            pt.BirdieMinus * league.BirdieMinusWeight +
            pt.EagleMinus * league.EagleMinusWeight +
            pt.BogeyPlus * league.BogeyPlusWeight +
            pt.DoubleBogeyPlus * league.DoubleBogeyPlusWeight +
            pt.TotalPuttDistance * league.TotalPuttDistWeight +
            pt.AvgPuttDistance * league.AvgPuttDistWeight +
            pt.LongThrowIn * league.LongThrowInWeight +
            pt.StrokesGainedTotal * league.TotalSGWeight +
            pt.StrokesGainedPutting * league.PuttingSGWeight +
            pt.StrokesGainedTeeToGreen * league.TeeToGreenSGWeight +
            pt.StrokesGainedC1xPutting * league.C1xSGWeight +
            pt.StrokesGainedC2Putting * league.C2SGWeight;
    }

    public async Task UpdateFantasyPoints(int leagueId)
    {
        var league = await _db.Leagues.FindAsync(leagueId);
        if (league == null) return;

        var tournaments = await _db.Tournaments
            .Where(t => (league.IncludeMPO && t.Division == "MPO") || (league.IncludeFPO && t.Division == "FPO"))
            .ToListAsync();

        var tournamentIds = tournaments.Select(t => t.Id).ToList();
        var tournamentMap = tournaments.ToDictionary(t => t.Id);

        var pts = await _db.PlayerTournaments
            .Where(pt => tournamentIds.Contains(pt.TournamentId))
            .ToListAsync();

        foreach (var pt in pts)
        {
            if (!tournamentMap.TryGetValue(pt.TournamentId, out var tournament)) continue;
            pt.FantasyPoints = CalculatePoints(league, pt, tournament);
        }

        await _db.SaveChangesAsync();
    }
}
