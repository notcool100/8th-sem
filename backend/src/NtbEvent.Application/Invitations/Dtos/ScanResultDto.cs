namespace NtbEvent.Application.Invitations.Dtos;

/// <summary>
/// Returned to the admin after scanning a QR. Drives the confirmation popup:
/// when <see cref="CanVerify"/> is true the admin may press "Verify" to check
/// the guest in (which then expires the QR).
/// </summary>
public sealed class ScanResultDto
{
    /// <summary>One of: verified, previewed, alreadyused, expired, cancelled, invalid.</summary>
    public string Result { get; set; } = string.Empty;

    /// <summary>Human-friendly message for the popup.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>True when the QR is valid and not yet used — the admin can confirm check-in.</summary>
    public bool CanVerify { get; set; }

    /// <summary>Invitation details (null when the token is invalid).</summary>
    public InvitationDto? Invitation { get; set; }
}
