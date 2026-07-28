using System.Text.Json.Serialization;

namespace NtbEvent.Application.Events.Dtos;

public sealed class EventDto
{
    public long Id { get; set; }

    public long? CreatedById { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("date_ad")]
    public DateTime DateAd { get; set; }

    [JsonPropertyName("end_date_ad")]
    public DateTime EndDateAd { get; set; }

    [JsonPropertyName("date_bs")]
    public string DateBs { get; set; } = string.Empty;

    [JsonPropertyName("end_date_bs")]
    public string EndDateBs { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string LongDescription { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string DateRangeLabel { get; set; } = string.Empty;

    public string DurationLabel { get; set; } = string.Empty;

    public string AttendanceLabel { get; set; } = string.Empty;

    public string AttendanceNote { get; set; } = string.Empty;

    public string EntryType { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public decimal Rating { get; set; }

    public string ReviewsLabel { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = [];

    public List<string> Image { get; set; } = [];

    public string MapImage { get; set; } = string.Empty;

    public string Organizer { get; set; } = string.Empty;

    public string OrganizerSubtitle { get; set; } = string.Empty;

    public bool OrganizerVerified { get; set; }

    public string? OrganizerImageUrl { get; set; }

    public List<EventHighlightDto> Highlights { get; set; } = [];

    public bool Featured { get; set; }

    public string ReadTime { get; set; } = string.Empty;

    public bool RequiresRegistration { get; set; }

    public bool RequiresInvitation { get; set; }

    public bool ShowEntryType { get; set; }

    /// <summary>Per-event override for the invitation email subject. Null/blank falls back to the global default.</summary>
    public string? InvitationEmailSubject { get; set; }

    /// <summary>Per-event override for the invitation email body (HTML). Null/blank falls back to the global default.</summary>
    public string? InvitationEmailBodyHtml { get; set; }

    /// <summary>Keys of the optional self-registration fields enabled for this event's public registration form.</summary>
    public List<string> SelfRegistrationFields { get; set; } = [];
}
