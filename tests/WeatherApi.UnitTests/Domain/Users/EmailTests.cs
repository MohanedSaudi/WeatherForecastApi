using FluentAssertions;
using WeatherApi.Domain.Users;
using Xunit;

namespace WeatherApi.UnitTests.Domain.Users;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("user+tag@example.com")]
    [InlineData("user_123@test-domain.com")]
    [InlineData("TEST@EXAMPLE.COM")]
    public void Create_WithValidEmail_ShouldSucceed(string email)
    {
        // Act
        var result = Email.Create(email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(email.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("notanemail")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user @example.com")]
    [InlineData("user@.com")]
    [InlineData("user@domain")]
    public void Create_WithInvalidEmail_ShouldFail(string email)
    {
        // Act
        var result = Email.Create(email);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidEmail);
    }

    [Fact]
    public void Create_WithNullEmail_ShouldFail()
    {
        // Act
        var result = Email.Create(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidEmail);
    }

    [Fact]
    public void Create_ShouldNormalizeToLowerCase()
    {
        // Arrange
        var email = "TEST@EXAMPLE.COM";

        // Act
        var result = Email.Create(email);

        // Assert
        result.Value.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Arrange
        var email = "  test@example.com  ";

        // Act
        var result = Email.Create(email);

        // Assert
        result.Value.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        string emailString = emailResult.Value;

        // Assert
        emailString.Should().Be("test@example.com");
    }
}