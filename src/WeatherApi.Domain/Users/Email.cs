using WeatherApi.Domain.Common.Result;

namespace WeatherApi.Domain.Users;

public sealed record Email
{
    private Email(string value) => Value = value;

    public string Value { get; }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Email>(UserErrors.InvalidEmail);

        email = email.Trim().ToLowerInvariant();

        if (!IsValidEmail(email))
            return Result.Failure<Email>(UserErrors.InvalidEmail);

        return Result.Success(new Email(email));
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var emailRegex = new System.Text.RegularExpressions.Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return emailRegex.IsMatch(email);
    }

    public static implicit operator string(Email email) => email.Value;
}