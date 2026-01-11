using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WeatherApi.Application.Common.Constants;
using WeatherApi.Application.Weather.DTOs;
using WeatherApi.Application.Weather.GetWeather;
using WeatherApi.Domain.Common.Result;

namespace WeatherApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WeatherController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(IMediator mediator, ILogger<WeatherController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get weather forecast for a specific city
    /// </summary>
    [HttpGet]
    [Authorize(Policy = Policies.RequireUserRole)]
    [EnableRateLimiting("sliding-by-user")]
    [ProducesResponseType(typeof(WeatherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetWeather([FromQuery] string city)
    {
        _logger.LogInformation("Weather request for city: {City}", city);

        var query = new GetWeatherQuery(city);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    /// <summary>
    /// Get weather for multiple cities (Premium users only)
    /// </summary>
    [HttpPost("bulk")]
    [Authorize(Policy = Policies.RequirePremiumRole)]
    [ProducesResponseType(typeof(IEnumerable<WeatherDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetBulkWeather([FromBody] string[] cities)
    {
        _logger.LogInformation("Bulk weather request for {Count} cities", cities.Length);

        var tasks = cities.Select(city => _mediator.Send(new GetWeatherQuery(city)));
        var results = await Task.WhenAll(tasks);

        var successResults = results.Where(r => r.IsSuccess).Select(r => r.Value).ToList();

        return Ok(successResults);
    }

    private IActionResult HandleFailure(Result result) => result.Error.Type switch
    {
        ErrorType.Validation => BadRequest(CreateProblemDetails(result.Error)),
        ErrorType.NotFound => NotFound(CreateProblemDetails(result.Error)),
        ErrorType.Unauthorized => Unauthorized(CreateProblemDetails(result.Error)),
        _ => StatusCode(500, CreateProblemDetails(result.Error))
    };

    private ProblemDetails CreateProblemDetails(Error error) => new()
    {
        Status = GetStatusCode(error.Type),
        Title = error.Code,
        Detail = error.Message,
        Instance = HttpContext.Request.Path
    };

    private static int GetStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => 400,
        ErrorType.NotFound => 404,
        ErrorType.Unauthorized => 401,
        _ => 500
    };
}