using WeatherApi.Domain.Common.Result;

namespace WeatherApi.Domain.Weather;

public static class WeatherErrors
{
    public static Error InvalidCity => Error.Validation(
        "Weather.InvalidCity",
        "City name is invalid or empty");

    public static Error ServiceUnavailable => Error.Failure(
        "Weather.ServiceUnavailable",
        "Weather service is temporarily unavailable");

    public static Error NotFound => Error.NotFound(
        "Weather.NotFound",
        "Weather data not found for the specified city");
}