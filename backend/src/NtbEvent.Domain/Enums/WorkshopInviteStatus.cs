namespace NtbEvent.Domain.Enums;

/// <summary>
/// Lifecycle of a single bulk workshop-invite recipient (plain email blast,
/// no QR/check-in involved).
/// </summary>
public enum WorkshopInviteStatus
{
    /// <summary>Imported but the invitation email has not been sent yet.</summary>
    Pending = 1,

    /// <summary>Invitation email has been sent to this recipient.</summary>
    Sent = 2,

    /// <summary>Guest checked in — the QR has been consumed and can no longer be used.</summary>
    Verified = 3
}
