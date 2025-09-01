using Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.Services.Implements
{
    public class NotificationCleanUpService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public NotificationCleanUpService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try { await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); } catch { }
            
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

                try
                {
                    await notificationRepo.DeleteOldReadNotificationsAsync();
                    Console.WriteLine($"[NotificationCleanup] Executed at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[NotificationCleanup] ERROR: {ex.Message}");
                }

                // Delay tiếp 24 tiếng
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
