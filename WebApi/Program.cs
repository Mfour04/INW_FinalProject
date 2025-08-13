using Application;
using Application.Mapping;
using Application.Services.Implements;
using Application.Services.Interfaces;
using Infrastructure;
using Infrastructure.InwContext;
using Infrastructure.SignalRHub;
using Microsoft.Extensions.Options;
using Net.payOS;
using Shared;
using Shared.Contracts.Response.OpenAI;


var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<MongoSetting>(
    builder.Configuration.GetSection("MongoDB"));

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
        policy.WithOrigins("http://localhost:5173")
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
app.Run();
