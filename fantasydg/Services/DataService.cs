using fantasydg.Data;
using fantasydg.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
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

        public DataService(HttpClient httpClient, ApplicationDbContext db, ILogger<DataService> logger)
        {
            _httpClient = httpClient;
            _db = db;
            _logger = logger;
        }

        // Get tournament name and date
        private async Task<List<string>> GetTournamentNameDate(int tournamentId)
        {
            try
            {
                var url = $"https://www.pdga.com/apps/tournament/live-api/live_results_fetch_event?TournID={tournamentId}";
                var json = await _httpClient.GetStringAsync(url);
                
                var name = JObject.Parse(json)?["data"]?["MultiLineName"]?["main"]?.ToString() ?? $"Tournament {tournamentId}";
                var dateStr = JObject.Parse(json)?["data"]?["StartDate"]?.ToString();

                DateTime? date = null;
                if (DateTime.TryParse(dateStr, out var parsedDate))
                {
                    date = parsedDate;
                }
                return new List<string> { name, dateStr };
            }
            catch
            {
                return new List<string> { $"Tournament {tournamentId}", "" };
            }
        }

        // Initialize player object from either the database or API response
        private async Task<Player> GetOrCreatePlayerAsync(int playerId, string name)
        {
            var player = _db.Players.Local
                .FirstOrDefault(p => p.PlayerId == playerId)
                ?? await _db.Players.FindAsync(playerId);

            if (player == null)
            {
                player = new Player { PlayerId = playerId, Name = name };
                _db.Players.Add(player);
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
                        Weight = 1.0
                    };
                    _db.Tournaments.Add(tournament);
                }

                // Assign tournament name and date
                var nameDate = await GetTournamentNameDate(tournamentId);
                var dateStr = nameDate[1];
                DateTime date = DateTime.TryParse(dateStr, out var parsedDate)
                    ? parsedDate
                    : DateTime.UtcNow;
                tournament.Name = nameDate[0];
                tournament.Date = date;

                await _db.SaveChangesAsync(); // Save tournament object to database
                
                var playerTournaments = new Dictionary<int, PlayerTournament>(); // Initialize dictionary of PlayerTournament objects

                var finalRoundStats = await FetchRounds(tournamentId, division); //Initialize player placement and score from the final round

                // Assign PlayerTournament stats for every player present in API response
                foreach (var playerData in statsArray ?? new JArray())
                {
                    var result = playerData["result"];
                    var statList = playerData["stats"] as JArray;

                    if (result == null || statList == null)
                        continue;

                    int playerId = result["resultId"]?.Value<int>() ?? 0;
                    string name = $"{result["firstName"]} {result["lastName"]}".Trim();

                    var player = await GetOrCreatePlayerAsync(playerId, name); // Initialize player object

                    // Initialize empty PlayerTournament object
                    var pt = new PlayerTournament
                    {
                        PlayerId = playerId,
                        TournamentId = tournamentId,
                        Division = division
                    };

                    // Assign final round place and score to PlayerTournament attributes
                    if (finalRoundStats.TryGetValue(pt.PlayerId, out var stats))
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

                    // Adds object to dictionary if the object's player ID doesn't exist in the dictionary yet
                    if (!playerTournaments.ContainsKey(playerId))
                        playerTournaments[playerId] = pt;
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

                    int playerId = result["resultId"]?.Value<int>() ?? 0;
                    if (!playerTournaments.TryGetValue(playerId, out var pt)) continue; // Skips player IDs not found in the existing dictionary

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

                // Adds PlayerTournament object if no PlayerTournament object with matching composite keys exists in database
                foreach (var pt in playerTournaments.Values)
                {
                    var existing = await _db.PlayerTournaments.FindAsync(pt.PlayerId, pt.TournamentId, pt.Division);
                    if (existing == null)
                    {
                        _db.PlayerTournaments.Add(pt);
                    }
                    else
                    {
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
        private async Task<Dictionary<int, (int Place, int ToPar)>> FetchRounds(int tournamentId, string division)
        {
            Dictionary<int, RoundScore> finalPlayerMap = null;
            int actualRoundNumber = 0;
            bool httpError = false;
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

                    if (roundData == null || roundData["scores"] == null)
                    {
                        continue;
                    }

                    int? liveRoundId = (int?)roundData["live_round_id"];
                    if (liveRoundId == null) continue;
                    actualRoundNumber += 1; // Adds 1 to round number every valid API request

                    // Initiate local tournament object from either the database or API response
                    var tournament = _db.Tournaments.Local
                        .FirstOrDefault(t => t.Id == tournamentId && t.Division == division)
                        ?? await _db.Tournaments
                            .FirstOrDefaultAsync(t => t.Id == tournamentId && t.Division == division);
                    if (tournament == null)
                    {
                        tournament = new Tournament { Id = tournamentId, Division = division, Name = $"Tournament {tournamentId}", Date = DateTime.UtcNow, Weight = 1.0 };
                        _db.Tournaments.Add(tournament);
                        await _db.SaveChangesAsync();
                    }

                    // Initiate local round object from either the database or API response
                    var round = _db.Rounds.Local
                        .FirstOrDefault(r => r.TournamentId == tournamentId &&
                                             r.Division == division &&
                                             r.RoundNumber == actualRoundNumber)
                        ?? await _db.Rounds
                            .FirstOrDefaultAsync(r => r.TournamentId == tournamentId &&
                                                      r.Division == division &&
                                                      r.RoundNumber == actualRoundNumber);
                    if (round == null)
                    {
                        round = new Round { TournamentId = tournamentId, Division = division, RoundNumber = actualRoundNumber };
                        _db.Rounds.Add(round);
                        await _db.SaveChangesAsync();
                    }
                    int roundId = round.RoundId;

                    var playerMap = new Dictionary<int, RoundScore>(); // Initialize dictionary of RoundScore objects

                    // Assign round info for every player present in API response
                    foreach (var p in roundData["scores"])
                    {
                        int playerId = p["ResultID"]?.Value<int>() ?? 0;
                        string name = $"{p["FirstName"]} {p["LastName"]}".Trim();
                        int place = p["RunningPlace"]?.Value<int>() ?? 0;
                        int roundScore = p["RoundtoPar"]?.Value<int>() ?? 0;
                        int toPar = p["ParThruRound"]?.Value<int>() ?? 0;

                        var player = await GetOrCreatePlayerAsync(playerId, name); // Initialize Player object

                        var rs = new RoundScore // Initialize RoundScore object
                        {
                            PlayerId = playerId,
                            RoundId = roundId,
                            Division = division,
                            RunningPlace = place,
                            RoundToPar = roundScore,
                            RunningToPar = toPar
                        };

                        // Adds object to dictionary if the object's player ID doesn't exist in the dictionary yet
                        if (!playerMap.ContainsKey(playerId))
                            playerMap[playerId] = rs;
                    }

                    // Parse round stats API response
                    string roundStatsUrl = $"https://www.pdga.com/api/v1/feat/stats/round-stats/{liveRoundId}";
                    var roundStatsJson = await _httpClient.GetStringAsync(roundStatsUrl);
                    var roundStatsArray = JArray.Parse(roundStatsJson);

                    // Assign API stats to RoundScore attributes
                    foreach (var playerData in roundStatsArray)
                    {
                        var liveResult = playerData["score"]?["liveResult"];
                        if (liveResult == null) continue;

                        int playerId = liveResult["resultId"]?.Value<int>() ?? 0;
                        if (!playerMap.TryGetValue(playerId, out var rs)) continue;

                        foreach (var stat in playerData["stats"] ?? new JArray())
                        {
                            int statId = stat["statId"]?.Value<int>() ?? 0;
                            double statValue = stat["statValue"]?.Type == JTokenType.Null ? 0.0 : stat["statValue"]?.Value<double>() ?? 0.0;
                            int statCount = stat["statValue"]?.Type == JTokenType.Null ? 0 : stat["statValue"]?.Value<int>() ?? 0;

                            switch (statId)
                            {
                                case 1: rs.Fairway = Math.Round(statValue, 0); break;
                                case 2: rs.C1InReg = Math.Round(statValue, 0); break;
                                case 3: rs.C2InReg = Math.Round(statValue, 0); break;
                                case 4: rs.Parked = Math.Round(statValue, 1); break;
                                case 5: rs.Scramble = Math.Round(statValue, 0); break;
                                case 6: rs.C1Putting = Math.Round(statValue, 0); break;
                                case 7: rs.C1xPutting = Math.Round(statValue, 0); break;
                                case 8: rs.C2Putting = Math.Round(statValue, 0); break;
                                case 9: rs.ObRate = Math.Round(statValue, 1); break;
                                case 10: rs.BirdieMinus = Math.Round(statValue, 0); break;
                                case 11: rs.DoubleBogeyPlus = Math.Round(statValue, 1); break;
                                case 12: rs.BogeyPlus = Math.Round(statValue, 0); break;
                                case 13: rs.Par = Math.Round(statValue, 0); break;
                                case 14: rs.Birdie = Math.Round(statValue, 0); break;
                                case 15: rs.EagleMinus = Math.Round(statValue, 1); break;
                                case 16: rs.TotalPuttDistance = statCount; break;
                                case 17: rs.LongThrowIn = statCount; break;
                                case 18: rs.AvgPuttDistance = Math.Round(statValue, 1); break;
                                default: continue;
                            }
                        }
                    }

                    // Parse round strokes gained stats API response
                    string strokesUrl = $"https://www.pdga.com/api/v1/feat/stats/strokes-gained/{liveRoundId}";
                    var strokesJson = await _httpClient.GetStringAsync(strokesUrl);
                    var strokesArray = JArray.Parse(strokesJson);

                    // Assign API strokes gained stats to RoundScore attributes
                    foreach (var sg in strokesArray)
                    {
                        var liveResult = sg["score"]?["liveResult"];
                        if (liveResult == null) continue;

                        int playerId = liveResult["resultId"]?.Value<int>() ?? 0;
                        if (!playerMap.TryGetValue(playerId, out var rs)) continue;

                        foreach (var stat in sg["stats"] ?? new JArray())
                        {
                            int statId = stat["statId"]?.Value<int>() ?? 0;
                            double statValue = stat["statValue"]?.Type == JTokenType.Null ? 0.0 : stat["statValue"]?.Value<double>() ?? 0.0;

                            switch (statId)
                            {
                                case 100: rs.StrokesGainedTotal = Math.Round(statValue, 2); break;
                                case 101: rs.StrokesGainedPutting = Math.Round(statValue, 2); break;
                                case 102: rs.StrokesGainedTeeToGreen = Math.Round(statValue, 2); break;
                                case 104: rs.StrokesGainedC1xPutting = Math.Round(statValue, 2); break;
                                case 105: rs.StrokesGainedC2Putting = Math.Round(statValue, 2); break;
                                default: continue;
                            }
                        }
                    }

                    // Adds RoundScore object if no RoundScore object with matching composite keys exists in database 
                    foreach (var rs in playerMap.Values)
                    {
                        var existing = await _db.RoundScores.FindAsync(rs.RoundId, rs.PlayerId);
                        if (existing == null)
                        {
                            _db.RoundScores.Add(rs);
                        }
                        else
                        {
                            existing.RunningPlace = rs.RunningPlace;
                            existing.RoundToPar = rs.RoundToPar;
                            existing.RunningToPar = rs.RunningToPar;
                            existing.Fairway = rs.Fairway;
                            existing.C1InReg = rs.C1InReg;
                            existing.C2InReg = rs.C2InReg;
                            existing.Parked = rs.Parked;
                            existing.Scramble = rs.Scramble;
                            existing.C1Putting = rs.C1Putting;
                            existing.C1xPutting = rs.C1xPutting;
                            existing.C2Putting = rs.C2Putting;
                            existing.ObRate = rs.ObRate;
                            existing.BirdieMinus = rs.BirdieMinus;
                            existing.DoubleBogeyPlus = rs.DoubleBogeyPlus;
                            existing.BogeyPlus = rs.BogeyPlus;
                            existing.Par = rs.Par;
                            existing.Birdie = rs.Birdie;
                            existing.EagleMinus = rs.EagleMinus;
                            existing.TotalPuttDistance = rs.TotalPuttDistance;
                            existing.LongThrowIn = rs.LongThrowIn;
                            existing.AvgPuttDistance = rs.AvgPuttDistance;
                            existing.StrokesGainedTotal = rs.StrokesGainedTotal;
                            existing.StrokesGainedPutting = rs.StrokesGainedPutting;
                            existing.StrokesGainedTeeToGreen = rs.StrokesGainedTeeToGreen;
                            existing.StrokesGainedC1xPutting = rs.StrokesGainedC1xPutting;
                            existing.StrokesGainedC2Putting = rs.StrokesGainedC2Putting;

                            _db.Entry(existing).State = EntityState.Modified;
                        }

                        finalPlayerMap = playerMap; // Saving the final round playerMap
                    }

                    await _db.SaveChangesAsync(); // Save RoundScore object to database
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing round {RoundNumber} for tournament {TournamentId}.", pdgaRound, tournamentId);
                    httpError = true;
                    continue;
                }
            }

            // Return final round RoundScore dictionary
            return finalPlayerMap != null
                ? GetFinalRoundStats(finalPlayerMap)
                : new Dictionary<int, (int Place, int ToPar)>();
        }

        // Gets final round placements and scores for each player
        private Dictionary<int, (int Place, int ToPar)> GetFinalRoundStats(Dictionary<int, RoundScore> finalPlayerMap)
        {
            var finalRoundStats = new Dictionary<int, (int Place, int ToPar)>();

            foreach (var kvp in finalPlayerMap)
            {
                finalRoundStats[kvp.Key] = (kvp.Value.RunningPlace, kvp.Value.RunningToPar);
            }

            return finalRoundStats;
        }
    }
}
