
namespace WeatherApi.Domain.Common.Result;

public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Failure
}