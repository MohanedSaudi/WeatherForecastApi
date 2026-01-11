using MediatR;
using WeatherApi.Application.Users.DTOs;
using WeatherApi.Domain.Common.Result;

namespace WeatherApi.Application.Users.Register;

public record RegisterCommand(
    string Username,
    string Email,
    string Password
) : IRequest<Result<AuthResponse>>;


