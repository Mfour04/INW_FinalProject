using Domain.Entities.System;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Contracts.Response;
using Shared.SystemHelpers.TokenGenerate;
using System.Security.Claims;
using System.Text;

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
                .AddHttpContextAccessor()
                .AddAuthentication(configuration)
                .AddAuthorization()
                .AddPersistence();
            services.Configure<EmailSettings>(
                configuration.GetSection("EmailSettings"));
            services.Configure<CloudinarySettings>(
                configuration.GetSection("CloudinarySettings"));
            return services;
        }

        private static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            services.AddSingleton<MongoDBHelper>();
            services.AddSingleton<JwtHelpers>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<INovelRepository, NovelRepository>();
            services.AddScoped<IChapterRepository, ChapterRepository>();
            services.AddScoped<ITagRepository, TagRepository>();


            services.AddScoped<IBadgeRepository, BadgeRepository>();
            services.AddScoped<IBadgeProgressRepository, BadgeProgressRepository>();
            services.AddScoped<IForumPostRepository, ForumPostRepository>();
            services.AddScoped<IForumPostLikeRepository, ForumPostLikeRepository>();
            services.AddScoped<IForumCommentRepository, ForumCommentRepository>();
            services.AddScoped<ICommentLikeRepository, CommentLikeRepository>();
            services.AddScoped<IAuthorEarningRepository, AuthorEarningRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<ITransactionLogRepository, TransactionLogRepository>();
            services.AddScoped<IPurchaserRepository, PurchaserRepository>();
            services.AddScoped<IUserBankAccountRepository, UserBankAccountRepository>();
            services.AddScoped<IUserFollowRepository, UserFollowRepository>();

            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IReadingProcessRepository, ReadingProcessRepository>();
            services.AddScoped<INovelFollowRepository, NovelFollowRepository>();
            services.AddScoped<IRatingRepository, RatingRepository>();
            services.AddScoped<INovelViewTrackingRepository, NovelViewTrackingRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IOpenAIRepository, OpenAIRepository>();
            return services;
        }

        private static IServiceCollection AddAuthentication(
    this IServiceCollection services,
    IConfiguration configuration
)
        {
            
            return services;
        }

    }
}
