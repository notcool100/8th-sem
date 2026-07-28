using Microsoft.EntityFrameworkCore;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Domain.Entities;

namespace NtbEvent.Infrastructure.Persistence;

public sealed class InvitationRepository : IInvitationRepository
{
    private readonly NtbEventDbContext _dbContext;

    public InvitationRepository(NtbEventDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Invitation> AddAsync(Invitation invitation, CancellationToken cancellationToken = default)
    {
        _dbContext.Invitations.Add(invitation);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return invitation;
    }

    public async Task UpdateAsync(Invitation invitation, CancellationToken cancellationToken = default)
    {
        // The invitation is already tracked (loaded via GetById/GetByToken or just
        // added), so scalar changes are detected automatically. Saving here avoids
        // re-marking the Event/Guest navigations as modified.
        if (_dbContext.Entry(invitation).State == EntityState.Detached)
        {
            _dbContext.Invitations.Attach(invitation);
            _dbContext.Entry(invitation).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Invitation?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Invitations
            .Include(invitation => invitation.Event)
            .Include(invitation => invitation.Guest)
            .FirstOrDefaultAsync(invitation => invitation.Id == id, cancellationToken);
    }

    public Task<Invitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return _dbContext.Invitations
            .Include(invitation => invitation.Event)
            .Include(invitation => invitation.Guest)
            .FirstOrDefaultAsync(invitation => invitation.Token == token, cancellationToken);
    }

    public Task<Invitation?> GetByEventAndGuestAsync(long eventId, long guestId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Invitations
            .FirstOrDefaultAsync(
                invitation => invitation.EventId == eventId && invitation.GuestId == guestId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Invitation>> GetByEventAsync(long eventId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Invitations
            .AsNoTracking()
            .Include(invitation => invitation.Event)
            .Include(invitation => invitation.Guest)
            .Where(invitation => invitation.EventId == eventId)
            .OrderByDescending(invitation => invitation.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Invitation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Invitations
            .AsNoTracking()
            .Include(invitation => invitation.Event)
            .Include(invitation => invitation.Guest)
            .OrderByDescending(invitation => invitation.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddScanAsync(InvitationScan scan, CancellationToken cancellationToken = default)
    {
        _dbContext.InvitationScans.Add(scan);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Invitations.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _dbContext.Invitations.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
