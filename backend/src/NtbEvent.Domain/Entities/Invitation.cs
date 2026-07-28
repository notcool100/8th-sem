using NtbEvent.Domain.Common;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Domain.Entities;

/// <summary>
/// Associative entity linking an <see cref="Event"/> to a <see cref="Guest"/>.
/// Holds invitation-specific data only (the secret token encoded in the QR,
/// status, expiry and check-in audit) — guest details are normalized away into
/// <see cref="Guest"/>. A unique (EventId, GuestId) pair prevents duplicate
/// invitations of the same person to the same event.
/// </summary>
public sealed class Invitation : BaseEntity
{
    public long EventId { get; set; }

    public Event Event { get; set; } = default!;

    public long GuestId { get; set; }

    public Guest Guest { get; set; } = default!;

    /// <summary>Opaque, unguessable token encoded inside the QR code and invite link.</summary>
    public string Token { get; set; } = string.Empty;

    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    /// <summary>Admin who created/sent the invitation.</summary>
    public long InvitedByUserId { get; set; }

    public User InvitedBy { get; set; } = default!;

    public DateTime? SentAtUtc { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }

    public DateTime? VerifiedAtUtc { get; set; }

    /// <summary>Admin who scanned + verified the QR at the door.</summary>
    public long? VerifiedByUserId { get; set; }

    public User? VerifiedBy { get; set; }

    public ICollection<InvitationScan> Scans { get; set; } = [];
}
