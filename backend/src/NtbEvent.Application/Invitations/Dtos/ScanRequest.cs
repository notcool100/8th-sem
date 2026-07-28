using System.ComponentModel.DataAnnotations;

namespace NtbEvent.Application.Invitations.Dtos;

/// <summary>
/// Sent when an admin scans a QR (or types the code) at the door. The scan step
/// only looks the invitation up and returns the guest info for confirmation; it
/// does not consume the QR.
/// </summary>
public sealed class ScanRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
}
