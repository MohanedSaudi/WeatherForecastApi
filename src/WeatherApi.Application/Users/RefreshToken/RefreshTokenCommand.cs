using MediatR;
using WeatherApi.Application.Users.DTOs;
using WeatherApi.Domain.Common.Result;

namespace WeatherApi.Application.Users.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthResponse>>;
