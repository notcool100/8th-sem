namespace NtbEvent.Application.WorkshopInvites.Dtos;

public sealed class WorkshopInviteDto
{
    public long Id { get; set; }

    public long EventId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Organization { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime? SentAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string EventTitle { get; set; } = string.Empty;

    public DateTime? EventDateAd { get; set; }

    public string EventLocation { get; set; } = string.Empty;

    /// <summary>Opaque token encoded inside the QR code and invite link.</summary>
    public string Token { get; set; } = string.Empty;

    public string InviteUrl { get; set; } = string.Empty;

    public string QrDataUri { get; set; } = string.Empty;

    public DateTime? VerifiedAtUtc { get; set; }
}
