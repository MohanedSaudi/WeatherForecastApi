using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WeatherApi.Domain.Users;
using WeatherApi.Infrastructure.Common.Persistence;
using WeatherApi.Infrastructure.Users;
using Xunit;

namespace WeatherApi.IntegrationTests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var services = new ServiceCollection();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
        });

        services.AddScoped<Application.Common.Interfaces.ICurrentUserService, TestCurrentUserService>();

        var serviceProvider = services.BuildServiceProvider();
        _context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        _repository = new UserRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddUserToDatabase()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;

        // Act
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        savedUser.Should().NotBeNull();
        savedUser!.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("TEST@EXAMPLE.COM");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExistsAsync_WhenUserExists_ShouldReturnTrue()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync("test@example.com");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenUserDoesNotExist_ShouldReturnFalse()
    {
        // Act
        var exists = await _repository.ExistsAsync("nonexistent@example.com");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        user.UpdateProfile("newusername");
        await _repository.UpdateAsync(user);
        await _context.SaveChangesAsync();

        // Assert
        var updatedUser = await _repository.GetByIdAsync(user.Id);
        updatedUser!.Username.Should().Be("newusername");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private class TestCurrentUserService : Application.Common.Interfaces.ICurrentUserService
    {
        public string? UserId => null;
        public string? Username => null;
        public string? Email => null;
        public bool IsAuthenticated => false;
        public bool IsInRole(string role) => false;
        public string GetUserIdentifier() => "TestUser";
    }
}