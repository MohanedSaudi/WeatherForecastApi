using System.Security.Claims;
using WeatherApi.Domain.Users;

namespace WeatherApi.Application.Common.Interfaces;

public interface IJwtService
{
    (string AccessToken, string RefreshToken) GenerateTokens(User user);
    ClaimsPrincipal? ValidateToken(string token);
}