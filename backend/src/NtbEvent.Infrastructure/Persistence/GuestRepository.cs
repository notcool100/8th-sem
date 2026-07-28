using Microsoft.EntityFrameworkCore;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Domain.Entities;

namespace NtbEvent.Infrastructure.Persistence;

public sealed class GuestRepository : IGuestRepository
{
    private readonly NtbEventDbContext _dbContext;

    public GuestRepository(NtbEventDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Guest?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return _dbContext.Guests
            .FirstOrDefaultAsync(guest => guest.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public async Task<Guest> AddAsync(Guest guest, CancellationToken cancellationToken = default)
    {
        _dbContext.Guests.Add(guest);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return guest;
    }

    public async Task UpdateAsync(Guest guest, CancellationToken cancellationToken = default)
    {
        _dbContext.Guests.Update(guest);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
