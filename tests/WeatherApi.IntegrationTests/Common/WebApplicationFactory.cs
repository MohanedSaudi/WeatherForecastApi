using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WeatherApi.Infrastructure.Common.Persistence;

namespace WeatherApi.IntegrationTests.Common;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 1. Remove existing DbContext registration (SQL Server)
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // 2. Add InMemory database for testing
            // CRITICAL CHANGE: Use a STATIC string for the database name. 
            // If you use Guid.NewGuid(), every time the context is resolved, 
            // it might point to a new, empty database.
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTestDb");
            });

            // 3. Build service provider to run the initialization
            var sp = services.BuildServiceProvider();

            // 4. Create scope and seed data
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

            // Ensure database is created
            db.Database.EnsureCreated();

            // === CRITICAL FIX: Run the Seeder! ===
            try
            {
                // We use .Wait() here because ConfigureServices cannot be async
                DbInitializer.InitializeAsync(scopedServices).Wait();
            }
            catch (Exception ex)
            {
                // Log the error to the Test Output so you can see if seeding crashes
                Console.WriteLine($"An error occurred seeding the database: {ex.Message}");
            }
        });

        builder.UseEnvironment("Testing");
    }
}


//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using WeatherApi.Infrastructure.Common.Persistence;

//namespace WeatherApi.IntegrationTests.Common;

//public class CustomWebApplicationFactory : WebApplicationFactory<Program>
//{
//    protected override void ConfigureWebHost(IWebHostBuilder builder)
//    {
//        builder.ConfigureServices(services =>
//        {
//            // Remove existing DbContext registration
//            var descriptor = services.SingleOrDefault(
//                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

//            if (descriptor != null)
//            {
//                services.Remove(descriptor);
//            }

//            // Add InMemory database for testing
//            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
//            {
//                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
//            });

//            // Build service provider
//            var sp = services.BuildServiceProvider();

//            // Create scope and seed data
//            using var scope = sp.CreateScope();
//            var scopedServices = scope.ServiceProvider;
//            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

//            // Ensure database is created
//            db.Database.EnsureCreated();
//        });

//        builder.UseEnvironment("Testing");
//    }
//}