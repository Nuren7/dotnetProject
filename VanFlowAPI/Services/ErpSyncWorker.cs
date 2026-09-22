using Microsoft.EntityFrameworkCore;
using VanFlowAPI.Data;
using VanFlowAPI.Models;

namespace VanFlowAPI.Services;

public sealed class ErpSyncWorker(IServiceScopeFactory scopeFactory, ILogger<ErpSyncWorker> logger) : BackgroundService
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ERP sync worker started; polling every {Seconds} seconds.", SyncInterval.TotalSeconds);
        while (!stoppingToken.IsCancellationRequested)
        {
            await SimulateERPSync(stoppingToken);
            await Task.Delay(SyncInterval, stoppingToken);
        }
    }

    private async Task SimulateERPSync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var pendingOrders = await dbContext.VanOrders.Where(order => order.ErpStatus == "Awaiting sync").OrderBy(order => order.CreatedAtUtc).Take(20).ToListAsync(cancellationToken);
            foreach (var order in pendingOrders) { order.ErpStatus = "Synced to Business Central"; order.Status = OrderStatus.InProduction; order.LastSyncedAtUtc = DateTime.UtcNow; }
            if (pendingOrders.Count > 0) await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception exception) { logger.LogError(exception, "ERP sync failed; the next cycle will retry."); }
    }
}
