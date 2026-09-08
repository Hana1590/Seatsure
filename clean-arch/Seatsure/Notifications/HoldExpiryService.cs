using Seatsure.Application.Services.Interfaces;

namespace Seatsure.Notifications;

public sealed class HoldExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HoldExpiryService> _logger;
    public HoldExpiryService(IServiceScopeFactory scopeFactory, ILogger<HoldExpiryService> logger)
    { _scopeFactory = scopeFactory; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IReservationService>();
                var count = await service.ExpirePendingHoldsAsync(stoppingToken);
                if (count > 0) _logger.LogInformation("Expired {Count} reservation holds.", count);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception ex) { _logger.LogError(ex, "Error while expiring reservation holds."); }
        }
    }
}
