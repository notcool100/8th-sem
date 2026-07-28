using System.ComponentModel.DataAnnotations;

namespace NtbEvent.Application.Registrations.Dtos;

public sealed class RegisterGuestRequest
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

    /// <summary>Submitted values for the event's enabled optional self-registration fields, keyed by field key.</summary>
    public Dictionary<string, string>? AdditionalFields { get; set; }
}
