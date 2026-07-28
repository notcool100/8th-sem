using System.ComponentModel.DataAnnotations;

namespace NtbEvent.Application.Invitations.Dtos;

/// <summary>
/// Basic guest details an admin enters when inviting a person to an event.
/// </summary>
public sealed class InviteGuestRequest
{
    [Required]
    [MaxLength(160)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(40)]
    public string? Phone { get; set; }

    [MaxLength(160)]
    public string? Organization { get; set; }

    /// <summary>Optional UTC expiry for the invitation/QR. Defaults to the event end date.</summary>
    public DateTime? ExpiresAtUtc { get; set; }
}
