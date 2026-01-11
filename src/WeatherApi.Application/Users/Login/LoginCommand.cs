using MediatR;
using WeatherApi.Application.Users.DTOs;
using WeatherApi.Domain.Common.Result;

namespace WeatherApi.Application.Users.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<Result<AuthResponse>>;
