using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NtbEvent.Application.Festivals.Dtos;

public sealed class SaveFestivalRequest
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

    public List<string> Image { get; set; } = [];

    [Required]
    [MaxLength(120)]
    public string Organizer { get; set; } = string.Empty;

    [MaxLength(160)]
    public string? OrganizerSubtitle { get; set; }

    public bool OrganizerVerified { get; set; } = true;

    [MaxLength(500)]
    public string? OrganizerImageUrl { get; set; }

    public List<SaveFestivalHighlightRequest> Highlights { get; set; } = [];

    public bool Featured { get; set; }

    [MaxLength(40)]
    public string? ReadTime { get; set; }
}
