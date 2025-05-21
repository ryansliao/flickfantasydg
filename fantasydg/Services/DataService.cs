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

        public async Task<Dictionary<int, List<Player>>> FetchRounds(int tournamentId, string division)
        {
            var roundDataMap = new Dictionary<int, List<Player>>();

            _logger.LogInformation("Fetching tournament stats for tournament_id {tournamentId}, {division} division:", tournamentId, division);

            int actualRoundNumber = 0;
            for (int roundNumber = 1; roundNumber <= 12; roundNumber++)
            {
                try
                {
                    string roundUrl = $"https://www.pdga.com/apps/tournament/live-api/live_results_fetch_round?TournID={tournamentId}&Division={division}&Round={roundNumber}";
                    var roundJson = await _httpClient.GetStringAsync(roundUrl);
                    var roundData = JObject.Parse(roundJson)["data"];

                    if (roundData == null || roundData["scores"] == null)
                        continue;

                    int? liveRoundId = (int?)roundData["live_round_id"];
                    if (liveRoundId == null)
                        continue;

                    bool? roundStarted = (bool?)roundData["tee_times"];

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

                    string roundStatsUrl = $"https://www.pdga.com/api/v1/feat/stats/round-stats/{liveRoundId}";
                    var roundStatsJson = await _httpClient.GetStringAsync(roundStatsUrl);
                    var roundStatsArray = JArray.Parse(roundStatsJson);

                    foreach (var playerData in roundStatsArray)
                    {
                        var liveResult = playerData["score"]?["liveResult"];
                        if (liveResult == null) continue;

                        int playerId = liveResult["resultId"]?.Value<int>() ?? 0;
                        if (!playerMap.ContainsKey(playerId)) continue;

                        var player = playerMap[playerId];
                        var roundStatList = playerData["stats"] as JArray;
                        if (roundStatList == null) continue;

                        foreach (var stat in roundStatList)
                        {
                            int statId = stat["statId"]?.Value<int>() ?? 0;
                            double statValue = stat["statValue"]?.Type == JTokenType.Null ? 0.0 : stat["statValue"]?.Value<double>() ?? 0.0;
                            int statCount = stat["statCount"]?.Type == JTokenType.Null ? 0 : stat["statCount"]?.Value<int>() ?? 0;

                            switch (statId)
                            {
                                case 1: player.Fairway = statValue; break;
                                case 2: player.C1InReg = statValue; break;
                                case 3: player.C2InReg = statValue; break;
                                case 4: player.Parked = statValue; break;
                                case 5: player.Scramble = statValue; break;
                                case 6: player.C1Putting = statValue; break;
                                case 7: player.C1xPutting = statValue; break;
                                case 8: player.C2Putting = statValue; break;
                                case 9: player.ObRate = statValue; break;
                                case 10: player.BirdiePlus = statValue; break;
                                case 11: player.DoubleBogeyPlus = statValue; break;
                                case 12: player.BogeyPlus = statValue; break;
                                case 13: player.Par = statValue; break;
                                case 14: player.Birdie = statValue; break;
                                case 15: player.EaglePlus = statValue; break;
                                case 16: player.PuttDistance = statCount; break;
                            }
                        }
                    }

                    string strokesGainedUrl = $"https://www.pdga.com/api/v1/feat/stats/strokes-gained/{liveRoundId}";
                    var strokesGainedJson = await _httpClient.GetStringAsync(strokesGainedUrl);
                    var strokesGainedArray = JArray.Parse(strokesGainedJson);

                    foreach (var playerData in strokesGainedArray)
                    {
                        var liveResult = playerData["score"]?["liveResult"];
                        if (liveResult == null) continue;

                        int playerId = liveResult["resultId"]?.Value<int>() ?? 0;
                        if (!playerMap.ContainsKey(playerId)) continue;

                        var player = playerMap[playerId];
                        var statList = playerData["stats"] as JArray;
                        if (statList == null) continue;

                        foreach (var stat in statList)
                        {
                            int statId = stat["statId"]?.Value<int>() ?? 0;
                            double statValue = stat["statValue"]?.Type == JTokenType.Null ? 0.0 : stat["statValue"]?.Value<double>() ?? 0.0;

                            switch (statId)
                            {
                                case 100: player.StrokesGainedTotal = statValue; break;
                                case 101: player.StrokesGainedPutting = statValue; break;
                                case 102: player.StrokesGainedTeeToGreen = statValue; break;
                                case 104: player.StrokesGainedC1xPutting = statValue; break;
                                case 105: player.StrokesGainedC2Putting = statValue; break;
                            }
                        }
                    }

                    if (playerMap.Count > 0)
                    {

                        if (roundNumber == 12)
                        {
                            actualRoundNumber = 4;
                        }
                        else { actualRoundNumber++; }
                            
                        roundDataMap[actualRoundNumber] = playerMap.Values.ToList();
                        _logger.LogInformation("Saved actual round {ActualRoundNumber} (PDGA round {PdgaRound}) with {Count} players",
                            actualRoundNumber, roundNumber, playerMap.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing round {RoundNumber} for tournament {TournamentId}.", roundNumber, tournamentId);
                    continue;
                }
            }

            return roundDataMap;
        }
    }
}