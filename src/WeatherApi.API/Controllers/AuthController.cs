using Azure;
using Azure.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WeatherApi.Application.Users.DTOs;
using WeatherApi.Application.Users.Login;
using WeatherApi.Application.Users.RefreshToken;
using WeatherApi.Application.Users.Register;
using WeatherApi.Application.Users.RevokeToken;
using WeatherApi.Domain.Common.Result;
using WeatherApi.Domain.RefreshTokens;

namespace WeatherApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed-by-ip")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        _logger.LogInformation("Registration request received for {Email}", command.Email);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return HandleFailure(result);

        SetRefreshTokenCookie(result.Value.RefreshToken);

        var response = result.Value with { RefreshToken = string.Empty };
        return Ok(response);
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        _logger.LogInformation("Login request received for {Email}", command.Email);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return HandleFailure(result);

        SetRefreshTokenCookie(result.Value.RefreshToken);

        var response = result.Value with { RefreshToken = string.Empty };
        return Ok(response);
    }

    /// <summary>
    /// Refresh access token using refresh token from cookie
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("Refresh token not found in cookies");
            return Unauthorized(CreateProblemDetails(RefreshTokenErrors.Invalid));
        }

        var command = new RefreshTokenCommand(refreshToken);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return HandleFailure(result);

        SetRefreshTokenCookie(result.Value.RefreshToken);

        var response = result.Value with { RefreshToken = string.Empty };
        return Ok(response);
    }

    /// <summary>
    /// Revoke refresh token (logout)
    /// </summary>
    [HttpPost("revoke-token")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("Revoke failed - refresh token not found in cookies");
            return Unauthorized(CreateProblemDetails(RefreshTokenErrors.Invalid));
        }

        var command = new RevokeTokenCommand(refreshToken);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return HandleFailure(result);

        Response.Cookies.Delete("refreshToken");
        return NoContent();
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    private IActionResult HandleFailure(Result result) => result.Error.Type switch
    {
        ErrorType.Validation => BadRequest(CreateProblemDetails(result.Error)),
        ErrorType.NotFound => NotFound(CreateProblemDetails(result.Error)),
        ErrorType.Conflict => Conflict(CreateProblemDetails(result.Error)),
        ErrorType.Unauthorized => Unauthorized(CreateProblemDetails(result.Error)),
        _ => StatusCode(500, CreateProblemDetails(result.Error))
    };

    private ProblemDetails CreateProblemDetails(Error error) => new()
    {
        Status = GetStatusCode(error.Type),
        Title = error.Code,
        Detail = error.Message,
        Instance = HttpContext?.Request?.Path ?? "unknown"
    };

    private static int GetStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => 400,
        ErrorType.NotFound => 404,
        ErrorType.Conflict => 409,
        ErrorType.Unauthorized => 401,
        _ => 500
    };
}