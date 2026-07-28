using System.Text.Json.Serialization;

namespace NtbEvent.Application.Festivals.Dtos;

public sealed class FestivalDto
{
    public long Id { get; set; }

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

    public string Region { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string LongDescription { get; set; } = string.Empty;

    public string DateRangeLabel { get; set; } = string.Empty;

    public string DurationLabel { get; set; } = string.Empty;

    public List<string> Image { get; set; } = [];

    public string Organizer { get; set; } = string.Empty;

    public string OrganizerSubtitle { get; set; } = string.Empty;

    public bool OrganizerVerified { get; set; }

    public string? OrganizerImageUrl { get; set; }

    public List<FestivalHighlightDto> Highlights { get; set; } = [];

    public bool Featured { get; set; }

    public string ReadTime { get; set; } = string.Empty;
}
