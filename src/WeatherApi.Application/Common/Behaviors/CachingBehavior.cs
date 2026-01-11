using MediatR;
using Microsoft.Extensions.Logging;
using WeatherApi.Application.Common.Interfaces;

namespace WeatherApi.Application.Common.Behaviors;

public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan CacheDuration { get; }
}

public class CachingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        ICacheService cacheService,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var cacheKey = request.CacheKey;

        _logger.LogDebug("Checking cache for key: {CacheKey}", cacheKey);

        var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey, ct);

        if (cachedResponse != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cachedResponse;
        }

        _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);

        var response = await next();

        await _cacheService.SetAsync(cacheKey, response, request.CacheDuration, ct);

        return response;
    }
}