using Serilog;
using WeatherApi.API.Common.Extensions;
using WeatherApi.Application;
using WeatherApi.Infrastructure;
using WeatherApi.Infrastructure.Common.Persistence;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Host (Logging)
builder.Host.AddCustomSerilog(builder.Configuration);

// 2. Register Services (Dependency Injection)
builder.Services
    .AddApplicationServices()          // Core Application Layer
    .AddInfrastructureServices(builder.Configuration) // DB, Repositories
    .AddWebServices(builder.Configuration); // API specific (Swagger, Auth, RateLimiting, etc.)

var app = builder.Build();

// 3. Initialize Database
await app.InitializeDatabaseAsync();

// 4. Configure Request Pipeline (Middleware)
app.UseWebPipeline();

try
{
    Log.Information("Starting Web Host on Environment: {Environment}", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program public for Integration Tests
public partial class Program { }