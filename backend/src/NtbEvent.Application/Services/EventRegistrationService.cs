using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Registrations.Dtos;
using NtbEvent.Domain.Entities;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Application.Services;

public sealed class EventRegistrationService : IEventRegistrationService
{
    private readonly IEventRegistrationRepository _registrationRepository;
    private readonly IGuestRepository _guestRepository;
    private readonly IEventRepository _eventRepository;

    public EventRegistrationService(
        IEventRegistrationRepository registrationRepository,
        IGuestRepository guestRepository,
        IEventRepository eventRepository)
    {
        _registrationRepository = registrationRepository;
        _guestRepository = guestRepository;
        _eventRepository = eventRepository;
    }

    public async Task<EventRegistrationDto> RegisterAsync(
        long eventId,
        RegisterGuestRequest request,
        CancellationToken cancellationToken = default)
    {
        var @event = await _eventRepository.GetByIdAsync(eventId, cancellationToken)
            ?? throw new KeyNotFoundException("Event not found.");

        if (!@event.RequiresRegistration)
        {
            throw new InvalidOperationException("This event does not accept self-registration.");
        }

        var email = (request.Email ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.");
        }

        var normalizedEmail = email.ToUpperInvariant();
        var now = DateTime.UtcNow;

        // Upsert the guest (one row per unique email — shared with the invitation flow).
        var guest = await _guestRepository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);
        if (guest is null)
        {
            guest = new Guest
            {
                FullName = request.FullName.Trim(),
                Email = email,
                NormalizedEmail = normalizedEmail,
                Phone = request.Phone?.Trim() ?? string.Empty,
                Organization = request.Organization?.Trim() ?? string.Empty,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            guest = await _guestRepository.AddAsync(guest, cancellationToken);
        }
        else
        {
            guest.FullName = request.FullName.Trim();
            guest.Phone = request.Phone?.Trim() ?? guest.Phone;
            guest.Organization = request.Organization?.Trim() ?? guest.Organization;
            guest.UpdatedAtUtc = now;
            await _guestRepository.UpdateAsync(guest, cancellationToken);
        }

        // Prevent duplicate active registrations. A cancelled/rejected one is
        // reactivated (reusing the row) back into Pending review.
        var existing = await _registrationRepository.GetByEventAndGuestAsync(eventId, guest.Id, cancellationToken);
        if (existing is not null)
        {
            if (existing.Status is RegistrationStatus.Pending or RegistrationStatus.Approved)
            {
                throw new InvalidOperationException("You have already registered for this event.");
            }

            existing.Status = RegistrationStatus.Pending;
            existing.RequestedAtUtc = now;
            existing.ReviewedByUserId = null;
            existing.ReviewedAtUtc = null;
            existing.AdditionalFields = request.AdditionalFields ?? new();
            existing.UpdatedAtUtc = now;
            await _registrationRepository.UpdateAsync(existing, cancellationToken);
            return Map(existing, @event, guest);
        }

        var registration = new EventRegistration
        {
            EventId = eventId,
            GuestId = guest.Id,
            Status = RegistrationStatus.Pending,
            RequestedAtUtc = now,
            AdditionalFields = request.AdditionalFields ?? new(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        registration = await _registrationRepository.AddAsync(registration, cancellationToken);
        return Map(registration, @event, guest);
    }

    public async Task<IReadOnlyList<EventRegistrationDto>> GetForEventAsync(long eventId, CancellationToken cancellationToken = default)
    {
        var registrations = await _registrationRepository.GetByEventAsync(eventId, cancellationToken);
        return registrations.Select(registration => Map(registration, registration.Event, registration.Guest)).ToList();
    }

    public async Task<IReadOnlyList<EventRegistrationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var registrations = await _registrationRepository.GetAllAsync(cancellationToken);
        return registrations.Select(registration => Map(registration, registration.Event, registration.Guest)).ToList();
    }

    public async Task<EventRegistrationDto?> ApproveAsync(long id, long reviewedByUserId, CancellationToken cancellationToken = default)
    {
        var registration = await _registrationRepository.GetByIdAsync(id, cancellationToken);
        if (registration is null)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        registration.Status = RegistrationStatus.Approved;
        registration.ReviewedByUserId = reviewedByUserId;
        registration.ReviewedAtUtc = now;
        registration.UpdatedAtUtc = now;
        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        return Map(registration, registration.Event, registration.Guest);
    }

    public async Task<EventRegistrationDto?> RejectAsync(long id, long reviewedByUserId, CancellationToken cancellationToken = default)
    {
        var registration = await _registrationRepository.GetByIdAsync(id, cancellationToken);
        if (registration is null)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        registration.Status = RegistrationStatus.Rejected;
        registration.ReviewedByUserId = reviewedByUserId;
        registration.ReviewedAtUtc = now;
        registration.UpdatedAtUtc = now;
        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        return Map(registration, registration.Event, registration.Guest);
    }

    private static EventRegistrationDto Map(EventRegistration registration, Event? @event, Guest? guest)
    {
        return new EventRegistrationDto
        {
            Id = registration.Id,
            EventId = registration.EventId,
            EventTitle = @event?.Title ?? string.Empty,
            GuestId = registration.GuestId,
            GuestName = guest?.FullName ?? string.Empty,
            GuestEmail = guest?.Email ?? string.Empty,
            GuestPhone = guest?.Phone ?? string.Empty,
            GuestOrganization = guest?.Organization ?? string.Empty,
            Status = registration.Status.ToString().ToLowerInvariant(),
            RequestedAtUtc = registration.RequestedAtUtc,
            ReviewedAtUtc = registration.ReviewedAtUtc,
            CreatedAtUtc = registration.CreatedAtUtc,
            AdditionalFields = registration.AdditionalFields ?? new()
        };
    }
}
