namespace NtbEvent.Domain.Enums;

/// <summary>
/// Lifecycle of a single event invitation / QR pass.
/// </summary>
public enum InvitationStatus
{
    /// <summary>Created but the email has not yet been dispatched.</summary>
    Pending = 1,

    /// <summary>Invitation email (link + QR) has been sent to the guest.</summary>
    Sent = 2,

    /// <summary>Guest checked in — the QR has been consumed and can no longer be used.</summary>
    Verified = 3,

    /// <summary>The invitation passed its expiry date without being used.</summary>
    Expired = 4,

    /// <summary>The invitation was revoked by an admin.</summary>
    Cancelled = 5
}
