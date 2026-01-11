using MediatR;
using Microsoft.Extensions.Logging;
using WeatherApi.Application.Common.Interfaces;
using WeatherApi.Application.Weather.DTOs;
using WeatherApi.Domain.Common.Result;
using WeatherApi.Domain.Weather;

namespace WeatherApi.Application.Weather.GetWeather;


public class GetWeatherQueryHandler : IRequestHandler<GetWeatherQuery, Result<WeatherDto>>
{
    private readonly IWeatherApiClient _weatherApiClient;
    private readonly ILogger<GetWeatherQueryHandler> _logger;

    public GetWeatherQueryHandler(
        IWeatherApiClient weatherApiClient,
        ILogger<GetWeatherQueryHandler> logger)
    {
        _weatherApiClient = weatherApiClient;
        _logger = logger;
    }

    public async Task<Result<WeatherDto>> Handle(
        GetWeatherQuery request,
        CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Fetching weather for city: {City}", request.City);

            var weather = await _weatherApiClient.GetWeatherAsync(request.City, ct);

            return Result.Success(weather);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch weather for city: {City}", request.City);
            return Result.Failure<WeatherDto>(WeatherErrors.ServiceUnavailable);
        }
    }
}