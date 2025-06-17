using Domain.Entities.System;
using Infrastructure.Common;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.SystemHelpers.TokenGenerate;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services
                // .AddHttpContextAccessor()
                .AddServices()
                .AddAuthentication(configuration)
                // .AddAuthorization()
                .AddPersistence();
            services.Configure<EmailSettings>(
                configuration.GetSection("EmailSettings"));
            services.Configure<CloudinarySettings>(
                configuration.GetSection("CloudinarySettings"));
            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
            return services;
        }

        private static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            services.AddSingleton<MongoDBHelper>();
            services.AddSingleton<JwtHelpers>();
            // services.AddScoped<IRoomFeatureRepository, RoomFeatureRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<INovelRepository, NovelRepository>();
            services.AddScoped<IChapterRepository, ChapterRepository>();
            services.AddScoped<IOwnershipRepository, OwnershipRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddTransient<IEmailService, EmailService>();

            services.AddScoped<IBadgeRepository, BadgeRepository>();
            services.AddScoped<IForumPostRepository, ForumPostRepository>();
            services.AddScoped<IForumPostLikeRepository, ForumPostLikeRepository>();
            services.AddScoped<IForumCommentRepository, ForumCommentRepository>();
            services.AddScoped<ICommentLikeRepository, CommentLikeRepository>();

            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ICloudDinaryService, CloudDinaryService>();
            services.AddScoped<IReportRepository, ReportRepository>();
            return services;
        }

        private static IServiceCollection AddAuthentication(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.Section));

            services
                .ConfigureOptions<JwtBearerTokenValidationConfiguration>()
                .AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            return services;
        }
    }
}
