namespace NtbEvent.Application.Registrations.Dtos;

public sealed class EventRegistrationDto
{
    public long Id { get; set; }

    public long EventId { get; set; }

    public string EventTitle { get; set; } = string.Empty;

    public long GuestId { get; set; }

    public string GuestName { get; set; } = string.Empty;

    public string GuestEmail { get; set; } = string.Empty;

    public string GuestPhone { get; set; } = string.Empty;

    public string GuestOrganization { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime RequestedAtUtc { get; set; }

    public DateTime? ReviewedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Submitted values for the event's enabled optional self-registration fields, keyed by field key.</summary>
    public Dictionary<string, string> AdditionalFields { get; set; } = new();
}
