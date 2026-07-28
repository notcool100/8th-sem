using Microsoft.EntityFrameworkCore;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Domain.Entities;

namespace NtbEvent.Infrastructure.Persistence;

public sealed class EventRegistrationRepository : IEventRegistrationRepository
{
    private readonly NtbEventDbContext _dbContext;

    public EventRegistrationRepository(NtbEventDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EventRegistration> AddAsync(EventRegistration registration, CancellationToken cancellationToken = default)
    {
        _dbContext.EventRegistrations.Add(registration);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return registration;
    }

    public async Task UpdateAsync(EventRegistration registration, CancellationToken cancellationToken = default)
    {
        if (_dbContext.Entry(registration).State == EntityState.Detached)
        {
            _dbContext.EventRegistrations.Attach(registration);
            _dbContext.Entry(registration).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<EventRegistration?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbContext.EventRegistrations
            .Include(registration => registration.Event)
            .Include(registration => registration.Guest)
            .FirstOrDefaultAsync(registration => registration.Id == id, cancellationToken);
    }

    public Task<EventRegistration?> GetByEventAndGuestAsync(long eventId, long guestId, CancellationToken cancellationToken = default)
    {
        return _dbContext.EventRegistrations
            .FirstOrDefaultAsync(
                registration => registration.EventId == eventId && registration.GuestId == guestId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<EventRegistration>> GetByEventAsync(long eventId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.EventRegistrations
            .AsNoTracking()
            .Include(registration => registration.Event)
            .Include(registration => registration.Guest)
            .Where(registration => registration.EventId == eventId)
            .OrderByDescending(registration => registration.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EventRegistration>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.EventRegistrations
            .AsNoTracking()
            .Include(registration => registration.Event)
            .Include(registration => registration.Guest)
            .OrderByDescending(registration => registration.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
