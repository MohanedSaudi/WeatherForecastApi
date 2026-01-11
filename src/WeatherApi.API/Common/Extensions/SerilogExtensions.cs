using Serilog;

namespace WeatherApi.API.Common.Extensions;

public static class SerilogExtensions
{
    public static IHostBuilder AddCustomSerilog(
        this IHostBuilder host,
        IConfiguration configuration)
    {
        host.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("Application", "WeatherApi")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/weather-api-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.Seq(
                    serverUrl: configuration["Seq:ServerUrl"] ?? "http://localhost:5341",
                    apiKey: configuration["Seq:ApiKey"]);
        });

        return host;
    }
}