namespace WeatherApi.Application.Users.DTOs;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    string Username,
    string Email,
    string Role);