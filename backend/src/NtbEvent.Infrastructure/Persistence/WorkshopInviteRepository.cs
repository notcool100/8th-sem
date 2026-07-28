using Microsoft.EntityFrameworkCore;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Domain.Entities;

namespace NtbEvent.Infrastructure.Persistence;

public sealed class WorkshopInviteRepository : IWorkshopInviteRepository
{
    private readonly NtbEventDbContext _dbContext;

    public WorkshopInviteRepository(NtbEventDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<WorkshopInvite>> GetByEventAsync(long eventId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.WorkshopInvites
            .AsNoTracking()
            .Where(invite => invite.EventId == eventId)
            .OrderBy(invite => invite.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkshopInvite>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.WorkshopInvites
            .AsNoTracking()
            .Include(invite => invite.Event)
            .OrderByDescending(invite => invite.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<WorkshopInvite?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbContext.WorkshopInvites
            .Include(invite => invite.Event)
            .FirstOrDefaultAsync(invite => invite.Id == id, cancellationToken);
    }

    public Task<WorkshopInvite?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return _dbContext.WorkshopInvites
            .Include(invite => invite.Event)
            .FirstOrDefaultAsync(invite => invite.Token == token, cancellationToken);
    }

    public async Task<HashSet<string>> GetNormalizedEmailsByEventAsync(long eventId, CancellationToken cancellationToken = default)
    {
        var emails = await _dbContext.WorkshopInvites
            .AsNoTracking()
            .Where(invite => invite.EventId == eventId)
            .Select(invite => invite.NormalizedEmail)
            .ToListAsync(cancellationToken);

        return new HashSet<string>(emails);
    }

    public async Task AddRangeAsync(IReadOnlyList<WorkshopInvite> invites, CancellationToken cancellationToken = default)
    {
        _dbContext.WorkshopInvites.AddRange(invites);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(WorkshopInvite invite, CancellationToken cancellationToken = default)
    {
        if (_dbContext.Entry(invite).State == EntityState.Detached)
        {
            _dbContext.WorkshopInvites.Attach(invite);
            _dbContext.Entry(invite).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
