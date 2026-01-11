using FluentAssertions;
using WeatherApi.Domain.RefreshTokens;
using Xunit;

namespace WeatherApi.UnitTests.Domain.RefreshTokens;

public class RefreshTokenTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "test_token_123";
        var ipAddress = "192.168.1.1";

        // Act
        var result = RefreshToken.Create(userId, token, 7, ipAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        result.Value.Token.Should().Be(token);
        result.Value.CreatedByIp.Should().Be(ipAddress);
        result.Value.IsRevoked.Should().BeFalse();
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyToken_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ipAddress = "192.168.1.1";

        // Act
        var result = RefreshToken.Create(userId, "", 7, ipAddress);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Revoke_WhenActive_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId, "token", 7, "192.168.1.1").Value;
        var ipAddress = "192.168.1.2";

        // Act
        var result = token.Revoke(ipAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        token.IsRevoked.Should().BeTrue();
        token.RevokedByIp.Should().Be(ipAddress);
        token.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId, "token", 7, "192.168.1.1").Value;
        token.Revoke("192.168.1.2");

        // Act
        var result = token.Revoke("192.168.1.3");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RefreshTokenErrors.Revoked);
    }

    [Fact]
    public void Validate_WhenActive_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId, "token", 7, "192.168.1.1").Value;

        // Act
        var result = token.Validate();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenRevoked_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId, "token", 7, "192.168.1.1").Value;
        token.Revoke("192.168.1.2");

        // Act
        var result = token.Validate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RefreshTokenErrors.Revoked);
    }

    [Fact]
    public void IsExpired_WhenNotExpired_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId, "token", 7, "192.168.1.1").Value;

        // Act & Assert
        token.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenActiveAndNotExpired_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId, "token", 7, "192.168.1.1").Value;

        // Act & Assert
        token.IsActive.Should().BeTrue();
    }
}