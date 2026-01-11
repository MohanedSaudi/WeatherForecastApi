using MediatR;
using WeatherApi.Application.Common.Behaviors;
using WeatherApi.Application.Weather.DTOs;
using WeatherApi.Domain.Common.Result;

namespace WeatherApi.Application.Weather.GetWeather;

public record GetWeatherQuery(string City)
    : IRequest<Result<WeatherDto>>, ICacheableQuery
{
    public string CacheKey => $"Weather_{City}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}
