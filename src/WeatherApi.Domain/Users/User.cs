using WeatherApi.Domain.Common.BaseEntity;
using WeatherApi.Domain.Common.Result;
using WeatherApi.Domain.Enums;

namespace WeatherApi.Domain.Users;

public sealed class User : MutableEntity
{
    private User() { } // EF Core

    private User(
        Guid id,
        string username,
        Email email,
        string passwordHash,
        UserRole role)
    {
        Id = id;
        Username = username;
        Email = email.Value;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
    }

    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }

    public static Result<User> Create(
        string username,
        Email email,
        Password password,
        UserRole role = UserRole.User)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            return Result.Failure<User>(UserErrors.InvalidUsername);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password.Value);

        var user = new User(
            Guid.NewGuid(),
            username.Trim(),
            email,
            passwordHash,
            role);

        return Result.Success(user);
    }

    public Result VerifyPassword(string password)
    {
        if (!BCrypt.Net.BCrypt.Verify(password, PasswordHash))
            return Result.Failure(UserErrors.InvalidCredentials);

        if (!IsActive)
            return Result.Failure(UserErrors.Inactive);

        return Result.Success();
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public Result ChangePassword(Password newPassword)
    {
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword.Value);
        return Result.Success();
    }

    public Result UpdateProfile(string username)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            return Result.Failure(UserErrors.InvalidUsername);

        Username = username.Trim();
        return Result.Success();
    }
}