using Microsoft.EntityFrameworkCore;
using Polly;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using WeatherApi.Application.Common.Interfaces;
using WeatherApi.Domain.Common.BaseEntity;
using WeatherApi.Domain.Enums;
using WeatherApi.Domain.RefreshTokens;
using WeatherApi.Domain.Users;
using WeatherApi.Domain.Weather;

namespace WeatherApi.Infrastructure.Common.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<WeatherCache> WeatherCaches => Set<WeatherCache>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // SEED DATA - Three default users
        SeedData(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Admin User
        var adminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        modelBuilder.Entity<User>().HasData(new
        {
            Id = adminUserId,
            Username = "admin",
            Email = "admin@weatherapi.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!@#"),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedBy = "System",
            UpdatedAt = (DateTime?)null,
            UpdatedBy = (string?)null
        });

        // Seed Premium User
        var premiumUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        modelBuilder.Entity<User>().HasData(new
        {
            Id = premiumUserId,
            Username = "premiumuser",
            Email = "premium@weatherapi.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Premium123!@#"),
            Role = UserRole.Premium,
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedBy = "System",
            UpdatedAt = (DateTime?)null,
            UpdatedBy = (string?)null
        });

        // Seed Regular User
        var regularUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        modelBuilder.Entity<User>().HasData(new
        {
            Id = regularUserId,
            Username = "testuser",
            Email = "test@weatherapi.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!@#"),
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedBy = "System",
            UpdatedAt = (DateTime?)null,
            UpdatedBy = (string?)null
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>();
        var userIdentifier = _currentUserService.GetUserIdentifier();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = userIdentifier;

                if (entry.Entity is MutableEntity mutableEntity)
                {
                    mutableEntity.UpdatedAt = null;
                    mutableEntity.UpdatedBy = null;
                }
            }
            else if (entry.State == EntityState.Modified && entry.Entity is MutableEntity mutable)
            {
                mutable.UpdatedAt = now;
                mutable.UpdatedBy = userIdentifier;
            }
        }

        return await base.SaveChangesAsync(ct);
    }
}