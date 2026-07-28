using Microsoft.EntityFrameworkCore;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Domain.Entities;

namespace NtbEvent.Infrastructure.Persistence;

public sealed class UserPermissionRepository : IUserPermissionRepository
{
    private readonly NtbEventDbContext _dbContext;

    public UserPermissionRepository(NtbEventDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UserPermission>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserPermissions
            .AsNoTracking()
            .Include(p => p.NavItem)
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceUserPermissionsAsync(long userId, IReadOnlyList<UserPermission> permissions, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.UserPermissions
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);

        _dbContext.UserPermissions.RemoveRange(existing);
        _dbContext.UserPermissions.AddRange(permissions);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
