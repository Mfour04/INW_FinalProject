using Application.Services.Implements;
using Application.Services.Interfaces;
using Domain.Entities.System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
           this IServiceCollection services,
           IConfiguration configuration
       )
        {
            services
                .AddServices()
                .AddPersistence();
            services.Configure<EmailSettings>(
                configuration.GetSection("EmailSettings"));
            services.Configure<CloudinarySettings>(
                configuration.GetSection("CloudinarySettings"));
            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddHostedService<NotificationCleanUpService>();
            services.AddSignalR();
            return services;
        }

        private static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            services.AddTransient<IEmailService, EmailService>();
            services.AddScoped<ICloudDinaryService, CloudDinaryService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IChapterHelperService, ChapterHelperService>();
            services.AddScoped<ICacheService, CacheService>();
            return services;
        }
    }
}