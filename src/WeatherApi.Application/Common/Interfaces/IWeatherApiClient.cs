using WeatherApi.Application.Weather.DTOs;

namespace WeatherApi.Application.Common.Interfaces;

public interface IWeatherApiClient
{
    Task<WeatherDto> GetWeatherAsync(string city, CancellationToken ct = default);
}