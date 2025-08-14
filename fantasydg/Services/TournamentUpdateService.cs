using fantasydg.Data;
using fantasydg.Services;
using Microsoft.EntityFrameworkCore;

public class TournamentUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TournamentUpdateService> _logger;
    private static readonly TimeZoneInfo PacificTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

    public TournamentUpdateService(IServiceProvider serviceProvider, ILogger<TournamentUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    // Public method for controller use
    public async Task RunManualUpdateAsync(DateTime nowPT)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dataService = scope.ServiceProvider.GetRequiredService<DataService>();
        await UpdateTournamentsAsync(nowPT, db, dataService);
    }

    // BackgroundService loop
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nowPT = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PacificTimeZone);

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var dataService = scope.ServiceProvider.GetRequiredService<DataService>();

            try
            {
                await UpdateTournamentsAsync(nowPT, db, dataService);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TournamentUpdateService failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(20), stoppingToken);
        }
    }

    // Shared logic used by both paths
    private async Task UpdateTournamentsAsync(DateTime nowPT, ApplicationDbContext db, DataService dataService)
    {
        var tournaments = await db.Tournaments
            .Where(t => t.StartDate <= nowPT && t.EndDate >= nowPT)
            .ToListAsync();

        foreach (var tournament in tournaments)
        {
            var leagues = await db.Leagues.ToListAsync();

            foreach (var league in leagues)
            {
                bool include = (tournament.Division == "MPO" && league.IncludeMPO) ||
                               (tournament.Division == "FPO" && league.IncludeFPO);

                if (!include) continue;

                await dataService.FetchTournaments(tournament.Id, tournament.Division, league);
            }
        }

        using var scope = _serviceProvider.CreateScope();
        var playerService = scope.ServiceProvider.GetRequiredService<PlayerService>();
        await playerService.UpdateAllWorldRankingsAsync(includeMPO: true, includeFPO: false);
    }
}
