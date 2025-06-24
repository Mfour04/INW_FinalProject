using Application;
using Application.Mapping;
using Application.Services;
using Infrastructure;
using Infrastructure.InwContext;
using Microsoft.Extensions.Options;
using Net.payOS;
using Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<MongoSetting>(
    builder.Configuration.GetSection("MongoDB"));

builder.Services.AddInfrastructure(builder.Configuration);
//builder.Services.AddApplication();

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Program.cs
builder.Services.Configure<PayOSConfig>(builder.Configuration.GetSection("PayOS"));
builder.Services.AddSingleton<PayOS>(sp =>
{
	var config = sp.GetRequiredService<IOptions<PayOSConfig>>().Value;
	return new PayOS(config.ClientId, config.ApiKey, config.ChecksumKey);
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddHostedService<TransactionCleanupService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseCors("AllowAll");
app.MapControllers();

app.Run();
