using fantasydg.Models;
using fantasydg.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace fantasydg.Services
{
    public class DataService
    {
        private readonly HttpClient _httpClient;
        private readonly DGDbContext _db;
        private readonly ILogger<DataService> _logger;

        public DataService(HttpClient httpClient, DGDbContext db, ILogger<DataService> logger)
        {
            _httpClient = httpClient;
            _db = db;
            _logger = logger;
        }

        public async Task<List<Player>> FetchAveragedTournamentStatsAsync(int tournamentId, string division)
        {
            var aggregatedPlayers = new Dictionary<int, List<Player>>();
            int roundCount = 0;

            for (int roundNumber = 1; roundNumber <= 12; roundNumber++)
            {
                try
                {
                    string roundUrl = $"https://www.pdga.com/apps/tournament/live-api/live_results_fetch_round?TournID={tournamentId}&Division={division}&Round={roundNumber}";
                    var roundJson = await _httpClient.GetStringAsync(roundUrl);
                    var roundData = JObject.Parse(roundJson)["data"];

                    if (roundData != null)
                    {
                        if (roundData["scores"] == null)
                            continue;

                        var playerCount = roundData["scores"]?.Count() ?? 0;
                        _logger.LogInformation("Found {Count} players in Round {Round}", playerCount, roundNumber);

                        bool roundStarted = (bool?)roundData["tee_times"] ?? false;
                        _logger.LogInformation("tee_times = {TeeTimes} for Round {Round}", roundStarted, roundNumber);

                        bool teeTimes = (bool?)roundData["tee_times"] ?? false;
                        int? liveRoundId = (int?)roundData["live_round_id"];
                        if (liveRoundId == null)
                            continue;

                        if (teeTimes && playerCount == 0)
                        {
                            _logger.LogWarning("Skipping Round {Round} — tee_times = true and no players", roundNumber);
                            continue;
                        }

                        _logger.LogInformation("Proceeding with Round {Round} — tee_times = {TeeTimes}, players = {PlayerCount}", roundNumber, teeTimes, playerCount);
                        var playerMap = new Dictionary<int, Player>();

                        foreach (var p in roundData["scores"])
                        {
                            int playerId = p["ResultID"]?.Value<int>() ?? 0;
                            string name = $"{p["FirstName"]} {p["LastName"]}".Trim();
                            int place = p["RunningPlace"]?.Value<int>() ?? 0;
                            int tournamentScore = p["ParThruRound"]?.Value<int>() ?? 0;
                            int roundScore = p["RoundtoPar"]?.Value<int>() ?? 0;

                            var player = new Player
                            {
                                Id = playerId,
                                Name = name,
                                Place = place,
                                TournamentScore = tournamentScore,
                                RoundScore = roundScore
                            };
                            playerMap[playerId] = player;
                        }

                        _logger.LogInformation("Fetching round stats for live_round_id {Id}", liveRoundId);
                        string statsUrl = $"https://www.pdga.com/api/v1/feat/stats/round-stats/{liveRoundId}";

                        try
                        {
                            var statsJson = await _httpClient.GetStringAsync(statsUrl);
                            var statsArray = JArray.Parse(statsJson);

                            foreach (var playerData in statsArray)
                            {
                                var liveResult = playerData["score"]?["liveResult"];
                                if (liveResult == null)
                                {
                                    _logger.LogWarning("liveResult missing for playerData");
                                    continue;
                                }

                                int playerId = liveResult["resultId"]?.Value<int>() ?? 0;
                                string name = $"{liveResult["firstName"]} {liveResult["lastName"]}".Trim();

                                if (!playerMap.ContainsKey(playerId))
                                {
                                    _logger.LogWarning("Player ID {Id} from stats not found in round player map — skipping", playerId);
                                    continue;
                                }

                                var player = playerMap[playerId];
                                player.Name = name;  // Reaffirm

                                var statList = playerData["stats"] as JArray;
                                if (statList == null) continue;

                                foreach (var stat in statList)
                                {
                                    int statId = stat["statId"]?.Value<int>() ?? 0;
                                    double statValue = stat["statValue"]?.Type == JTokenType.Null ? 0.0 : stat["statValue"]?.Value<double>() ?? 0.0;
                                    int statCount = stat["statCount"]?.Type == JTokenType.Null ? 0 : stat["statCount"]?.Value<int>() ?? 0;

                                    switch (statId)
                                    {
                                        case 1: player.Fairway = Math.Round(statValue, 2); break;
                                        case 2: player.C1InReg = Math.Round(statValue, 2); break;
                                        case 3: player.C2InReg = Math.Round(statValue, 2); break;
                                        case 4: player.Parked = Math.Round(statValue, 2); break;
                                        case 5: player.Scramble = Math.Round(statValue, 2); break;
                                        case 6: player.C1Putting = Math.Round(statValue, 2); break;
                                        case 7: player.C1xPutting = Math.Round(statValue, 2); break;
                                        case 8: player.C2Putting = Math.Round(statValue, 2); break;
                                        case 9: player.ObRate = Math.Round(statValue, 2); break;
                                        case 10: player.BirdiePlus = Math.Round(statValue, 2); break;
                                        case 11: player.DoubleBogeyPlus = Math.Round(statValue, 2); break;
                                        case 12: player.BogeyPlus = Math.Round(statValue, 2); break;
                                        case 13: player.Par = Math.Round(statValue, 2); break;
                                        case 14: player.Birdie = Math.Round(statValue, 2); break;
                                        case 15: player.EaglePlus = Math.Round(statValue, 2); break;
                                        case 16: player.PuttDistance = statCount; break;
                                    }
                                }
                            }

                            foreach (var player in playerMap.Values)
                            {
                                if (!aggregatedPlayers.ContainsKey(player.Id))
                                    aggregatedPlayers[player.Id] = new List<Player>();

                                aggregatedPlayers[player.Id].Add(player);
                            }

                            roundCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to fetch or parse stats for Round {Round} (live_round_id = {Id})", roundNumber, liveRoundId);
                            continue;
                        }

                        _logger.LogInformation("Fetching round strokes gained stats for live_round_id {Id}", liveRoundId);
                        string strokesGainedUrl = $"https://www.pdga.com/api/v1/feat/stats/strokes-gained/{liveRoundId}";

                        try
                        {
                            var strokesGainedJson = await _httpClient.GetStringAsync(strokesGainedUrl);
                            var strokesGainedArray = JArray.Parse(strokesGainedJson);

                            foreach (var playerData in strokesGainedArray)
                            {
                                var liveResult = playerData["score"]?["liveResult"];
                                if (liveResult == null)
                                {
                                    _logger.LogWarning("liveResult missing for playerData");
                                    continue;
                                }

                                int playerId = liveResult["resultId"]?.Value<int>() ?? 0;
                                string name = $"{liveResult["firstName"]} {liveResult["lastName"]}".Trim();

                                if (!playerMap.ContainsKey(playerId))
                                {
                                    _logger.LogWarning("Player ID {Id} from strokes gained stats not found in round player map — skipping", playerId);
                                    continue;
                                }

                                var player = playerMap[playerId];
                                player.Name = name;

                                var statList = playerData["stats"] as JArray;
                                if (statList == null) continue;

                                foreach (var stat in statList)
                                {
                                    int statId = stat["statId"]?.Value<int>() ?? 0;
                                    double statValue = stat["statValue"]?.Type == JTokenType.Null ? 0.0 : stat["statValue"]?.Value<double>() ?? 0.0;
                                    int statCount = stat["statCount"]?.Type == JTokenType.Null ? 0 : stat["statCount"]?.Value<int>() ?? 0;

                                    switch (statId)
                                    {
                                        case 100: player.StrokesGainedTotal = Math.Round(statValue, 2); break;
                                        case 101: player.StrokesGainedPutting = Math.Round(statValue, 2); break;
                                        case 102: player.StrokesGainedTeeToGreen = Math.Round(statValue, 2); break;
                                        case 104: player.StrokesGainedC1xPutting = Math.Round(statValue, 2); break;
                                        case 105: player.StrokesGainedC2Putting = Math.Round(statValue, 2); break;
                                    }
                                }
                            }

                            foreach (var player in playerMap.Values)
                            {
                                if (!aggregatedPlayers.ContainsKey(player.Id))
                                    aggregatedPlayers[player.Id] = new List<Player>();

                                aggregatedPlayers[player.Id].Add(player);
                            }

                            roundCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to fetch or parse strokes gained stats for Round {Round} (live_round_id = {Id})", roundNumber, liveRoundId);
                            continue;
                        }

                    }
                    else
                    {
                        _logger.LogWarning("roundData is null for Round {Round}", roundNumber);
                    }              
                    
                }
                catch (HttpRequestException httpEx) when (httpEx.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("Round {RoundNumber} not found (404) — skipping", roundNumber);
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing round {roundNumber} for tournament {tournamentId}.");
                    continue;
                }
            }

            // ✅ Now average across successful rounds only
            var averagedPlayers = new List<Player>();

            foreach (var kvp in aggregatedPlayers)
            {
                var players = kvp.Value;
                var player = new Player
                {
                    Id = kvp.Key,
                    Name = players.First().Name,
                    Place = players.Last().Place,
                    TournamentScore = players.Last().TournamentScore,
                    Fairway = Math.Round(players.Average(p => p.Fairway), 2),
                    C1InReg = Math.Round(players.Average(p => p.C1InReg), 2),
                    C2InReg = Math.Round(players.Average(p => p.C2InReg), 2),
                    Parked = Math.Round(players.Average(p => p.Parked), 2),
                    Scramble = Math.Round(players.Average(p => p.Scramble), 2),
                    C1Putting = Math.Round(players.Average(p => p.C1Putting), 2),
                    C1xPutting = Math.Round(players.Average(p => p.C1xPutting), 2),
                    C2Putting = Math.Round(players.Average(p => p.C2Putting), 2),
                    ObRate = Math.Round(players.Average(p => p.ObRate), 2),
                    BirdiePlus = Math.Round(players.Average(p => p.BirdiePlus), 2),
                    DoubleBogeyPlus = Math.Round(players.Average(p => p.DoubleBogeyPlus), 2),
                    BogeyPlus = Math.Round(players.Average(p => p.BogeyPlus), 2),
                    Par = Math.Round(players.Average(p => p.Par), 2),
                    Birdie = Math.Round(players.Average(p => p.Birdie), 2),
                    EaglePlus = Math.Round(players.Average(p => p.EaglePlus), 2),
                    PuttDistance = (int)players.Average(p => p.PuttDistance),
                    StrokesGainedTotal = Math.Round(players.Average(p => p.StrokesGainedTotal), 2),
                    StrokesGainedPutting = Math.Round(players.Average(p => p.StrokesGainedPutting), 2),
                    StrokesGainedTeeToGreen = Math.Round(players.Average(p => p.StrokesGainedTeeToGreen), 2),
                    StrokesGainedC1xPutting = Math.Round(players.Average(p => p.StrokesGainedC1xPutting), 2),
                    StrokesGainedC2Putting = Math.Round(players.Average(p => p.StrokesGainedC2Putting), 2)
                };

                averagedPlayers.Add(player);
            }
            return averagedPlayers;
        }
    }
}

