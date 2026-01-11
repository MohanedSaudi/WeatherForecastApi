using FluentAssertions;
using WeatherApi.Domain.Users;

namespace WeatherApi.UnitTests.Domain.Users;

public class PasswordTests
{
    [Fact]
    public void Create_WithValidPassword_ShouldSucceed()
    {
        // Act
        var result = Password.Create("Pass123!@#");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("")] // Empty
    [InlineData("Pass123")] // No special character
    [InlineData("pass123!")] // No uppercase
    [InlineData("PASS123!")] // No lowercase
    [InlineData("Password!")] // No digit
    [InlineData("Pass1!")] // Too short
    public void Create_WithInvalidPassword_ShouldFail(string password)
    {
        // Act
        var result = Password.Create(password);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.WeakPassword);
    }
}