using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Contracts.Persistence;

public interface IGuestRepository
{
    Task<Guest?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<Guest> AddAsync(Guest guest, CancellationToken cancellationToken = default);

    Task UpdateAsync(Guest guest, CancellationToken cancellationToken = default);
}
