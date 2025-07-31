using fantasydg.Data;
using fantasydg.Services;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;

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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nowPT = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PacificTimeZone);

            // Only run once per day at/after midnight PT
            if (_lastRunDate.Date != nowPT.Date && nowPT.Hour == 0)
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var dataService = scope.ServiceProvider.GetRequiredService<DataService>();
                var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();

                try
                {
                    _logger.LogInformation("Midnight tournament discovery running for {Date}", nowPT.Date);
                    await DiscoverNewTournamentsAsync(nowPT, db, dataService, httpClient);
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

    private async Task DiscoverNewTournamentsAsync(DateTime nowPT, ApplicationDbContext db, DataService dataService, HttpClient httpClient)
    {
        var url = "https://www.pdga.com/apps/tournament/live-api/live_results_fetch_recent_events";

        try
        {
            var json = await httpClient.GetStringAsync(url);
            var tournaments = JObject.Parse(json)?["data"]?["Tournaments"] as JArray;

            if (tournaments != null)
            {
                foreach (var t in tournaments)
                {
                    int id = t["TournID"]?.Value<int>() ?? 0;
                    string rawTier = t["RawTier"]?.ToString();

                    if ((rawTier == "ES" || rawTier == "M") && id > 0)
                    {
                        bool exists = await db.Tournaments.AnyAsync(x => x.Id == id);
                        if (!exists)
                        {
                            _logger.LogInformation("Found new tournament: {Id} ({Tier})", id, rawTier);
                            await dataService.FetchTournaments(id, "MPO");
                            await dataService.FetchTournaments(id, "FPO");
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
