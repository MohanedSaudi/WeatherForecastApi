using FluentAssertions;
using WeatherApi.Domain.Users;
using WeatherApi.Domain.Enums;
using Xunit;

namespace WeatherApi.UnitTests.Domain.Users;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");

        // Act
        var result = User.Create("testuser", emailResult.Value, passwordResult.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Username.Should().Be("testuser");
        result.Value.Email.Should().Be("test@example.com");
        result.Value.IsActive.Should().BeTrue();
        result.Value.Role.Should().Be(UserRole.User);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("a")]
    [InlineData("")]
    public void Create_WithShortUsername_ShouldFail(string username)
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");

        // Act
        var result = User.Create(username, emailResult.Value, passwordResult.Value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidUsername);
    }

    [Fact]
    public void Create_WithAdminRole_ShouldSucceed()
    {
        // Arrange
        var emailResult = Email.Create("admin@example.com");
        var passwordResult = Password.Create("Admin123!@#");

        // Act
        var result = User.Create("admin", emailResult.Value, passwordResult.Value, UserRole.Admin);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldSucceed()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;

        // Act
        var result = user.VerifyPassword("Test123!@#");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithWrongPassword_ShouldFail()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;

        // Act
        var result = user.VerifyPassword("WrongPassword");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidCredentials);
    }

    [Fact]
    public void VerifyPassword_WhenUserInactive_ShouldFail()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;
        user.Deactivate();

        // Act
        var result = user.VerifyPassword("Test123!@#");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Inactive);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;
        user.Deactivate();

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ChangePassword_WithValidPassword_ShouldSucceed()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;
        var newPasswordResult = Password.Create("NewPass123!@#");

        // Act
        var result = user.ChangePassword(newPasswordResult.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.VerifyPassword("NewPass123!@#").IsSuccess.Should().BeTrue();
        user.VerifyPassword("Test123!@#").IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void UpdateProfile_WithValidUsername_ShouldSucceed()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;

        // Act
        var result = user.UpdateProfile("newusername");

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Username.Should().Be("newusername");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("")]
    [InlineData("  ")]
    public void UpdateProfile_WithInvalidUsername_ShouldFail(string username)
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;

        // Act
        var result = user.UpdateProfile(username);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidUsername);
    }
}