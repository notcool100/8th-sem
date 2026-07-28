namespace NtbEvent.Domain.Enums;

/// <summary>
/// Lifecycle of a self-submitted event registration.
/// </summary>
public enum RegistrationStatus
{
    /// <summary>Submitted by the guest, awaiting admin review.</summary>
    Pending = 1,

    /// <summary>Approved by an admin — the guest is confirmed to attend.</summary>
    Approved = 2,

    /// <summary>Rejected by an admin.</summary>
    Rejected = 3,

    /// <summary>Withdrawn/cancelled.</summary>
    Cancelled = 4
}
