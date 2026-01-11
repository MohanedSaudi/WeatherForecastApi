using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WeatherApi.Application.Users.DTOs;
using WeatherApi.Application.Users.Login;
using WeatherApi.Application.Weather.DTOs;
using WeatherApi.IntegrationTests.Common;
using Xunit;

namespace WeatherApi.IntegrationTests.Controllers;

public class WeatherControllerTests : IntegrationTestBase
{
    public WeatherControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    private async Task<string> GetAuthTokenAsync(string email, string password)
    {
        var loginCommand = new LoginCommand(email, password);
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginCommand);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.AccessToken;
    }

    [Fact]
    public async Task GetWeather_WithoutAuthentication_ShouldReturn401()
    {
        // Act
        var response = await Client.GetAsync("/api/weather?city=London");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetWeather_WithValidToken_ShouldReturn200()
    {
        // Arrange
        var token = await GetAuthTokenAsync("test@weatherapi.com", "Test123!@#");
        Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await Client.GetAsync("/api/weather?city=London");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var weather = await response.Content.ReadFromJsonAsync<WeatherDto>();
        weather.Should().NotBeNull();
        weather!.City.Should().Be("London");
        weather.TemperatureC.Should().BeInRange(-20, 55);
    }

    [Fact]
    public async Task GetWeather_WithInvalidCity_ShouldReturn400()
    {
        // Arrange
        var token = await GetAuthTokenAsync("test@weatherapi.com", "Test123!@#");
        Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await Client.GetAsync("/api/weather?city=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("London")]
    [InlineData("Paris")]
    [InlineData("Tokyo")]
    [InlineData("NewYork")]
    public async Task GetWeather_DifferentCities_ShouldReturnWeatherData(string city)
    {
        // Arrange
        var token = await GetAuthTokenAsync("test@weatherapi.com", "Test123!@#");
        Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await Client.GetAsync($"/api/weather?city={city}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var weather = await response.Content.ReadFromJsonAsync<WeatherDto>();
        weather!.City.Should().Be(city);
    }

    [Fact]
    public async Task BulkWeather_AsPremiumUser_ShouldReturn200()
    {
        // Arrange
        var token = await GetAuthTokenAsync("premium@weatherapi.com", "Premium123!@#");
        Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var cities = new[] { "London", "Paris", "Tokyo" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/weather/bulk", cities);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<WeatherDto[]>();
        results.Should().NotBeNull();
        results!.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task BulkWeather_AsRegularUser_ShouldReturn403()
    {
        // Arrange
        var token = await GetAuthTokenAsync("test@weatherapi.com", "Test123!@#");
        Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var cities = new[] { "London", "Paris" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/weather/bulk", cities);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetWeather_MultipleTimes_ShouldUseCaching()
    {
        // Arrange
        var token = await GetAuthTokenAsync("test@weatherapi.com", "Test123!@#");
        Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act - First request
        var response1 = await Client.GetAsync("/api/weather?city=London");
        var weather1 = await response1.Content.ReadFromJsonAsync<WeatherDto>();

        // Act - Second request (should be cached)
        var response2 = await Client.GetAsync("/api/weather?city=London");
        var weather2 = await response2.Content.ReadFromJsonAsync<WeatherDto>();

        // Assert
        weather1!.TemperatureC.Should().Be(weather2!.TemperatureC);
        weather1.Summary.Should().Be(weather2.Summary);
    }
}