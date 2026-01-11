using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WeatherApi.Application.Users.DTOs;
using WeatherApi.Application.Users.Login;
using WeatherApi.Application.Users.Register;
using WeatherApi.IntegrationTests.Common;
using Xunit;

namespace WeatherApi.IntegrationTests.Controllers;

public class AuthControllerTests : IntegrationTestBase
{
    public AuthControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_WithValidData_ShouldReturn200AndToken()
    {
        // Arrange
        var command = new RegisterCommand(
            "newuser",
            $"newuser{Guid.NewGuid()}@example.com",
            "NewUser123!@#");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content.Should().NotBeNull();
        content!.Username.Should().Be("newuser");
        content.AccessToken.Should().NotBeNullOrEmpty();
        content.Role.Should().Be("User");
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldReturn409()
    {
        // Arrange
        var command = new RegisterCommand(
            "duplicate",
            "admin@weatherapi.com", // Pre-seeded user
            "Test123!@#");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturn400()
    {
        // Arrange
        var command = new RegisterCommand(
            "weakpass",
            "weak@example.com",
            "123"); // Weak password

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturn400()
    {
        // Arrange
        var command = new RegisterCommand(
            "testuser",
            "not-an-email", // Invalid email
            "Test123!@#");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200AndToken()
    {
        // Arrange
        var command = new LoginCommand("test@weatherapi.com", "Test123!@#");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content.Should().NotBeNull();
        content!.Email.Should().Be("test@weatherapi.com");
        content.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturn401()
    {
        // Arrange
        var command = new LoginCommand("test@weatherapi.com", "WrongPassword");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ShouldReturn401()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@example.com", "Test123!@#");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_AsAdmin_ShouldReturnAdminRole()
    {
        // Arrange
        var command = new LoginCommand("admin@weatherapi.com", "Admin123!@#");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", command);

        // Assert
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content!.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_AsPremium_ShouldReturnPremiumRole()
    {
        // Arrange
        var command = new LoginCommand("premium@weatherapi.com", "Premium123!@#");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", command);

        // Assert
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content!.Role.Should().Be("Premium");
    }

    [Fact]
    public async Task RefreshToken_WithoutCookie_ShouldReturn401()
    {
        // Act
        var response = await Client.PostAsync("/api/auth/refresh-token", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RevokeToken_WithoutAuthentication_ShouldReturn401()
    {
        // Act
        var response = await Client.PostAsync("/api/auth/revoke-token", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    //[Fact]
    //public async Task CompleteAuthFlow_RegisterLoginLogout_ShouldWork()
    //{
    //    // Step 1: Register
    //    var registerCommand = new RegisterCommand(
    //        "flowuser",
    //        $"flow{Guid.NewGuid()}@example.com",
    //        "Flow123!@#");

    //    var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerCommand);
    //    registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

    //    var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
    //    var token = authResponse!.AccessToken;

    //    // Step 2: Use token to access protected resource
    //    Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    //    var weatherResponse = await Client.GetAsync("/api/weather?city=London");
    //    weatherResponse.StatusCode.Should().Be(HttpStatusCode.OK);

    //    // Step 3: Revoke token (logout)
    //    var revokeResponse = await Client.PostAsync("/api/auth/revoke-token", null);
    //    revokeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    //}

    [Fact]
    public async Task CompleteAuthFlow_RegisterLoginLogout_ShouldWork()
    {
        // Step 1: Register
        var registerCommand = new RegisterCommand("flowuser", $"flow{Guid.NewGuid()}@example.com", "Flow123!@#");
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerCommand);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        var token = authResponse!.AccessToken;

        // === FIX: Capture the Cookie ===
        // The Controller puts the Refresh Token in a HttpOnly Cookie, not the body.
        if (!registerResponse.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            throw new Exception("Set-Cookie header was not found in the response.");
        }
        // Extract the "refreshToken=..." part
        var refreshTokenCookie = cookies.FirstOrDefault(c => c.StartsWith("refreshToken"));
        // ==============================

        // Step 2: Access Protected Resource
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var weatherResponse = await Client.GetAsync("/api/weather?city=London");
        weatherResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 3: Revoke (Attach the Cookie)
        // Create a new request message to manually add the Cookie header
        var revokeRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/revoke-token");
        revokeRequest.Headers.Add("Cookie", refreshTokenCookie); // <--- Send Cookie back

        var revokeResponse = await Client.SendAsync(revokeRequest);
        revokeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}