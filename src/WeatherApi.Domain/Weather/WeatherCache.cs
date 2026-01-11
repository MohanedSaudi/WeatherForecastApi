using WeatherApi.Domain.Common.BaseEntity;
using WeatherApi.Domain.Common.Result;

namespace WeatherApi.Domain.Weather;

public sealed class WeatherCache : ImmutableEntity
{
    private WeatherCache() { } // EF Core

    private WeatherCache(
        string city,
        int temperatureC,
        string summary,
        string description,
        int humidity,
        double windSpeed,
        int cacheMinutes)
    {
        Id = Guid.NewGuid();
        City = city;
        TemperatureC = temperatureC;
        TemperatureF = 32 + (int)(temperatureC / 0.5556);
        Summary = summary;
        Description = description;
        Humidity = humidity;
        WindSpeed = windSpeed;
        ExpiresAt = DateTime.UtcNow.AddMinutes(cacheMinutes);
    }

    public string City { get; private set; } = string.Empty;
    public int TemperatureC { get; private set; }
    public int TemperatureF { get; private set; }
    public string Summary { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int Humidity { get; private set; }
    public double WindSpeed { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public static Result<WeatherCache> Create(
        string city,
        int temperatureC,
        string summary,
        string description,
        int humidity,
        double windSpeed,
        int cacheMinutes = 10)
    {
        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<WeatherCache>(WeatherErrors.InvalidCity);

        var weather = new WeatherCache(
            city.Trim(),
            temperatureC,
            summary,
            description,
            humidity,
            windSpeed,
            cacheMinutes);

        return Result.Success(weather);
    }
}