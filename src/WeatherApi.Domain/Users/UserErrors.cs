using WeatherApi.Domain.Common.Result;

namespace WeatherApi.Domain.Users;

public static class UserErrors
{
    public static Error EmailAlreadyExists => Error.Conflict(
        "User.EmailAlreadyExists",
        "User with the specified email already exists");

    public static Error InvalidCredentials => Error.Unauthorized(
        "User.InvalidCredentials",
        "The provided credentials are invalid");

    public static Error NotFound => Error.NotFound(
        "User.NotFound",
        "User was not found");

    public static Error Inactive => Error.Unauthorized(
        "User.Inactive",
        "User account is inactive");

    public static Error InvalidEmail => Error.Validation(
        "User.InvalidEmail",
        "The email format is invalid");

    public static Error WeakPassword => Error.Validation(
        "User.WeakPassword",
        "Password must be at least 8 characters and contain uppercase, lowercase, number, and special character");

    public static Error InvalidUsername => Error.Validation(
        "User.InvalidUsername",
        "Username must be at least 3 characters");
}