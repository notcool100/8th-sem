using Microsoft.EntityFrameworkCore;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Domain.Entities;

namespace NtbEvent.Infrastructure.Persistence;

public sealed class NavItemRepository : INavItemRepository
{
    private readonly NtbEventDbContext _dbContext;

    public NavItemRepository(NtbEventDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<NavItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.NavItems
            .AsNoTracking()
            .OrderBy(n => n.SortOrder)
            .ToListAsync(cancellationToken);
    }
}
