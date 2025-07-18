using Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Helpers;

namespace Application.Services
{
    public class ScheduledChapterReleaseService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly bool IsTestMode = false;

        public ScheduledChapterReleaseService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RunReleaseAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var nowVN = TimeHelper.NowVN;
                    var nextRunTimeVN = nowVN.Date.AddDays(1);
                    var delay = nextRunTimeVN - nowVN;

                    if (IsTestMode)
                    {
                        delay = TimeSpan.FromSeconds(5);
                        Console.WriteLine($"[ChapterRelease] [TEST MODE] Waiting 5s before next release.");
                    }
                    else
                    {
                        Console.WriteLine($"[ChapterRelease] Waiting until {nextRunTimeVN:yyyy-MM-dd HH:mm:ss} VN to release chapters...");
                    }

                    if (delay < TimeSpan.Zero)
                        delay = TimeSpan.Zero;

                    await Task.Delay(delay, stoppingToken);

                    await RunReleaseAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ChapterRelease] Error: {ex.Message}");
                }
            }
        }

        private async Task RunReleaseAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var chapterRepo = scope.ServiceProvider.GetRequiredService<IChapterRepository>();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                int releasedCount = await chapterRepo.ReleaseScheduledAsync();
                Console.WriteLine($"[ChapterRelease] Released {releasedCount} chapter(s) at {TimeHelper.NowVN:yyyy-MM-dd HH:mm:ss} VN");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChapterRelease] Error during execution: {ex.Message}");
            }
        }
    }
}
