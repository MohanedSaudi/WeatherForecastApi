namespace WeatherApi.Domain.Weather;

public interface IWeatherCacheRepository
{
    Task<WeatherCache?> GetByCityAsync(string city, CancellationToken ct = default);
    Task AddAsync(WeatherCache cache, CancellationToken ct = default);
    Task DeleteExpiredAsync(CancellationToken ct = default);
}