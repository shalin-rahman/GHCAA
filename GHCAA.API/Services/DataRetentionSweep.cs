using GHCAA.Application.Interfaces;
using GHCAA.Domain;

namespace GHCAA.API.Services;

// 98.14. Trims old log rows once a day. The first pass waits a few minutes, so it does not race the
// database bootstrap.
public sealed class DataRetentionSweep(IServiceScopeFactory scopes, ILogger<DataRetentionSweep> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromMinutes(Constants.Retention.FirstSweepDelayMinutes), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }
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
