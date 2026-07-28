using Microsoft.EntityFrameworkCore;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Domain.Entities;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Infrastructure.Persistence;

public sealed class UserRepository : IUserRepository
{
    private readonly NtbEventDbContext _dbContext;

    public UserRepository(NtbEventDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .Include(u => u.Permissions)
            .ThenInclude(p => p.NavItem)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .FirstOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Include(u => u.Permissions)
            .ThenInclude(p => p.NavItem)
            .OrderByDescending(user => user.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<long>> GetAdminUserIdsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.IsActive && (u.Role == UserRole.Admin || u.Role == UserRole.SuperAdmin))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
    }
}
