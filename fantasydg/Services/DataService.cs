// Existing using directives remain
using fantasydg.Data;
using fantasydg.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace fantasydg.Services
{
    public partial class DataService
    {
        private readonly HttpClient _httpClient;
        private readonly DGDbContext _db;
        private readonly ILogger<DataService> _logger;

        private async Task<string> GetTournamentNameAsync(int tournamentId)
        {
            try
            {
                var url = $"https://www.pdga.com/apps/tournament/live-api/live_results_fetch_event?TournID={tournamentId}";
                var json = await _httpClient.GetStringAsync(url);
                var name = JObject.Parse(json)?["data"]?["Name"]?.ToString();
                return string.IsNullOrWhiteSpace(name) ? $"Tournament {tournamentId}" : name;
            }
            catch
            {
                return $"Tournament {tournamentId}";
            }
        }

        public DataService(HttpClient httpClient, DGDbContext db, ILogger<DataService> logger)
        {
            _httpClient = httpClient;
            _db = db;
            _logger = logger;
        }

        public async Task FetchTournaments(int tournamentId, string division)
        {
            _logger.LogInformation("Fetching tournament-level stats for TournID {TournamentId}, Division {Division}", tournamentId, division);

            try
            {
                var statsUrl = $"https://www.pdga.com/api/v1/feat/stats/tournament-division-stats/{tournamentId}/{division}/";
                var statsJson = await _httpClient.GetStringAsync(statsUrl);
                var statsRoot = JObject.Parse(statsJson);
                var statsArray = statsRoot["resultStats"] as JArray;

                var strokesGainedUrl = $"https://www.pdga.com/api/v1/feat/stats/tournament-division-strokes-gained/{tournamentId}/{division}/";
                var strokesJson = await _httpClient.GetStringAsync(strokesGainedUrl);
                var strokesRoot = JObject.Parse(strokesJson);
                var strokesArray = strokesRoot["resultStats"] as JArray;

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
                        Date = DateTime.UtcNow,
                        Weight = 1.0
                    };
                    _db.Tournaments.Add(tournament);
                }

                tournament.Name = await GetTournamentNameAsync(tournamentId);

                await _db.SaveChangesAsync();

                var playerTournaments = new Dictionary<int, PlayerTournament>();

                foreach (var playerData in statsArray ?? new JArray())
                {
                    var result = playerData["result"];
                    var statList = playerData["stats"] as JArray;

                    if (result == null || statList == null)
                        continue;

                    int playerId = result["resultId"]?.Value<int>() ?? 0;
                    string name = $"{result["firstName"]} {result["lastName"]}".Trim();

                    var player = await _db.Players.FindAsync(playerId);
                    if (player == null)
                    {
                        player = new Player { PlayerId = playerId, Name = name };
                        _db.Players.Add(player);
                    }

                    var pt = new PlayerTournament
                    {
                        PlayerId = playerId,
                        TournamentId = tournamentId,
                        Division = division
                    };

                    foreach (var stat in statList)
                    {
                        int statId = stat["statId"]?.Value<int>() ?? 0;
                        double statValue = stat["statValue"]?.Type == JTokenType.Null ? 0.0 : stat["statValue"]?.Value<double>() ?? 0.0;
                        int statCount = stat["statCount"]?.Type == JTokenType.Null ? 0 : stat["statCount"]?.Value<int>() ?? 0;

                        switch (statId)
                        {
                            case 1: pt.Fairway = statValue; break;
                            case 2: pt.C1InReg = statValue; break;
                            case 3: pt.C2InReg = statValue; break;
                            case 4: pt.Parked = statValue; break;
                            case 5: pt.Scramble = statValue; break;
                            case 6: pt.C1Putting = statValue; break;
                            case 7: pt.C1xPutting = statValue; break;
                            case 8: pt.C2Putting = statValue; break;
                            case 9: pt.ObRate = statValue; break;
                            case 10: pt.BirdiePlus = statValue; break;
                            case 11: pt.DoubleBogeyPlus = statValue; break;
                            case 12: pt.BogeyPlus = statValue; break;
                            case 13: pt.Par = statValue; break;
                            case 14: pt.Birdie = statValue; break;
                            case 15: pt.EaglePlus = statValue; break;
                            case 16: pt.PuttDistance = statCount; break;
                            default: continue;
                        }
                    }

                    if (!playerTournaments.ContainsKey(playerId))
                        playerTournaments[playerId] = pt;
                }

                foreach (var sgData in strokesArray ?? new JArray())
                {
                    var result = sgData["result"];
                    var sgList = sgData["stats"] as JArray;

                    if (result == null || sgList == null)
                        continue;

                    int playerId = result["resultId"]?.Value<int>() ?? 0;
                    if (!playerTournaments.TryGetValue(playerId, out var pt)) continue;

                    foreach (var stat in sgList)
                    {
                        int statId = stat["statId"]?.Value<int>() ?? 0;
                        double statValue = stat["statValue"]?.Type == JTokenType.Null ? 0.0 : stat["statValue"]?.Value<double>() ?? 0.0;

                        switch (statId)
                        {
                            case 100: pt.StrokesGainedTotal = statValue; break;
                            case 101: pt.StrokesGainedPutting = statValue; break;
                            case 102: pt.StrokesGainedTeeToGreen = statValue; break;
                            case 104: pt.StrokesGainedC1xPutting = statValue; break;
                            case 105: pt.StrokesGainedC2Putting = statValue; break;
                            default: continue;
                        }
                    }
                }

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
                        existing.BirdiePlus = pt.BirdiePlus;
                        existing.DoubleBogeyPlus = pt.DoubleBogeyPlus;
                        existing.BogeyPlus = pt.BogeyPlus;
                        existing.Par = pt.Par;
                        existing.Birdie = pt.Birdie;
                        existing.EaglePlus = pt.EaglePlus;
                        existing.PuttDistance = pt.PuttDistance;
                        existing.StrokesGainedTotal = pt.StrokesGainedTotal;
                        existing.StrokesGainedPutting = pt.StrokesGainedPutting;
                        existing.StrokesGainedTeeToGreen = pt.StrokesGainedTeeToGreen;
                        existing.StrokesGainedC1xPutting = pt.StrokesGainedC1xPutting;
                        existing.StrokesGainedC2Putting = pt.StrokesGainedC2Putting;

                        _db.Entry(existing).State = EntityState.Modified;
                    }
                }

                await _db.SaveChangesAsync();
                _logger.LogInformation("Inserted or updated {Count} PlayerTournament entries for tournament {TournamentId}", playerTournaments.Count, tournamentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch tournament stats for {TournamentId} ({Division})", tournamentId, division);
            }
        }

        public async Task FetchRounds(int tournamentId, string division)
        {
            _logger.LogInformation("Fetching round-by-round stats for TournID {TournamentId}, Division {Division}", tournamentId, division);

            int actualRoundNumber = 0;
            for (int pdgaRound = 1; pdgaRound <= 12; pdgaRound++)
            {
                try
                {
                    string roundUrl = $"https://www.pdga.com/apps/tournament/live-api/live_results_fetch_round?TournID={tournamentId}&Division={division}&Round={pdgaRound}";
                    var roundJson = await _httpClient.GetStringAsync(roundUrl);
                    var roundData = JObject.Parse(roundJson)["data"];

                    if (roundData == null || roundData["scores"] == null)
                    {
                        _logger.LogWarning("No round data or scores for Tournament {0}, Division {1}, PDGA Round {2}", tournamentId, division, pdgaRound);
                        continue;
                    }

                    int? liveRoundId = (int?)roundData["live_round_id"];
                    if (liveRoundId == null) continue;

                    actualRoundNumber = (pdgaRound == 12) ? 4 : actualRoundNumber + 1;

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

                    var round = await _db.Rounds.FirstOrDefaultAsync(r =>
                        r.TournamentId == tournamentId &&
                        r.Division == division &&
                        r.RoundNumber == actualRoundNumber);

                    if (round == null)
                    {
                        round = new Round { TournamentId = tournamentId, Division = division, RoundNumber = actualRoundNumber };
                        _db.Rounds.Add(round);
                        await _db.SaveChangesAsync();
                    }

                    int roundId = round.RoundId;
                    var playerMap = new Dictionary<int, RoundScore>();

                    foreach (var p in roundData["scores"])
                    {
                        int playerId = p["ResultID"]?.Value<int>() ?? 0;
                        string name = $"{p["FirstName"]} {p["LastName"]}".Trim();
                        int place = p["RunningPlace"]?.Value<int>() ?? 0;
                        int roundScore = p["RoundtoPar"]?.Value<int>() ?? 0;
                        int toPar = p["ParThruRound"]?.Value<int>() ?? 0;

                        var player = await _db.Players.FindAsync(playerId);
                        if (player == null)
                        {
                            player = new Player { PlayerId = playerId, Name = name };
                            _db.Players.Add(player);
                        }

                        var rs = new RoundScore
                        {
                            PlayerId = playerId,
                            RoundId = roundId,
                            Division = division,
                            RunningPlace = place,
                            RoundToPar = roundScore,
                            RunningToPar = toPar
                        };

                        playerMap[playerId] = rs;
                    }

                    string roundStatsUrl = $"https://www.pdga.com/api/v1/feat/stats/round-stats/{liveRoundId}";
                    var roundStatsJson = await _httpClient.GetStringAsync(roundStatsUrl);
                    var roundStatsArray = JArray.Parse(roundStatsJson);

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
                            int statCount = stat["statCount"]?.Type == JTokenType.Null ? 0 : stat["statCount"]?.Value<int>() ?? 0;

                            switch (statId)
                            {
                                case 1: rs.Fairway = statValue; break;
                                case 2: rs.C1InReg = statValue; break;
                                case 3: rs.C2InReg = statValue; break;
                                case 4: rs.Parked = statValue; break;
                                case 5: rs.Scramble = statValue; break;
                                case 6: rs.C1Putting = statValue; break;
                                case 7: rs.C1xPutting = statValue; break;
                                case 8: rs.C2Putting = statValue; break;
                                case 9: rs.ObRate = statValue; break;
                                case 10: rs.BirdiePlus = statValue; break;
                                case 11: rs.DoubleBogeyPlus = statValue; break;
                                case 12: rs.BogeyPlus = statValue; break;
                                case 13: rs.Par = statValue; break;
                                case 14: rs.Birdie = statValue; break;
                                case 15: rs.EaglePlus = statValue; break;
                                case 16: rs.PuttDistance = statCount; break;
                            }
                        }
                    }

                    string strokesUrl = $"https://www.pdga.com/api/v1/feat/stats/strokes-gained/{liveRoundId}";
                    var strokesJson = await _httpClient.GetStringAsync(strokesUrl);
                    var strokesArray = JArray.Parse(strokesJson);

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
                                case 100: rs.StrokesGainedTotal = statValue; break;
                                case 101: rs.StrokesGainedPutting = statValue; break;
                                case 102: rs.StrokesGainedTeeToGreen = statValue; break;
                                case 104: rs.StrokesGainedC1xPutting = statValue; break;
                                case 105: rs.StrokesGainedC2Putting = statValue; break;
                            }
                        }
                    }

                    foreach (var rs in playerMap.Values)
                    {
                        var existing = await _db.RoundScores.FindAsync(rs.RoundId, rs.PlayerId);
                        if (existing == null)
                        {
                            _logger.LogInformation("Saving RoundScore for Player {0}, RoundId {1}, Division {2}", rs.PlayerId, rs.RoundId, division);
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
                            existing.BirdiePlus = rs.BirdiePlus;
                            existing.DoubleBogeyPlus = rs.DoubleBogeyPlus;
                            existing.BogeyPlus = rs.BogeyPlus;
                            existing.Par = rs.Par;
                            existing.Birdie = rs.Birdie;
                            existing.EaglePlus = rs.EaglePlus;
                            existing.PuttDistance = rs.PuttDistance;
                            existing.StrokesGainedTotal = rs.StrokesGainedTotal;
                            existing.StrokesGainedPutting = rs.StrokesGainedPutting;
                            existing.StrokesGainedTeeToGreen = rs.StrokesGainedTeeToGreen;
                            existing.StrokesGainedC1xPutting = rs.StrokesGainedC1xPutting;
                            existing.StrokesGainedC2Putting = rs.StrokesGainedC2Putting;

                            _db.Entry(existing).State = EntityState.Modified;
                        }
                    }

                    await _db.SaveChangesAsync();
                    _logger.LogInformation("Saved RoundScores for Tournament {0}, Division {1}, Round {2}", tournamentId, division, actualRoundNumber);

                    // Step 1: Identify the latest round number
                    var latestRound = await _db.Rounds
                        .Where(r => r.TournamentId == tournamentId && r.Division == division)
                        .OrderByDescending(r => r.RoundNumber)
                        .FirstOrDefaultAsync();

                    _logger.LogInformation("Latest Round for Tournament {0}, Division {1} is RoundNumber {2} (RoundId {3})",
                         tournamentId, division, latestRound?.RoundNumber, latestRound?.RoundId);

                    if (latestRound != null)
                    {
                        // Step 2: Get all RoundScores from the latest round
                        var latestScores = await _db.RoundScores
                            .Where(rs => rs.RoundId == latestRound.RoundId)
                            .ToListAsync();

                        foreach (var rs in latestScores)
                        {
                            var pt = await _db.PlayerTournaments
                                .FirstOrDefaultAsync(pt =>
                                    pt.TournamentId == tournamentId &&
                                    pt.Division == division &&
                                    pt.PlayerId == rs.PlayerId);

                            if (pt == null)
                            {
                                _logger.LogWarning("Missing PlayerTournament for PlayerId={0}, Division={1}, TournamentId={2} — creating new.",
                                    rs.PlayerId, division, tournamentId);

                                pt = new PlayerTournament
                                {
                                    PlayerId = rs.PlayerId,
                                    Division = division,
                                    TournamentId = tournamentId
                                };
                                _db.PlayerTournaments.Add(pt);
                            }

                            pt.Place = rs.RunningPlace;
                            pt.TotalToPar = rs.RunningToPar;
                            _db.Entry(pt).State = EntityState.Modified;
                          
                            _logger.LogInformation("Updating PT: Player {0}, ToPar={1}, Place={2}", pt.PlayerId, pt.TotalToPar, pt.Place);
                            await _db.SaveChangesAsync();
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing round {RoundNumber} for tournament {TournamentId}.", pdgaRound, tournamentId);
                    continue;
                }
            }
        }
    }
}
