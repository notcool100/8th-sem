namespace NtbEvent.Application.Invitations.Dtos;

public sealed class InvitationDto
{
    public long Id { get; set; }

    public long EventId { get; set; }

    public string EventTitle { get; set; } = string.Empty;

    public DateTime? EventDateAd { get; set; }

    public string EventLocation { get; set; } = string.Empty;

    public long GuestId { get; set; }

    public string GuestName { get; set; } = string.Empty;

    public string GuestEmail { get; set; } = string.Empty;

    public string GuestPhone { get; set; } = string.Empty;

    public string GuestOrganization { get; set; } = string.Empty;

    /// <summary>Opaque token (also encoded in the QR). Used for manual check-in entry.</summary>
    public string Token { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    /// <summary>Absolute URL the guest can open to view their invitation.</summary>
    public string InviteUrl { get; set; } = string.Empty;

    /// <summary>QR image as a data URI (image/png;base64) — handy for display.</summary>
    public string QrDataUri { get; set; } = string.Empty;

    public DateTime? SentAtUtc { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }

    public DateTime? VerifiedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
