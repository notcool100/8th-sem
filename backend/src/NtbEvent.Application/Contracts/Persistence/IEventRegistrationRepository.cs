using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Contracts.Persistence;

public interface IEventRegistrationRepository
{
    Task<EventRegistration> AddAsync(EventRegistration registration, CancellationToken cancellationToken = default);

    Task UpdateAsync(EventRegistration registration, CancellationToken cancellationToken = default);

    /// <summary>Loads a registration with its Event and Guest navigations populated.</summary>
    Task<EventRegistration?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Returns an existing registration for the (event, guest) pair, if any.</summary>
    Task<EventRegistration?> GetByEventAndGuestAsync(long eventId, long guestId, CancellationToken cancellationToken = default);

    /// <summary>All registrations for an event, newest first (Guest included).</summary>
    Task<IReadOnlyList<EventRegistration>> GetByEventAsync(long eventId, CancellationToken cancellationToken = default);

    /// <summary>All registrations across every event, newest first (Event + Guest included).</summary>
    Task<IReadOnlyList<EventRegistration>> GetAllAsync(CancellationToken cancellationToken = default);
}
