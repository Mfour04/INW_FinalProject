using Application.Services.Implements;
using Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services
                .AddApplicationServices()
                .AddBackgroundJobs();
            return services;
        }

        private static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
        {
            services.AddHostedService<NotificationCleanUpService>();
            services.AddHostedService<TransactionCleanupService>();
            services.AddHostedService<ScheduledChapterReleaseService>();
            services.AddSignalR();
            return services;
        }

        private static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddTransient<IEmailService, EmailService>();
            services.AddScoped<ICloudDinaryService, CloudDinaryService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IChapterHelperService, ChapterHelperService>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<ICommentSpamGuard, CommentSpamGuard>();
            return services;
        }
    }
}