using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WeatherApi.Application.Common.Interfaces;
using WeatherApi.Application.Users.DTOs;
using WeatherApi.Domain.Common.Interfaces;
using WeatherApi.Domain.Common.Result;
using WeatherApi.Domain.RefreshTokens;
using WeatherApi.Domain.Users;

namespace WeatherApi.Application.Users.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(
        LoginCommand request,
        CancellationToken ct)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result.Failure<AuthResponse>(emailResult.Error);

        var user = await _userRepository.GetByEmailAsync(emailResult.Value, ct);
        if (user == null)
        {
            _logger.LogWarning("Login failed - user not found: {Email}", request.Email);
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);
        }

        var verificationResult = user.VerifyPassword(request.Password);
        if (verificationResult.IsFailure)
        {
            _logger.LogWarning("Login failed - invalid password for: {Email}", request.Email);
            return Result.Failure<AuthResponse>(verificationResult.Error);
        }

        _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var (accessToken, refreshTokenString) = _jwtService.GenerateTokens(user);

        var refreshTokenResult = WeatherApi.Domain.RefreshTokens.RefreshToken.Create(user.Id, refreshTokenString, 7, ipAddress);
        if (refreshTokenResult.IsFailure)
            return Result.Failure<AuthResponse>(refreshTokenResult.Error);

        await _refreshTokenRepository.AddAsync(refreshTokenResult.Value, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var response = new AuthResponse(
            accessToken,
            refreshTokenString,
            user.Username,
            user.Email,
            user.Role.ToString());

        return Result.Success(response);
    }
}