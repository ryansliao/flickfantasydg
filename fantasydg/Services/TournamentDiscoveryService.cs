using fantasydg.Data;
using fantasydg.Models;
using fantasydg.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

public class TournamentDiscoveryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TournamentDiscoveryService> _logger;
    private static readonly TimeZoneInfo PacificTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
    private DateTime _lastRunDate = DateTime.MinValue;

    public TournamentDiscoveryService(IServiceProvider serviceProvider, ILogger<TournamentDiscoveryService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    // Public controller-safe method
    public async Task RunManualDiscoveryAsync(DateTime nowPT, int leagueId)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dataService = scope.ServiceProvider.GetRequiredService<DataService>();
        var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();

        var league = await db.Leagues.FirstOrDefaultAsync(l => l.LeagueId == leagueId);
        if (league == null)
        {
            _logger.LogWarning("League {LeagueId} not found. Skipping tournament discovery.", leagueId);
            return;
        }

        await DiscoverNewTournamentsAsync(nowPT, db, dataService, httpClient, league);
    }

    // Background worker auto-executes this
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nowPT = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PacificTimeZone);

            if (_lastRunDate.Date != nowPT.Date && nowPT.Hour == 0)
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var dataService = scope.ServiceProvider.GetRequiredService<DataService>();
                var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();

                try
                {
                    var leagues = await db.Leagues.AsNoTracking().ToListAsync();

                    foreach (var league in leagues)
                    {
                        _logger.LogInformation("Midnight discovery for league {LeagueId} on {Date}", league.LeagueId, nowPT.Date);
                        await DiscoverNewTournamentsAsync(nowPT, db, dataService, httpClient, league);
                    }

                    _lastRunDate = nowPT.Date;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Tournament discovery failed.");
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    // Private shared logic
    private async Task DiscoverNewTournamentsAsync(DateTime nowPT, ApplicationDbContext db, DataService dataService, HttpClient httpClient, League league)
    {
        var url = "https://www.pdga.com/apps/tournament/live-api/live_results_fetch_recent_events";

        try
        {
            var json = await httpClient.GetStringAsync(url);
            var tournaments = JObject.Parse(json)?["data"]?["Tournaments"] as JArray;
            bool shouldFetchMPO = league.IncludeMPO;
            bool shouldFetchFPO = league.IncludeFPO;
            bool anyNewDiscovered = false;

            if (tournaments != null)
            {
                foreach (var t in tournaments)
                {
                    int id = t["TournID"]?.Value<int>() ?? 0;
                    string rawTier = t["RawTier"]?.ToString();

                    if ((rawTier == "ES" || rawTier == "M") && id > 0)
                    {
                        bool exists = await db.Tournaments.AnyAsync(x => x.Id == id);
                        if (exists) continue;

                        _logger.LogInformation("Found new tournament: {Id} ({Tier})", id, rawTier);

                        bool discovered = false;

                        if (league.IncludeMPO)
                        {
                            await dataService.FetchTournaments(id, "MPO", league);
                            discovered = true;
                        }

                        if (league.IncludeFPO)
                        {
                            await dataService.FetchTournaments(id, "FPO", league);
                            discovered = true;
                        }

                        if (discovered)
                        {
                            anyNewDiscovered = true;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during DiscoverNewTournamentsAsync");
        }
    }
}