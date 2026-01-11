using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using WeatherApi.Application.Common.Interfaces;
using WeatherApi.Application.Weather.DTOs;

namespace WeatherApi.Infrastructure.Weather;

public class WeatherApiClient : IWeatherApiClient
{
    private readonly HttpClient _httpClient;
    private readonly WeatherApiSettings _settings;
    private readonly ILogger<WeatherApiClient> _logger;

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public WeatherApiClient(
        HttpClient httpClient,
        IOptions<WeatherApiSettings> settings,
        ILogger<WeatherApiClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<WeatherDto> GetWeatherAsync(string city, CancellationToken ct = default)
    {
        if (_settings.UseFakeData)
        {
            _logger.LogInformation("Using fake weather data for city: {City}", city);
            return GenerateFakeWeather(city);
        }

        try
        {
            _logger.LogInformation("Fetching real weather data for city: {City}", city);

            var url = $"weather?q={Uri.EscapeDataString(city)}&appid={_settings.ApiKey}&units=metric";
            var response = await _httpClient.GetAsync(url, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Weather API returned {StatusCode} for city: {City}",
                    response.StatusCode, city);
                return GenerateFakeWeather(city);
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            var weatherData = JsonSerializer.Deserialize<OpenWeatherMapResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (weatherData?.Main == null)
            {
                _logger.LogWarning("Invalid weather data received for city: {City}", city);
                return GenerateFakeWeather(city);
            }

            return new WeatherDto(
                city,
                DateTime.UtcNow,
                (int)weatherData.Main.Temp,
                (int)(weatherData.Main.Temp * 9 / 5 + 32),
                weatherData.Weather?.FirstOrDefault()?.Main ?? "Clear",
                weatherData.Weather?.FirstOrDefault()?.Description ?? "Clear sky",
                weatherData.Main.Humidity,
                weatherData.Wind?.Speed ?? 0
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch weather data for city: {City}", city);
            throw new InvalidOperationException($"Unable to fetch weather data for {city}", ex);
        }
    }

    private static WeatherDto GenerateFakeWeather(string city)
    {
        var random = new Random(city.GetHashCode() + DateTime.UtcNow.Day);
        var temperatureC = random.Next(-20, 55);

        return new WeatherDto(
            city,
            DateTime.UtcNow,
            temperatureC,
            32 + (int)(temperatureC / 0.5556),
            Summaries[random.Next(Summaries.Length)],
            "Mock weather data for testing purposes",
            random.Next(30, 90),
            Math.Round(random.NextDouble() * 20, 2)
        );
    }

    private class OpenWeatherMapResponse
    {
        public MainData? Main { get; set; }
        public WeatherData[]? Weather { get; set; }
        public WindData? Wind { get; set; }
    }

    private class MainData
    {
        public double Temp { get; set; }
        public int Humidity { get; set; }
    }

    private class WeatherData
    {
        public string Main { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    private class WindData
    {
        public double Speed { get; set; }
    }
}