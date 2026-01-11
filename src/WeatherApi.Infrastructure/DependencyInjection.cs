using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherApi.Application.Common.Interfaces;
using WeatherApi.Domain.Common.Interfaces;
using WeatherApi.Domain.Users;
using WeatherApi.Domain.RefreshTokens;
using WeatherApi.Domain.Weather;
using WeatherApi.Infrastructure.Common.Persistence;
using WeatherApi.Infrastructure.Users;
using WeatherApi.Infrastructure.RefreshTokens;
using WeatherApi.Infrastructure.Weather;
using WeatherApi.Infrastructure.Authentication;
using WeatherApi.Infrastructure.Caching;
using WeatherApi.Infrastructure.Resilience;

namespace WeatherApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database - SQL Server with Retry Logic
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            options.UseSqlServer(connectionString, sqlOptions =>
            {
                // Enable retry on failure
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);

                // Command timeout
                sqlOptions.CommandTimeout(30);

                // Migrations assembly
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            });

            // Logging and diagnostics
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(configuration.GetValue<bool>("DetailedErrors", false));
        });

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IWeatherCacheRepository, WeatherCacheRepository>();

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<ICacheService, CacheService>();

        // Weather API Client with Resilience
        services.Configure<WeatherApiSettings>(
            configuration.GetSection(WeatherApiSettings.SectionName));

        var weatherSettings = configuration
            .GetSection(WeatherApiSettings.SectionName)
            .Get<WeatherApiSettings>() ?? new WeatherApiSettings();

        services.AddHttpClient<IWeatherApiClient, WeatherApiClient>(client =>
        {
            client.BaseAddress = new Uri(weatherSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(weatherSettings.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddPolicyHandler((services, request) =>
            ResiliencePolicies.GetCombinedPolicy(
                services.GetService<ILogger<WeatherApiClient>>()));

        // Memory Cache
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1024;
            options.CompactionPercentage = 0.25;
        });

        services.AddHttpContextAccessor();

        return services;
    }
}