using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WeatherApi.Application.Common.Interfaces;
using WeatherApi.Application.Users.DTOs;
using WeatherApi.Domain.Common.Interfaces;
using WeatherApi.Domain.Common.Result;
using WeatherApi.Domain.RefreshTokens;
using WeatherApi.Domain.Users;

namespace WeatherApi.Application.Users.RefreshToken;


public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken ct)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, ct);
        if (token == null)
        {
            _logger.LogWarning("Refresh token not found");
            return Result.Failure<AuthResponse>(RefreshTokenErrors.NotFound);
        }

        var validationResult = token.Validate();
        if (validationResult.IsFailure)
        {
            _logger.LogWarning("Invalid refresh token: {Error}", validationResult.Error.Message);
            return Result.Failure<AuthResponse>(validationResult.Error);
        }

        var user = await _userRepository.GetByIdAsync(token.UserId, ct);
        if (user == null)
        {
            _logger.LogWarning("User not found for refresh token");
            return Result.Failure<AuthResponse>(UserErrors.NotFound);
        }

        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        var (newAccessToken, newRefreshTokenString) = _jwtService.GenerateTokens(user);

        var newRefreshTokenResult = WeatherApi.Domain.RefreshTokens.RefreshToken.Create(user.Id, newRefreshTokenString, 7, ipAddress);
        if (newRefreshTokenResult.IsFailure)
            return Result.Failure<AuthResponse>(newRefreshTokenResult.Error);

        token.Revoke(ipAddress, newRefreshTokenString);
        await _refreshTokenRepository.UpdateAsync(token, ct);

        await _refreshTokenRepository.AddAsync(newRefreshTokenResult.Value, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Token refreshed for user: {UserId}", user.Id);

        var response = new AuthResponse(
            newAccessToken,
            newRefreshTokenString,
            user.Username,
            user.Email,
            user.Role.ToString());

        return Result.Success(response);
    }
}