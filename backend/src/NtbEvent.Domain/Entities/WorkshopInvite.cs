using NtbEvent.Domain.Common;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Domain.Entities;

/// <summary>
/// A recipient imported for a bulk, plain-text workshop invitation email
/// (e.g. from a registration spreadsheet). Distinct from <see cref="Invitation"/>,
/// which issues a QR check-in pass — this is a simple announcement with no QR
/// and no token, sent (or re-sent) on demand by an admin.
/// </summary>
public sealed class WorkshopInvite : BaseEntity
{
    public long EventId { get; set; }

    public Event Event { get; set; } = default!;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string NormalizedEmail { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Organization { get; set; } = string.Empty;

    public WorkshopInviteStatus Status { get; set; } = WorkshopInviteStatus.Pending;

    public DateTime? SentAtUtc { get; set; }

    /// <summary>The exact date this guest was told to attend in their invite email, e.g. day 3 of a multi-day workshop. Falls back to the event's start date until the invite is first sent.</summary>
    public DateTime? WorkshopDateAd { get; set; }

    /// <summary>Opaque, unguessable token encoded inside the QR code and invite link.</summary>
    public string Token { get; set; } = string.Empty;

    public DateTime? VerifiedAtUtc { get; set; }

    /// <summary>Admin who scanned + verified the QR at the door.</summary>
    public long? VerifiedByUserId { get; set; }

    public User? VerifiedBy { get; set; }
}
