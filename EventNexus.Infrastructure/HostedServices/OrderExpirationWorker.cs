using EventNexus.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventNexus.Infrastructure.HostedServices;

public class OrderExpirationWorker : BackgroundService {
    private readonly IServiceScopeFactory _scopeFactory;

    public OrderExpirationWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var cleanupService = scope.ServiceProvider.GetRequiredService<IOrderCleanupService>();

                await cleanupService.CancelExpiresOrderAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ", ex.Message);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
