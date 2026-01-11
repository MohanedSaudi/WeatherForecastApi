using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WeatherApi.Application.Common.Interfaces;
using WeatherApi.Application.Users.DTOs;
using WeatherApi.Domain.Common.Interfaces;
using WeatherApi.Domain.Common.Result;
using WeatherApi.Domain.RefreshTokens;
using WeatherApi.Domain.Users;

namespace WeatherApi.Application.Users.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<RegisterCommandHandler> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(
        RegisterCommand request,
        CancellationToken ct)
    {
        _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result.Failure<AuthResponse>(emailResult.Error);

        var exists = await _userRepository.ExistsAsync(emailResult.Value, ct);
        if (exists)
        {
            _logger.LogWarning("Registration failed - email already exists: {Email}", request.Email);
            return Result.Failure<AuthResponse>(UserErrors.EmailAlreadyExists);
        }

        var passwordResult = Password.Create(request.Password);
        if (passwordResult.IsFailure)
            return Result.Failure<AuthResponse>(passwordResult.Error);

        var userResult = User.Create(request.Username, emailResult.Value, passwordResult.Value);
        if (userResult.IsFailure)
            return Result.Failure<AuthResponse>(userResult.Error);

        var user = userResult.Value;

        await _userRepository.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("User registered successfully: {UserId}", user.Id);

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