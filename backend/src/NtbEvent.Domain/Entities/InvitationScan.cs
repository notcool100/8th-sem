using NtbEvent.Domain.Common;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Domain.Entities;

/// <summary>
/// Audit row recording every scan attempt against an invitation QR. Keeping the
/// scan history in its own table (rather than as repeating columns on
/// <see cref="Invitation"/>) keeps the design in first normal form.
/// </summary>
public sealed class InvitationScan : BaseEntity
{
    public long InvitationId { get; set; }

    public Invitation Invitation { get; set; } = default!;

    /// <summary>Admin who performed the scan.</summary>
    public long ScannedByUserId { get; set; }

    public User ScannedBy { get; set; } = default!;

    public ScanResult Result { get; set; }

    public DateTime ScannedAtUtc { get; set; } = DateTime.UtcNow;
}
