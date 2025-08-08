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
            .Include(t => t.LeagueTournaments)
                .ThenInclude(lt => lt.League)
            .Where(t => t.StartDate <= nowPT && t.EndDate >= nowPT)
            .ToListAsync();

        bool anyUpdated = false;

        foreach (var tournament in tournaments)
        {
            try
            {
                _logger.LogInformation("Checking tournament {Id} ({Division})", tournament.Id, tournament.Division);

                bool updated = false;

                foreach (var lt in tournament.LeagueTournaments)
                {
                    var league = lt.League;
                    if (league == null)
                    {
                        _logger.LogWarning("Skipping LeagueTournament without a valid League reference (TournamentId: {Id}, Division: {Division})", tournament.Id, tournament.Division);
                        continue;
                    }

                    bool include = (tournament.Division == "MPO" && league.IncludeMPO) ||
                                   (tournament.Division == "FPO" && league.IncludeFPO);

                    if (!include)
                    {
                        _logger.LogInformation("Skipping tournament {Id} for league {LeagueId} — Division {Division} is excluded", tournament.Id, league.LeagueId, tournament.Division);
                        continue;
                    }

                    _logger.LogInformation("Fetching tournament {Id} ({Division}) for league {LeagueId}", tournament.Id, tournament.Division, league.LeagueId);

                    await dataService.FetchTournaments(tournament.Id, tournament.Division, league);
                    updated = true;
                }

                if (updated)
                {
                    tournament.LastUpdatedTime = DateTime.UtcNow;
                    db.Entry(tournament).Property(t => t.LastUpdatedTime).IsModified = true;
                    await db.SaveChangesAsync();
                    anyUpdated = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error updating tournament {Id}: {Msg}", tournament.Id, ex.Message);
            }
        }

        if (anyUpdated)
        {
            using var scope = _serviceProvider.CreateScope();
            var playerService = scope.ServiceProvider.GetRequiredService<PlayerService>();
            await playerService.UpdateAllWorldRankingsAsync(includeMPO: true, includeFPO: false);
        }
    }
}
