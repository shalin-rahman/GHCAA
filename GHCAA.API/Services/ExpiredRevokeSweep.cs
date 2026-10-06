using System;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GHCAA.API.Services;

// 37.13x. An emergency revoke that nobody approves or rejects still needs its expired audit row.
// The first pass waits one interval, so a restart does not race the database bootstrap.
public sealed class ExpiredRevokeSweep(IServiceScopeFactory scopes, ILogger<ExpiredRevokeSweep> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(Constants.Elections.ExpiredRevokeSweepMinutes));
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await RunOnceAsync(stoppingToken);
    }

    public async Task RunOnceAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            var written = await scope.ServiceProvider.GetRequiredService<IElectionApprovalService>().AuditExpiredRevokesAsync(ct);
            if (written > 0)
                logger.LogInformation("Wrote the expired audit row for {Count} emergency revoke request(s).", written);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            // One bad pass must not stop the timer. The rows keep their key, so the next pass tries again.
            logger.LogError(ex, "Expired emergency revoke sweep failed.");
        }
    }
}
