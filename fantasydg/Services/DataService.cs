using fantasydg.Data;
using fantasydg.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Drawing.Printing;
using System.Security.Principal;
using static System.Formats.Asn1.AsnWriter;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace fantasydg.Services
{
    // Get API data
    // Initialize and populate class objects
    public partial class DataService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<DataService> _logger;
        private object playerTournaments;

        public DataService(HttpClient httpClient, ApplicationDbContext db, ILogger<DataService> logger)
        {
            _httpClient = httpClient;
            _db = db;
            _logger = logger;
        }

        // Get tournament name and date
        public async Task<List<string>> GetTournamentInfo(int tournamentId)
        {
            try
            {
                var url = $"https://www.pdga.com/apps/tournament/live-api/live_results_fetch_event?TournID={tournamentId}";
                var json = await _httpClient.GetStringAsync(url);
                
                var name = JObject.Parse(json)?["data"]?["MultiLineName"]?["main"]?.ToString() ?? $"Tournament {tournamentId}";
                var startDateStr = JObject.Parse(json)?["data"]?["StartDate"]?.ToString();
                var endDateStr = JObject.Parse(json)?["data"]?["EndDate"]?.ToString();
                var tier = JObject.Parse(json)?["data"]?["TierPro"]?.ToString();

                DateTime.TryParse(startDateStr, out var startDate);
                DateTime.TryParse(endDateStr, out var endDate);

                return new List<string> { name, startDateStr, endDateStr, tier };
            }
            catch
            {
                return new List<string> { $"Tournament {tournamentId}", "" };
            }
        }

        // Initialize player object from either the database or API response
        private async Task<Player> GetOrCreatePlayerAsync(int pdgaNumber, string name)
        {
            // Try local context first (helps avoid unnecessary DB hits)
            var player = _db.Players.Local.FirstOrDefault(p => p.PDGANumber == pdgaNumber)
                ?? await _db.Players.FindAsync(pdgaNumber);

            if (player == null)
            {
                player = new Player
                {
                    PDGANumber = pdgaNumber,
                    Name = name
                };
                _db.Players.Add(player);
                await _db.SaveChangesAsync(); // Ensure it's saved to DB
            }

            return player;
        }

        // Get tournament stats and populates tournament object
        public async Task FetchTournaments(int tournamentId, string division)
        {
            try
            {
                // Parse tournament stats API response
                var statsUrl = $"https://www.pdga.com/api/v1/feat/stats/tournament-division-stats/{tournamentId}/{division}/";
                var statsJson = await _httpClient.GetStringAsync(statsUrl);
                var statsRoot = JObject.Parse(statsJson);
                var statsArray = statsRoot["resultStats"] as JArray;

                // Initialize Tournament object from either the database or API response
                var tournament = _db.Tournaments.Local
                    .FirstOrDefault(t => t.Id == tournamentId && t.Division == division)
                    ?? await _db.Tournaments
                        .FirstOrDefaultAsync(t => t.Id == tournamentId && t.Division == division);
                if (tournament == null)
                {
                    tournament = new Tournament
                    {
                        Id = tournamentId,
                        Division = division,
                    };
                    _db.Tournaments.Add(tournament);
                }

                // Assign tournament name and date
                var info = await GetTournamentInfo(tournamentId);
                var startDateStr = info[1];
                var endDateStr = info[2];

                DateTime.TryParse(startDateStr, out var parsedStartDate);
                DateTime.TryParse(endDateStr, out var parsedEndDate);

                tournament.Name = info[0];
                tournament.Tier = info[2];
                tournament.StartDate = parsedStartDate == default ? DateTime.UtcNow : parsedStartDate;
                tournament.EndDate = parsedEndDate == default ? DateTime.UtcNow : parsedEndDate;

                await _db.SaveChangesAsync(); // Save tournament object to database

                var playerTournaments = new Dictionary<int, PlayerTournament>(); // Initialize dictionary of PlayerTournament objects

                var (finalRoundStats, resultToPdga, roundNumber, nameMap) = await FetchRounds(tournamentId, division); //Initialize player from the final round
                tournament.RoundNumber = roundNumber;

                // Initialize player tournaments with empty stats before tournament starts
                if (statsArray == null || !statsArray.Any())
                {
                    foreach (var kvp in resultToPdga)
                    {
                        int resultId = kvp.Key;
                        int pdgaNumber = kvp.Value;

                        string name = nameMap.ContainsKey(pdgaNumber) ? nameMap[pdgaNumber] : "Unknown";
                        var player = await GetOrCreatePlayerAsync(pdgaNumber, name);

                        var pt = new PlayerTournament
                        {
                            ResultId = resultId,
                            PDGANumber = pdgaNumber,
                            TournamentId = tournamentId,
                            Division = division
                        };

                        if (finalRoundStats.TryGetValue(pdgaNumber, out var stats))
                        {
                            pt.Place = stats.Place;
                            pt.TotalToPar = stats.ToPar;
                        }

                        playerTournaments[resultId] = pt;
                    }
                }

                // Assign PlayerTournament stats for every player present in API response
                foreach (var playerData in statsArray ?? new JArray())
                {
                    var result = playerData["result"];
                    var statList = playerData["stats"] as JArray;

                    if (result == null)
                        continue;

                    int resultId = result["resultId"]?.Value<int>() ?? 0;
                    string name = $"{result["firstName"]} {result["lastName"]}".Trim();

                    if (!resultToPdga.TryGetValue(resultId, out int PDGANumber) || PDGANumber == 0)
                        continue;

                    var player = await GetOrCreatePlayerAsync(PDGANumber, name); // Initialize player object

                    // Initialize empty PlayerTournament object
                    var pt = new PlayerTournament
                    {
                        ResultId = resultId,
                        PDGANumber = PDGANumber,
                        TournamentId = tournamentId,
                        Division = division
                    };

                    playerTournaments[resultId] = pt;

                    // Assign final round place and score to PlayerTournament attributes
                    if (finalRoundStats.TryGetValue(pt.PDGANumber, out var stats))
                    {
                        pt.Place = stats.Place;
                        pt.TotalToPar = stats.ToPar;
                    }

                    // Assign API stats to PlayerTournament attributes
                    foreach (var stat in statList)
                    {
                        int statId = stat["statId"]?.Value<int>() ?? 0;
                        double statValue = stat["statValue"]?.Type == JTokenType.Null ? 0.0 : stat["statValue"]?.Value<double>() ?? 0.0;
                        int statCount = stat["statValue"]?.Type == JTokenType.Null ? 0 : stat["statValue"]?.Value<int>() ?? 0;

                        switch (statId)
                        {
                            case 1: pt.Fairway = Math.Round(statValue, 0); break;
                            case 2: pt.C1InReg = Math.Round(statValue, 0); break;
                            case 3: pt.C2InReg = Math.Round(statValue, 0); break;
                            case 4: pt.Parked = Math.Round(statValue, 1); break;
                            case 5: pt.Scramble = Math.Round(statValue, 0); break;
                            case 6: pt.C1Putting = Math.Round(statValue, 0); break;
                            case 7: pt.C1xPutting = Math.Round(statValue, 0); break;
                            case 8: pt.C2Putting = Math.Round(statValue, 0); break;
                            case 9: pt.ObRate = Math.Round(statValue, 1); break;
                            case 10: pt.BirdieMinus = Math.Round(statValue, 0); break;
                            case 11: pt.DoubleBogeyPlus = Math.Round(statValue, 1); break;
                            case 12: pt.BogeyPlus = Math.Round(statValue, 0); break;
                            case 13: pt.Par = Math.Round(statValue, 0); break;
                            case 14: pt.Birdie = Math.Round(statValue, 0); break;
                            case 15: pt.EagleMinus = Math.Round(statValue, 1); break;
                            case 16: pt.TotalPuttDistance = statCount; break;
                            case 17: pt.LongThrowIn = statCount; break;
                            case 18: pt.AvgPuttDistance = Math.Round(statValue, 1); break;
                            default: continue;
                        }
                    }
                }

                // Parse tournament strokes gained stats API response
                var strokesGainedUrl = $"https://www.pdga.com/api/v1/feat/stats/tournament-division-strokes-gained/{tournamentId}/{division}/";
                var strokesJson = await _httpClient.GetStringAsync(strokesGainedUrl);
                var strokesRoot = JObject.Parse(strokesJson);
                var strokesArray = strokesRoot["resultStats"] as JArray;

                // Assign PlayerTournament strokes gained stats for every player present in API response
                foreach (var sgData in strokesArray ?? new JArray())
                {
                    var result = sgData["result"];
                    var sgList = sgData["stats"] as JArray;

                    if (result == null || sgList == null)
                        continue;

                    int resultId = result["resultId"]?.Value<int>() ?? 0;
                    if (playerTournaments.TryGetValue(resultId, out var pt))
                    {
                        // Assign API strokes gained stats to PlayerTournament attributes
                        foreach (var stat in sgList)
                        {
                            int statId = stat["statId"]?.Value<int>() ?? 0;
                            double statValue = stat["statValue"]?.Type == JTokenType.Null ? 0.0 : stat["statValue"]?.Value<double>() ?? 0.0;

                            switch (statId)
                            {
                                case 100: pt.StrokesGainedTotal = Math.Round(statValue, 2); break;
                                case 101: pt.StrokesGainedPutting = Math.Round(statValue, 2); break;
                                case 102: pt.StrokesGainedTeeToGreen = Math.Round(statValue, 2); break;
                                case 104: pt.StrokesGainedC1xPutting = Math.Round(statValue, 2); break;
                                case 105: pt.StrokesGainedC2Putting = Math.Round(statValue, 2); break;
                                default: continue;
                            }
                        }
                    }
                }

                // Adds PlayerTournament object if no PlayerTournament object with matching composite keys exists in database
                foreach (var pt in playerTournaments.Values)
                {
                    var existing = await _db.PlayerTournaments
                        .FirstOrDefaultAsync(x =>
                            x.PDGANumber == pt.PDGANumber &&
                            x.TournamentId == pt.TournamentId &&
                            x.Division == pt.Division);
                    if (existing == null)
                    {
                        _db.PlayerTournaments.Add(pt);
                    }
                    else
                    {
                        existing.Place = pt.Place;
                        existing.TotalToPar = pt.TotalToPar;
                        existing.Fairway = pt.Fairway;
                        existing.C1InReg = pt.C1InReg;
                        existing.C2InReg = pt.C2InReg;
                        existing.Parked = pt.Parked;
                        existing.Scramble = pt.Scramble;
                        existing.C1Putting = pt.C1Putting;
                        existing.C1xPutting = pt.C1xPutting;
                        existing.C2Putting = pt.C2Putting;
                        existing.ObRate = pt.ObRate;
                        existing.BirdieMinus = pt.BirdieMinus;
                        existing.DoubleBogeyPlus = pt.DoubleBogeyPlus;
                        existing.BogeyPlus = pt.BogeyPlus;
                        existing.Par = pt.Par;
                        existing.Birdie = pt.Birdie;
                        existing.EagleMinus = pt.EagleMinus;
                        existing.TotalPuttDistance = pt.TotalPuttDistance;
                        existing.LongThrowIn = pt.LongThrowIn;
                        existing.AvgPuttDistance = pt.AvgPuttDistance;
                        existing.StrokesGainedTotal = pt.StrokesGainedTotal;
                        existing.StrokesGainedPutting = pt.StrokesGainedPutting;
                        existing.StrokesGainedTeeToGreen = pt.StrokesGainedTeeToGreen;
                        existing.StrokesGainedC1xPutting = pt.StrokesGainedC1xPutting;
                        existing.StrokesGainedC2Putting = pt.StrokesGainedC2Putting;

                        _db.Entry(existing).State = EntityState.Modified;
                    }
                }

                await _db.SaveChangesAsync(); // Save PlayerTournament objects in database
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch tournament stats for {TournamentId} ({Division})", tournamentId, division);
            }
        }

        // Get round stats and populates round object
        private async Task<(Dictionary<int, (int Place, int ToPar)> finalStats, Dictionary<int, int> resultToPdga, int roundNumber, Dictionary<int, string> nameMap)>
        FetchRounds(int tournamentId, string division)
        {
            Dictionary<int, (int Place, int ToPar)> latestRoundStats = new();
            Dictionary<int, int> resultToPdga = new();
            Dictionary<int, string> nameMap = new();
            bool httpError = false;
            int roundNumber = 0;

            for (int pdgaRound = 1; pdgaRound <= 12; pdgaRound++)
            {
                // If a round doesn't exist, skip to round 12
                if (httpError && pdgaRound < 12)
                    continue; 

                try
                {
                    // Parse round information API response
                    string roundUrl = $"https://www.pdga.com/apps/tournament/live-api/live_results_fetch_round?TournID={tournamentId}&Division={division}&Round={pdgaRound}";
                    var roundJson = await _httpClient.GetStringAsync(roundUrl);
                    var roundData = JObject.Parse(roundJson)["data"];

                    if (roundData[0]["pool"] != null)
                    {
                        roundData = roundData[0];
                    }

                    if (roundData == null || roundData["scores"] == null)
                    {
                        continue;
                    }

                    

                    // Get final round placement and score
                    roundNumber += 1;

                    var currentRoundStats = new Dictionary<int, (int Place, int ToPar)>();
                    foreach (var p in roundData["scores"])
                    {
                        int PDGANumber = p["PDGANum"]?.Value<int>() ?? 0;
                        int resultId = p["ResultID"]?.Value<int>() ?? 0;
                        int place = p["RunningPlace"]?.Type == JTokenType.Null ? 0 : p["RunningPlace"]?.Value<int>() ?? 0;
                        int toPar = p["ParThruRound"]?.Type == JTokenType.Null ? 0 : p["ParThruRound"]?.Value<int>() ?? 0;

                        if (PDGANumber > 0)
                            currentRoundStats[PDGANumber] = (place, toPar);
                        if (PDGANumber > 0 && resultId > 0)
                            resultToPdga[resultId] = PDGANumber;
                        if (PDGANumber > 0 && !nameMap.ContainsKey(PDGANumber))
                        {
                            string name = p["Name"]?.ToString() ?? "";
                            nameMap[PDGANumber] = name;
                        }
                    }

                    if (currentRoundStats.Count > 0)
                    {
                        latestRoundStats = currentRoundStats;
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing round {RoundNumber} for tournament {TournamentId}.", pdgaRound, tournamentId);
                    httpError = true;
                    continue;
                }
            }

            return (latestRoundStats, resultToPdga, roundNumber, nameMap);
        }
    }
}
