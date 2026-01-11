using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WeatherApi.Domain.Enums;
using WeatherApi.Domain.Users;

namespace WeatherApi.Infrastructure.Common.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            logger.LogInformation("Starting database initialization...");

            if (context.Database.IsRelational())
            {
                // Real Database: Run Migrations
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully.");
                }
                else
                {
                    logger.LogInformation("Database is up to date.");
                }
            }
            else
            {
                // Test/In-Memory Database: Just ensure the schema is created
                // (MigrateAsync crashes on In-Memory)
                logger.LogInformation("Non-relational provider detected. Using EnsureCreated().");
                await context.Database.EnsureCreatedAsync();
            }
            // ==============================================================

            // Seed Data
            if (!await context.Users.AnyAsync())
            {
                logger.LogInformation("Seeding initial data...");
                var passwordHash = Password.Create("Test123!@#").Value; 
                var adminPassHash = Password.Create("Admin123!@#").Value;
                var premiumPassHash = Password.Create("Premium123!@#").Value;

                var users = new List<User>
            {
                // 1. Standard User (For Login Tests)
                User.Create("Test User", Email.Create("test@weatherapi.com").Value, passwordHash).Value,                
                // 2. Admin User (For Register Conflict & Admin Role Tests)
                User.Create("Admin User", Email.Create("admin@weatherapi.com").Value, adminPassHash, UserRole.Admin)
                    .Value, // Assuming you have a method/setter for Role

                // 3. Premium User (For Premium Role Tests)
                User.Create("Premium User", Email.Create("premium@weatherapi.com").Value, premiumPassHash, UserRole.Premium)
                    .Value
            };

                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
            }
            logger.LogInformation("Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }
}