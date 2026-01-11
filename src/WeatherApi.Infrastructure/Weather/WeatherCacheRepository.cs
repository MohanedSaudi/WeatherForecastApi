using Microsoft.EntityFrameworkCore;
using WeatherApi.Domain.Weather;
using WeatherApi.Infrastructure.Common.Persistence;

namespace WeatherApi.Infrastructure.Weather;

public class WeatherCacheRepository : IWeatherCacheRepository
{
    private readonly ApplicationDbContext _context;

    public WeatherCacheRepository(ApplicationDbContext context) => _context = context;

    public async Task<WeatherCache?> GetByCityAsync(string city, CancellationToken ct = default)
        => await _context.WeatherCaches.AsNoTracking()
            .Where(w => w.City.ToLower() == city.ToLower() && w.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(w => w.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(WeatherCache cache, CancellationToken ct = default)
        => await _context.WeatherCaches.AddAsync(cache, ct);

    public async Task DeleteExpiredAsync(CancellationToken ct = default)
    {
        var expired = await _context.WeatherCaches
            .Where(w => w.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(ct);

        _context.WeatherCaches.RemoveRange(expired);
    }
}