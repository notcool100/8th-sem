using NtbEvent.Application.WorkshopInvites.Dtos;

namespace NtbEvent.Application.Contracts.Services;

/// <summary>
/// Bulk, plain-text workshop invitation emails — distinct from the QR-based
/// <see cref="IInvitationService"/>. Recipients are imported once (e.g. from a
/// registration spreadsheet) and an admin triggers each send individually.
/// </summary>
public interface IWorkshopInviteService
{
    /// <summary>Imports recipients for an event, skipping any email already imported for it.</summary>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller did not create this event and is not a SuperAdmin.
    /// </exception>
    Task<int> ImportAsync(long eventId, ImportWorkshopInviteesRequest request, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default);

    /// <exception cref="UnauthorizedAccessException">
    /// The caller did not create this event and is not a SuperAdmin.
    /// </exception>
    Task<IReadOnlyList<WorkshopInviteDto>> GetForEventAsync(long eventId, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default);

    /// <summary>All workshop invites across every event — backs the admin Attendees page.</summary>
    Task<IReadOnlyList<WorkshopInviteDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Sends (or re-sends) the workshop invitation email to one recipient.</summary>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller did not create this recipient's event and is not a SuperAdmin.
    /// </exception>
    Task<WorkshopInviteDto?> SendAsync(long id, SendWorkshopInviteRequest request, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default);

    Task<WorkshopInviteDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Public: load a recipient by its QR/invite token (for the guest landing page).</summary>
    Task<WorkshopInviteDto?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>Returns the QR PNG for a recipient.</summary>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller did not create this recipient's event and is not a SuperAdmin.
    /// </exception>
    Task<byte[]?> GetQrPngAsync(long id, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Scan a QR (or paste the code) at the door. Returns the guest info for the
    /// confirmation popup but does NOT consume the QR.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller did not create this recipient's event and is not a SuperAdmin.
    /// </exception>
    Task<WorkshopInviteScanResultDto> ScanAsync(string token, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default);

    /// <summary>Confirm check-in: marks the recipient verified so the QR can't be reused.</summary>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller did not create this recipient's event and is not a SuperAdmin.
    /// </exception>
    Task<WorkshopInviteScanResultDto?> VerifyAsync(long id, long verifiedByUserId, bool isSuperAdmin, CancellationToken cancellationToken = default);
}
