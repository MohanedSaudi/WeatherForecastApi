using MediatR;
using WeatherApi.Domain.Common.Result;

namespace WeatherApi.Application.Users.RevokeToken;

public record RevokeTokenCommand(string RefreshToken) : IRequest<Result>;
