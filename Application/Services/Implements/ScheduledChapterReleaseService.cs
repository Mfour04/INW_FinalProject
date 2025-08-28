using Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Helpers;

namespace Application.Services.Implements
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
                var novelRepo = scope.ServiceProvider.GetRequiredService<INovelRepository>();

                var releasedChapters = await chapterRepo.ReleaseScheduledAndReturnAsync();
                Console.WriteLine($"[ChapterRelease] Released {releasedChapters.Count} chapter(s) at {TimeHelper.NowVN:yyyy-MM-dd HH:mm:ss} VN");

                // ✅ cập nhật price cho các novel liên quan
                var novelIds = releasedChapters.Select(c => c.novel_id).Distinct().ToList();
                foreach (var novelId in novelIds)
                {
                    await novelRepo.UpdateNovelPriceAsync(novelId);
                    Console.WriteLine($"[ChapterRelease] Updated price for novel {novelId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChapterRelease] Error during execution: {ex.Message}");
            }
        }

    }
}
