using Application;
using Application.Auth.Commands;
using Application.Mapping;
using Application.Services.Implements;
using Application.Services.Interfaces;
using Infrastructure;
using Infrastructure.InwContext;
using Infrastructure.SignalRHub;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Net.payOS;
using Shared;
using Shared.Contracts.Response.OpenAI;
using Shared.SystemHelpers.TokenGenerate;
using System.Security.Claims;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<MongoSetting>(
    builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<MongoDBHelper>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://inkwave-a5aqekhgdmhdducc.southeastasia-01.azurewebsites.net")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Program.cs
builder.Services.Configure<PayOSConfig>(builder.Configuration.GetSection("PayOS"));
builder.Services.AddSingleton<PayOS>(sp =>
{
    var config = sp.GetRequiredService<IOptions<PayOSConfig>>().Value;
    return new PayOS(config.ClientId, config.ApiKey, config.ChecksumKey);
});
var jwtSettings = builder.Configuration.GetSection(JwtSettings.Section).Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.Section));

builder.Services.AddAuthentication(options =>
{
    // Mặc định dùng JWT
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
// JWT Bearer cho API
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
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
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs/notification"))
            {
                context.Token = accessToken;
            }

            if (string.IsNullOrEmpty(context.Token) &&
                context.Request.Cookies.ContainsKey("jwt"))
            {
                context.Token = context.Request.Cookies["jwt"];
            }

            return Task.CompletedTask;
        }
    };
})
// Google login
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google";

    options.Events.OnCreatingTicket = async ctx =>
    {
        var email = ctx.Principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = ctx.Principal.FindFirst(ClaimTypes.Name)?.Value;
        var picture = ctx.Principal.FindFirst("urn:google:picture")?.Value;

        var mediator = ctx.HttpContext.RequestServices.GetRequiredService<IMediator>();

        var response = await mediator.Send(new LoginGoogleCommand
        {
            Email = email,
            Name = name,
            AvatarUrl = picture
        });
        var data = (dynamic)response.Data;
        ctx.Properties.RedirectUri = $"/auth-success?token={data.AccessToken}&refreshToken={data.RefreshToken}";
    };

});
builder.Services.Configure<OpenAIConfig>(builder.Configuration.GetSection("OpenAI"));
builder.Services.AddHttpClient<IOpenAIService, OpenAIService>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontendDev");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notification");
app.MapGet("/login", async (HttpContext http) =>
{
    await http.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = "/profile" // hoặc nơi bạn muốn cho sau khi xử lý OnCreatingTicket
    });
});

// Optional: route FE sẽ bắt token tại /auth-success
app.MapGet("/auth-success", async context =>
{
    var token = context.Request.Query["token"].ToString();
    await context.Response.WriteAsync($"Login success. Token: {token}");
});

app.Run();
