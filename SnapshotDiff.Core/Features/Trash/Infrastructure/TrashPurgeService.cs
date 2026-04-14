using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SnapshotDiff.Features.Trash.Infrastructure;

/// <summary>
/// Background service that automatically purges expired trash items daily.
/// Runs once on startup, then every 24 hours.
/// </summary>
public sealed class TrashPurgeService(ITrashService trashService, ILogger<TrashPurgeService> logger) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await trashService.PurgeExpiredAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Trash purge failed");
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
