using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WeatherApi.Domain.Common.Interfaces;
using WeatherApi.Domain.Common.Result;
using WeatherApi.Domain.RefreshTokens;

namespace WeatherApi.Application.Users.RevokeToken;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RevokeTokenCommandHandler> _logger;

    public RevokeTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        ILogger<RevokeTokenCommandHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<Result> Handle(RevokeTokenCommand request, CancellationToken ct)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, ct);
        if (token == null)
        {
            _logger.LogWarning("Revoke failed - token not found");
            return Result.Failure(RefreshTokenErrors.NotFound);
        }

        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var revokeResult = token.Revoke(ipAddress);

        if (revokeResult.IsFailure)
            return revokeResult;

        await _refreshTokenRepository.UpdateAsync(token, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Token revoked successfully");
        return Result.Success();
    }
}