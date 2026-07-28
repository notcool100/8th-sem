using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NtbEvent.Application.Events.Dtos;

public sealed class SaveEventRequest
{
    [MaxLength(200)]
    public string? Slug { get; set; }

    [Required]
    [MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(220)]
    public string Summary { get; set; } = string.Empty;

    [Required]
    public string LongDescription { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Type { get; set; } = "event";

    [MaxLength(30)]
    public string? Status { get; set; }

    [Required]
    [JsonPropertyName("date_ad")]
    public DateTime DateAd { get; set; }

    [JsonPropertyName("end_date_ad")]
    public DateTime? EndDateAd { get; set; }

    [JsonPropertyName("date_bs")]
    [MaxLength(20)]
    public string? DateBs { get; set; }

    [JsonPropertyName("end_date_bs")]
    [MaxLength(20)]
    public string? EndDateBs { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; }

    [Required]
    [MaxLength(120)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Region { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Address { get; set; }

    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }

    [MaxLength(140)]
    public string? DateRangeLabel { get; set; }

    [MaxLength(120)]
    public string? DurationLabel { get; set; }

    [MaxLength(120)]
    public string? AttendanceLabel { get; set; }

    public string? AttendanceNote { get; set; }

    [MaxLength(40)]
    public string? EntryType { get; set; }

    [Range(0, 1000000)]
    public decimal Price { get; set; }

    [Range(0, 5)]
    public decimal Rating { get; set; }

    [MaxLength(30)]
    public string? ReviewsLabel { get; set; }

    public List<string> Tags { get; set; } = [];

    public List<string> Image { get; set; } = [];

    [Url]
    public string? MapImage { get; set; }

    [Required]
    [MaxLength(120)]
    public string Organizer { get; set; } = string.Empty;

    [MaxLength(160)]
    public string? OrganizerSubtitle { get; set; }

    public bool OrganizerVerified { get; set; } = true;

    [MaxLength(500)]
    public string? OrganizerImageUrl { get; set; }

    public List<SaveEventHighlightRequest> Highlights { get; set; } = [];

    public bool Featured { get; set; }

    [MaxLength(40)]
    public string? ReadTime { get; set; }

    /// <summary>When true, normal users can self-register via the public registration link.</summary>
    public bool RequiresRegistration { get; set; }

    /// <summary>When true, only admin-issued invitations grant entry; self-registration is disabled.</summary>
    public bool RequiresInvitation { get; set; }

    /// <summary>When true, the entry type badge (Free/Paid) is displayed publicly. Defaults to true.</summary>
    public bool ShowEntryType { get; set; } = true;

    /// <summary>Optional per-event override for the invitation email subject. Leave blank to use the global default.</summary>
    [MaxLength(300)]
    public string? InvitationEmailSubject { get; set; }

    /// <summary>Optional per-event override for the invitation email body (HTML). Leave blank to use the global default.</summary>
    public string? InvitationEmailBodyHtml { get; set; }

    /// <summary>Keys of the optional self-registration fields enabled for this event's public registration form.</summary>
    public List<string> SelfRegistrationFields { get; set; } = [];
}
