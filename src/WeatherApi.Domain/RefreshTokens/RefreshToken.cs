using WeatherApi.Domain.Common.BaseEntity;
using WeatherApi.Domain.Common.Result;

namespace WeatherApi.Domain.RefreshTokens;

public sealed class RefreshToken : ImmutableEntity
{
    private RefreshToken() { } // EF Core

    private RefreshToken(
        Guid userId,
        string token,
        DateTime expiresAt,
        string createdByIp)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp;
        IsRevoked = false;
    }

    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public string CreatedByIp { get; private set; } = string.Empty;
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    public static Result<RefreshToken> Create(
        Guid userId,
        string token,
        int expirationDays,
        string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Result.Failure<RefreshToken>(Error.Validation(
                "RefreshToken.InvalidToken",
                "Token cannot be empty"));
        }

        var refreshToken = new RefreshToken(
            userId,
            token,
            DateTime.UtcNow.AddDays(expirationDays),
            ipAddress);

        return Result.Success(refreshToken);
    }

    public Result Revoke(string ipAddress, string? replacedByToken = null)
    {
        if (IsRevoked)
            return Result.Failure(RefreshTokenErrors.Revoked);

        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = ipAddress;
        ReplacedByToken = replacedByToken;

        return Result.Success();
    }

    public Result Validate()
    {
        if (IsRevoked)
            return Result.Failure(RefreshTokenErrors.Revoked);

        if (IsExpired)
            return Result.Failure(RefreshTokenErrors.Expired);

        return Result.Success();
    }
}