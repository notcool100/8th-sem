namespace NtbEvent.Domain.Enums;

/// <summary>
/// Outcome of an admin scanning / verifying an invitation QR code.
/// </summary>
public enum ScanResult
{
    /// <summary>QR is valid and was successfully verified (check-in completed).</summary>
    Verified = 1,

    /// <summary>QR was previewed but not yet verified (info shown to admin).</summary>
    Previewed = 2,

    /// <summary>QR was already verified earlier — entry must be refused.</summary>
    AlreadyUsed = 3,

    /// <summary>QR has expired.</summary>
    Expired = 4,

    /// <summary>Invitation was cancelled/revoked.</summary>
    Cancelled = 5,

    /// <summary>No invitation matches the scanned token.</summary>
    Invalid = 6
}
