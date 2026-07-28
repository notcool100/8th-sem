using NtbEvent.Application.Invitations.Dtos;

namespace NtbEvent.Application.Contracts.Services;

public interface IInvitationService
{
    /// <summary>
    /// Creates (or reuses) the guest, creates an invitation with a fresh token,
    /// generates the QR and emails the guest the invite link + QR code.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller did not create this event and is not a SuperAdmin.
    /// </exception>
    Task<InvitationDto> InviteAsync(long eventId, InviteGuestRequest request, long invitedByUserId, bool isSuperAdmin, CancellationToken cancellationToken = default);

    /// <exception cref="UnauthorizedAccessException">
    /// The caller did not create this event and is not a SuperAdmin.
    /// </exception>
    Task<IReadOnlyList<InvitationDto>> GetForEventAsync(long eventId, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default);

    /// <summary>All invitations across every event — backs the admin Attendees page.</summary>
    Task<IReadOnlyList<InvitationDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <exception cref="UnauthorizedAccessException">
    /// The caller did not create this invitation's event and is not a SuperAdmin.
    /// </exception>
    Task<InvitationDto?> GetByIdAsync(long id, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default);

    Task<InvitationDto?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>Re-sends the invitation email (link + QR).</summary>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller did not create this invitation's event and is not a SuperAdmin.
    /// </exception>
    Task<InvitationDto?> ResendAsync(long id, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Looks up an invitation by scanned token and returns the guest info for the
    /// confirmation popup. Records a scan audit row. Does NOT consume the QR.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller did not create this invitation's event and is not a SuperAdmin.
    /// </exception>
    Task<ScanResultDto> ScanAsync(string token, long scannedByUserId, bool isSuperAdmin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirms check-in: marks the invitation Verified and expires the QR so the
    /// same code cannot be scanned again. Returns null if the invitation is missing.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller did not create this invitation's event and is not a SuperAdmin.
    /// </exception>
    Task<ScanResultDto?> VerifyAsync(long id, long verifiedByUserId, bool isSuperAdmin, CancellationToken cancellationToken = default);

    /// <summary>Revokes an invitation.</summary>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller did not create this invitation's event and is not a SuperAdmin.
    /// </exception>
    Task<bool> CancelAsync(long id, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default);

    /// <summary>Returns the QR PNG for an invitation, or null if not found.</summary>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller did not create this invitation's event and is not a SuperAdmin.
    /// </exception>
    Task<byte[]?> GetQrPngAsync(long id, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default);
}
