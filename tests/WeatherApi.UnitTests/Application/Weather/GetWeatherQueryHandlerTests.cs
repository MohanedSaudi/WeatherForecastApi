using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherApi.Application.Common.Interfaces;
using WeatherApi.Application.Weather.DTOs;
using WeatherApi.Application.Weather.GetWeather;
using WeatherApi.Domain.Weather;
using Xunit;

namespace WeatherApi.UnitTests.Application.Weather;

public class GetWeatherQueryHandlerTests
{
    private readonly Mock<IWeatherApiClient> _weatherApiClientMock;
    private readonly Mock<ILogger<GetWeatherQueryHandler>> _loggerMock;
    private readonly GetWeatherQueryHandler _handler;

    public GetWeatherQueryHandlerTests()
    {
        _weatherApiClientMock = new Mock<IWeatherApiClient>();
        _loggerMock = new Mock<ILogger<GetWeatherQueryHandler>>();

        _handler = new GetWeatherQueryHandler(
            _weatherApiClientMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCity_ShouldReturnWeatherData()
    {
        // Arrange
        var query = new GetWeatherQuery("London");
        var expectedWeather = new WeatherDto(
            "London",
            DateTime.UtcNow,
            15,
            59,
            "Mild",
            "Partly cloudy",
            65,
            12.5);

        _weatherApiClientMock
            .Setup(x => x.GetWeatherAsync("London", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedWeather);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedWeather);
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetWeatherQuery("InvalidCity");

        _weatherApiClientMock
            .Setup(x => x.GetWeatherAsync("InvalidCity", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Weather service error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WeatherErrors.ServiceUnavailable);
    }

    [Theory]
    [InlineData("London")]
    [InlineData("Paris")]
    [InlineData("Tokyo")]
    [InlineData("NewYork")]
    public async Task Handle_DifferentCities_ShouldReturnCorrectCity(string city)
    {
        // Arrange
        var query = new GetWeatherQuery(city);
        var expectedWeather = new WeatherDto(city, DateTime.UtcNow, 20, 68, "Warm", "Sunny", 50, 10);

        _weatherApiClientMock
            .Setup(x => x.GetWeatherAsync(city, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedWeather);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.City.Should().Be(city);
    }

    [Fact]
    public async Task Handle_ShouldLogInformation()
    {
        // Arrange
        var query = new GetWeatherQuery("London");
        var expectedWeather = new WeatherDto("London", DateTime.UtcNow, 15, 59, "Mild", "Cloudy", 65, 12);

        _weatherApiClientMock
            .Setup(x => x.GetWeatherAsync("London", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedWeather);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fetching weather for city")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenServiceFails_ShouldLogError()
    {
        // Arrange
        var query = new GetWeatherQuery("London");

        _weatherApiClientMock
            .Setup(x => x.GetWeatherAsync("London", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch weather")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}