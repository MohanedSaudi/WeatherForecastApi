using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WeatherApi.Application.Common.Interfaces;

namespace WeatherApi.Infrastructure.Caching;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            _cache.TryGetValue(key, out T? value);

            if (value != null)
                _logger.LogDebug("Cache hit for key: {Key}", key);
            else
                _logger.LogDebug("Cache miss for key: {Key}", key);

            return Task.FromResult(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache for key: {Key}", key);
            return Task.FromResult(default(T));
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan duration, CancellationToken ct = default)
    {
        try
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(duration)
                .SetSize(1);

            _cache.Set(key, value, cacheEntryOptions);

            _logger.LogDebug("Cache set for key: {Key}, Duration: {Duration}", key, duration);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache for key: {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            _cache.Remove(key);
            _logger.LogDebug("Cache removed for key: {Key}", key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache for key: {Key}", key);
            return Task.CompletedTask;
        }
    }
}