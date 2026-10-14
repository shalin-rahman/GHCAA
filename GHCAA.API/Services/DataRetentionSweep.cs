using GHCAA.Application.Interfaces;
using GHCAA.Domain;

namespace GHCAA.API.Services;

// 98.14. Trims old log rows once a day. Program.cs awaits the database bootstrap before the host
// starts, so the first pass runs straight away. The schedule is not saved anywhere: Render restarts
// the service more often than once a day, and a purge is safe to repeat, so each boot runs one pass
// rather than waiting out an interval it may not live through.
public sealed class DataRetentionSweep(IServiceScopeFactory scopes, ILogger<DataRetentionSweep> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Leave the host's start path before touching the database.
        await Task.Yield();
        await RunOnceAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromHours(Constants.Retention.SweepHours));
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await RunOnceAsync(stoppingToken);
    }

    public async Task RunOnceAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            var deleted = await scope.ServiceProvider.GetRequiredService<IDataRetentionService>().PurgeAsync(DateTime.UtcNow, ct);
            foreach (var (table, count) in deleted.Where(d => d.Value > 0))
                logger.LogInformation("Retention removed {Count} old row(s) from {Table}.", count, table);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            // One bad pass must not stop the timer. Nothing is half-done, so the next pass starts clean.
            logger.LogError(ex, "Data retention sweep failed.");
        }
    }
}
