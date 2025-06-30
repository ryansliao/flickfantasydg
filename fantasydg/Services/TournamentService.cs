using fantasydg.Data;
using fantasydg.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

public class TournamentService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TournamentService> _logger;
    private static readonly TimeZoneInfo PacificTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
    private DateTime _lastDiscoveryDate = DateTime.MinValue;

    public TournamentService(IServiceProvider serviceProvider, ILogger<TournamentService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /* Test Code
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var nowPT = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PacificTimeZone);

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dataService = scope.ServiceProvider.GetRequiredService<DataService>();

        try
        {
            _logger.LogInformation("🏁 TournamentService starting — running UpdateActiveTournamentsAsync...");

            await UpdateActiveTournamentsAsync(nowPT, db, dataService);

            _logger.LogInformation("✅ UpdateActiveTournamentsAsync completed at startup.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during UpdateActiveTournamentsAsync at startup.");
        }

        // Keep service alive for future scheduling if needed
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    } */

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nowPT = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PacificTimeZone);

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var dataService = scope.ServiceProvider.GetRequiredService<DataService>();
            var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();

            try
            {
                if (nowPT.DayOfWeek == DayOfWeek.Tuesday && nowPT.Hour == 0 && _lastDiscoveryDate.Date != nowPT.Date)
                {
                    await DiscoverNewTournamentsAsync(nowPT, db, dataService, httpClient);
                    await WeeklyTournamentRefreshAsync(nowPT, db, dataService);
                    _lastDiscoveryDate = nowPT.Date;
                }
                await UpdateActiveTournamentsAsync(nowPT, db, dataService);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TournamentService encountered an error.");
            }

            if (nowPT.Hour >= 10 && nowPT.Hour < 19)
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            else
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    // Auto-fetch new ES/M tournaments every Tuesday at midnight
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
                    int id = t["TournamentID"]?.Value<int>() ?? 0;
                    string rawTier = t["RawTier"]?.ToString();

                    if ((rawTier == "ES" || rawTier == "M") && id > 0)
                    {
                        bool exists = await db.Tournaments.AnyAsync(x => x.Id == id);
                        if (!exists)
                        {
                            _logger.LogInformation("Auto-discovering new tournament: {Id} ({Tier})", id, rawTier);
                            await dataService.FetchTournaments(id, "MPO");
                            await dataService.FetchTournaments(id, "FPO");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering new tournaments.");
        }
    }

    // Auto-updates tournaments from 10AM-7PM PT between start date and end date
    private async Task UpdateActiveTournamentsAsync(DateTime nowPT, ApplicationDbContext db, DataService dataService)
    {
        var tournaments = await db.Tournaments.ToListAsync();

        foreach (var tournament in tournaments)
        {
            _logger.LogInformation("Checking tournament {Id}: {Name}, Division={Division}, Start={Start}, End={End}",
                tournament.Id, tournament.Name, tournament.Division, tournament.StartDate, tournament.EndDate);

            if (tournament.StartDate <= nowPT && nowPT <= tournament.EndDate && !string.IsNullOrEmpty(tournament.Division))
            {
                try
                {
                    _logger.LogInformation("Updating active tournament: {Id}", tournament.Id);
                    await dataService.FetchTournaments(tournament.Id, tournament.Division);

                    tournament.LastUpdatedTime = DateTime.UtcNow;
                    db.Entry(tournament).Property(t => t.LastUpdatedTime).IsModified = true;
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Skipping tournament {Id} — outside date range or missing division");
                }
            }
        }
    }

    // Updates every tournament in the database for that year every week on tuesday midnight
    private async Task WeeklyTournamentRefreshAsync(DateTime nowPT, ApplicationDbContext db, DataService dataService)
    {
        int currentYear = nowPT.Year;

        var tournaments = await db.Tournaments
            .Where(t => t.StartDate.Year == currentYear && !string.IsNullOrEmpty(t.Division))
            .ToListAsync();

        foreach (var tournament in tournaments)
        {
            try
            {
                _logger.LogInformation("Weekly refresh: Updating tournament {Id}", tournament.Id);
                await dataService.FetchTournaments(tournament.Id, tournament.Division);

                tournament.LastUpdatedTime = DateTime.UtcNow;
                db.Entry(tournament).Property(t => t.LastUpdatedTime).IsModified = true;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Weekly refresh failed for tournament {Id}", tournament.Id);
            }
        }
    }
}
