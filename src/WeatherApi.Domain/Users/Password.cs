using WeatherApi.Domain.Common.Result;

namespace WeatherApi.Domain.Users;

public sealed record Password
{
    private Password(string value) => Value = value;

    public string Value { get; }

    public static Result<Password> Create(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Result.Failure<Password>(UserErrors.WeakPassword);

        if (password.Length < 8)
            return Result.Failure<Password>(UserErrors.WeakPassword);

        if (!password.Any(char.IsUpper))
            return Result.Failure<Password>(UserErrors.WeakPassword);

        if (!password.Any(char.IsLower))
            return Result.Failure<Password>(UserErrors.WeakPassword);

        if (!password.Any(char.IsDigit))
            return Result.Failure<Password>(UserErrors.WeakPassword);

        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            return Result.Failure<Password>(UserErrors.WeakPassword);

        return Result.Success(new Password(password));
    }

    public static implicit operator string(Password password) => password.Value;
}