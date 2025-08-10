using Domain.Entities.System;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Contracts.Response;
using Shared.SystemHelpers.TokenGenerate;
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
            var jwtSettings = configuration.GetSection(JwtSettings.Section).Get<JwtSettings>();

            var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.Section));

            services
                .AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // Lấy token từ query string cho SignalR
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/hubs/notification"))
                            {
                                context.Token = accessToken;
                            }

                            // Nếu token trong cookie "jwt"
                            if (string.IsNullOrEmpty(context.Token) &&
                                context.Request.Cookies.ContainsKey("jwt"))
                            {
                                context.Token = context.Request.Cookies["jwt"];
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }
    }
}
