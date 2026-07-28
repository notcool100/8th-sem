using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Contracts.Persistence;

public interface IInvitationRepository
{
    Task<Invitation> AddAsync(Invitation invitation, CancellationToken cancellationToken = default);

    Task UpdateAsync(Invitation invitation, CancellationToken cancellationToken = default);

    /// <summary>Loads an invitation with its Event and Guest navigations populated.</summary>
    Task<Invitation?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Loads an invitation by its secret token (Event + Guest included).</summary>
    Task<Invitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>Returns an existing invitation for the (event, guest) pair, if any.</summary>
    Task<Invitation?> GetByEventAndGuestAsync(long eventId, long guestId, CancellationToken cancellationToken = default);

    /// <summary>All invitations for an event, newest first (Guest included).</summary>
    Task<IReadOnlyList<Invitation>> GetByEventAsync(long eventId, CancellationToken cancellationToken = default);

    /// <summary>All invitations across every event, newest first (Event + Guest included).</summary>
    Task<IReadOnlyList<Invitation>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddScanAsync(InvitationScan scan, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
