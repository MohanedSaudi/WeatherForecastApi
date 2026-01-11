using Microsoft.EntityFrameworkCore;
using WeatherApi.Domain.Users;
using WeatherApi.Infrastructure.Common.Persistence;

namespace WeatherApi.Infrastructure.Users;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);

    public async Task<bool> ExistsAsync(string email, CancellationToken ct = default)
        => await _context.Users.AsNoTracking().AnyAsync(u => u.Email == email.ToLower(), ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _context.Users.AddAsync(user, ct);

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }
}