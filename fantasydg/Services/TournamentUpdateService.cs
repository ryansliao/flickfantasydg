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
                var tournaments = await db.Tournaments
                    .Where(t => t.StartDate <= nowPT && t.EndDate >= nowPT)
                    .ToListAsync();

                foreach (var tournament in tournaments)
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
                        _logger.LogWarning("Error updating tournament {Id}: {Msg}", tournament.Id, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TournamentUpdateService failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(20), stoppingToken);
        }
    }
}
