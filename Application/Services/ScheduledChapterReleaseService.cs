using Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class ScheduledChapterReleaseService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ScheduledChapterReleaseService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var nowUtc = DateTime.UtcNow;
                    var nextUtcMidnight = nowUtc.Date.AddDays(1);
                    var delay = nextUtcMidnight - nowUtc;

                    if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;

                    Console.WriteLine($"[ChapterRelease] Waiting until {nextUtcMidnight:yyyy-MM-dd HH:mm:ss} UTC to release chapters...");

                    await Task.Delay(delay, stoppingToken); // chờ đến 0h UTC

                    using var scope = _serviceProvider.CreateScope();
                    var chapterRepo = scope.ServiceProvider.GetRequiredService<IChapterRepository>();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    int releasedCount = await chapterRepo.ReleaseScheduledChaptersAsync();

                    Console.WriteLine($"[ChapterRelease] Released {releasedCount} chapter(s) at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ChapterRelease] Error: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
