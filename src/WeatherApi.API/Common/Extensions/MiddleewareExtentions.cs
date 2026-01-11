using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using WeatherApi.API.Common.Middleware;
using WeatherApi.Infrastructure.Common.Persistence;

namespace WeatherApi.API.Common.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication UseWebPipeline(this WebApplication app)
    {
        // 1. Development Tools
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Weather API V1");
                c.DisplayRequestDuration();
            });
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
        }

        // 2. Core Infrastructure Middleware
        app.UseSerilogRequestLogging();    // Structured Logging
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseResponseCompression();
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        // 3. Security & Access Control
        app.UseCors("AllowSpecificOrigin");
        app.UseRateLimiter();
        app.UseAntiforgery();
        app.UseAuthentication();
        app.UseAuthorization();

        // 4. Endpoints
        app.MapControllers();
        app.MapHealthChecks();
        app.MapFallbackEndpoint();

        return app;
    }

    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        try
        {
            await DbInitializer.InitializeAsync(scope.ServiceProvider);
            Log.Information("Database Initialized");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database Initialization Failed");
            if (!app.Environment.IsDevelopment()) throw;
        }
    }

    private static void MapHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        app.MapHealthChecksUI(options => options.UIPath = "/health-dashboard");
    }

    private static void MapFallbackEndpoint(this WebApplication app)
    {
        app.MapGet("/error", () => Results.Problem("An error occurred.")).ExcludeFromDescription();
    }
}