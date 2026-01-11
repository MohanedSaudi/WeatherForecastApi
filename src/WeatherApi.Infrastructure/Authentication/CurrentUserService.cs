using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using WeatherApi.Application.Common.Interfaces;

namespace WeatherApi.Infrastructure.Authentication;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    // Fix: Use .FindFirst(...)?.Value instead of .FindFirstValue(...)
    public string? UserId => _httpContextAccessor.HttpContext?.User
        .FindFirst(ClaimTypes.NameIdentifier)?.Value ??
        _httpContextAccessor.HttpContext?.User.FindFirst("userId")?.Value;

    public string? Username => _httpContextAccessor.HttpContext?.User
        .FindFirst(ClaimTypes.Name)?.Value;

    public string? Email => _httpContextAccessor.HttpContext?.User
        .FindFirst(ClaimTypes.Email)?.Value;

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) =>
        _httpContextAccessor.HttpContext?.User.IsInRole(role) ?? false;

    public string GetUserIdentifier() => IsAuthenticated
        ? (Username ?? UserId ?? "Unknown")
        : "Unknown";
}