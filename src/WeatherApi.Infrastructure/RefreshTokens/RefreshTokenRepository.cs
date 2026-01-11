using Microsoft.EntityFrameworkCore;
using WeatherApi.Domain.RefreshTokens;
using WeatherApi.Infrastructure.Common.Persistence;

namespace WeatherApi.Infrastructure.RefreshTokens;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context) => _context = context;

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        => await _context.RefreshTokens.AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);

    public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(
        Guid userId,
        CancellationToken ct = default)
        => await _context.RefreshTokens.AsNoTracking()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
        => await _context.RefreshTokens.AddAsync(token, ct);

    public Task UpdateAsync(RefreshToken token, CancellationToken ct = default)
    {
        _context.RefreshTokens.Update(token);
        return Task.CompletedTask;
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, string ipAddress, CancellationToken ct = default)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in tokens)
        {
            token.Revoke(ipAddress);
        }
    }
}