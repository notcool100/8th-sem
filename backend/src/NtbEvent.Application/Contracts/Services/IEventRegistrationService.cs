using NtbEvent.Application.Registrations.Dtos;

namespace NtbEvent.Application.Contracts.Services;

public interface IEventRegistrationService
{
    Task<EventRegistrationDto> RegisterAsync(
        long eventId,
        RegisterGuestRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventRegistrationDto>> GetForEventAsync(long eventId, CancellationToken cancellationToken = default);

    /// <summary>All registrations across every event — backs the admin Attendees view.</summary>
    Task<IReadOnlyList<EventRegistrationDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<EventRegistrationDto?> ApproveAsync(long id, long reviewedByUserId, CancellationToken cancellationToken = default);

    Task<EventRegistrationDto?> RejectAsync(long id, long reviewedByUserId, CancellationToken cancellationToken = default);
}
