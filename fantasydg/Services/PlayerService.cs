using fantasydg.Data;
using fantasydg.Models;
using fantasydg.Services;
using Microsoft.EntityFrameworkCore;
using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class PlayerService
{
    private readonly ApplicationDbContext _db;
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://www.pdga.com";
    private const string RankingsUrl = "https://www.pdga.com/world-rankings";

    public PlayerService(ApplicationDbContext db, HttpClient httpClient)
    {
        _db = db;
        _httpClient = httpClient;
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

    public async Task UpdateAllWorldRankingsAsync(bool includeMPO, bool includeFPO)
    {
        if (!includeMPO && !includeFPO)
            return;

        List<RankedPlayer> mpo = new();
        List<RankedPlayer> fpo = new();

        if (includeMPO)
            mpo = await GetRankingsAsync("MPO");

        if (includeFPO)
            fpo = await GetRankingsAsync("FPO");

        var players = await _db.Players.ToListAsync();

        foreach (var player in players)
        {
            RankedPlayer? match = null;

            if (includeMPO)
                match = mpo.FirstOrDefault(p => p.PDGANumber == player.PDGANumber);

            if (match == null && includeFPO)
                match = fpo.FirstOrDefault(p => p.PDGANumber == player.PDGANumber);

            player.WorldRanking = match?.Rank ?? 0;
        }

        await _db.SaveChangesAsync();
    }

    private async Task<List<RankedPlayer>> GetRankingsAsync(string division)
    {
        var rootHtml = await _httpClient.GetStringAsync(RankingsUrl);
        var doc = new HtmlDocument();
        doc.LoadHtml(rootHtml);

        string linkText = division == "MPO" ? "All MPO Rankings" : "All FPO Rankings";
        var linkNode = doc.DocumentNode.SelectSingleNode($"//a[contains(text(), '{linkText}')]");

        if (linkNode == null) return new List<RankedPlayer>();

        var href = linkNode.GetAttributeValue("href", null);
        string divisionUrl = href != null && href.StartsWith("http") ? href : BaseUrl + href;
        string rankingsHtml;
        try
        {
            rankingsHtml = await _httpClient.GetStringAsync(divisionUrl);
        }
        catch
        {
            return new List<RankedPlayer>();
        }
        var rankingsDoc = new HtmlDocument();
        rankingsDoc.LoadHtml(rankingsHtml);

        var rows = rankingsDoc.DocumentNode.SelectNodes("//table//tr");
        var result = new List<RankedPlayer>();

        if (rows == null) return result;

        foreach (var row in rows)
        {
            var cells = row.SelectNodes("td");
            if (cells == null || cells.Count < 3) continue;

            var rankText = cells[0].InnerText.Trim();
            var rankParts = rankText.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var rankString = rankParts.LastOrDefault();

            var numText = cells[1].SelectSingleNode(".//a");
            if (numText == null) continue;

            var numString = numText.GetAttributeValue("href", "");
            var parts = numString.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var pdgaString = parts.LastOrDefault();

            if (!int.TryParse(pdgaString, out int pdgaNumber))
                continue;

            if (!int.TryParse(rankString, out int rank))
                continue;

            result.Add(new RankedPlayer
            {
                Rank = rank,
                PDGANumber = pdgaNumber
            });
        }

        return result;
    }

    private class RankedPlayer
    {
        public int Rank { get; set; }
        public int PDGANumber { get; set; }
    }
}
