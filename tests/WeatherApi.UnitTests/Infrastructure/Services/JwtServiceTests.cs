using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WeatherApi.Application.Common.Interfaces;
using WeatherApi.Domain.Enums;
using WeatherApi.Domain.Users;
using WeatherApi.Infrastructure.Authentication;
using Xunit;

namespace WeatherApi.UnitTests.Infrastructure.Services;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Jwt:Key"] = "YourSuperSecretKeyThatIsAtLeast32CharactersLongForTesting!",
                ["Jwt:Issuer"] = "WeatherApi",
                ["Jwt:Audience"] = "WeatherApiUsers"
            }!)
            .Build();

        _jwtService = new JwtService(configuration);
    }

    [Fact]
    public void GenerateTokens_ShouldReturnAccessTokenAndRefreshToken()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;

        // Act
        var (accessToken, refreshToken) = _jwtService.GenerateTokens(user);

        // Assert
        accessToken.Should().NotBeNullOrEmpty();
        refreshToken.Should().NotBeNullOrEmpty();
        accessToken.Should().NotBe(refreshToken);
    }

    //[Fact]
    //public void GenerateTokens_AccessToken_ShouldContainUserClaims()
    //{
    //    // Arrange
    //    var emailResult = Email.Create("test@example.com");
    //    var passwordResult = Password.Create("Test123!@#");
    //    var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;

    //    // Act
    //    var (accessToken, _) = _jwtService.GenerateTokens(user);

    //    // Parse token
    //    var handler = new JwtSecurityTokenHandler();
    //    var token = handler.ReadJwtToken(accessToken);

    //    // Assert
    //    token.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "test@example.com");
    //    token.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "testuser");
    //    token.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == UserRole.User.ToString());
    //    token.Claims.Should().Contain(c => c.Type == "userId");
    //}

    [Fact]
    public void GenerateTokens_AccessToken_ShouldContainUserClaims()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;

        // Act
        var (accessToken, _) = _jwtService.GenerateTokens(user);

        // Parse token
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(accessToken);

        // Assert
        // 1. Fix: Use JwtRegisteredClaimNames.Email (value is "email") instead of ClaimTypes.Email
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "test@example.com");

        // 2. Fix: Use JwtRegisteredClaimNames.Name (value is "name") instead of ClaimTypes.Name
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Name && c.Value == "testuser");

        // 3. Keep: ClaimTypes.Role is correct because the error log shows the full URI for the role
        token.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == UserRole.User.ToString());

        // 4. Custom Claim
        token.Claims.Should().Contain(c => c.Type == "userId");
    }

    [Fact]
    public void GenerateTokens_RefreshToken_ShouldBe64Bytes()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;

        // Act
        var (_, refreshToken) = _jwtService.GenerateTokens(user);

        // Assert
        var bytes = Convert.FromBase64String(refreshToken);
        bytes.Length.Should().Be(64);
    }

    [Fact]
    public void GenerateTokens_MultipleCallsForSameUser_ShouldGenerateDifferentRefreshTokens()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;

        // Act
        var (_, refreshToken1) = _jwtService.GenerateTokens(user);
        var (_, refreshToken2) = _jwtService.GenerateTokens(user);

        // Assert
        refreshToken1.Should().NotBe(refreshToken2);
    }

    [Fact]
    public void GenerateTokens_ForAdminUser_ShouldIncludeAdminRole()
    {
        // Arrange
        var emailResult = Email.Create("admin@example.com");
        var passwordResult = Password.Create("Admin123!@#");
        var user = User.Create("admin", emailResult.Value, passwordResult.Value, UserRole.Admin).Value;

        // Act
        var (accessToken, _) = _jwtService.GenerateTokens(user);

        // Parse token
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(accessToken);

        // Assert
        token.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == UserRole.Admin.ToString());
    }

    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnPrincipal()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;
        var (accessToken, _) = _jwtService.GenerateTokens(user);

        // Act
        var principal = _jwtService.ValidateToken(accessToken);

        // Assert
        principal.Should().NotBeNull();
        principal!.Identity!.IsAuthenticated.Should().BeTrue();
        principal.FindFirst(ClaimTypes.Email)!.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _jwtService.ValidateToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithMalformedToken_ShouldReturnNull()
    {
        // Arrange
        var malformedToken = "not-a-jwt-token";

        // Act
        var principal = _jwtService.ValidateToken(malformedToken);

        // Assert
        principal.Should().BeNull();
    }
}