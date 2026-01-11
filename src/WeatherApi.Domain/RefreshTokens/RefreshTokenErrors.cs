using WeatherApi.Domain.Common.Result;

namespace WeatherApi.Domain.RefreshTokens;

public static class RefreshTokenErrors
{
    public static Error Invalid => Error.Unauthorized(
        "RefreshToken.Invalid",
        "The refresh token is invalid");

    public static Error Expired => Error.Unauthorized(
        "RefreshToken.Expired",
        "The refresh token has expired");

    public static Error Revoked => Error.Unauthorized(
        "RefreshToken.Revoked",
        "The refresh token has been revoked");

    public static Error NotFound => Error.NotFound(
        "RefreshToken.NotFound",
        "Refresh token was not found");
}