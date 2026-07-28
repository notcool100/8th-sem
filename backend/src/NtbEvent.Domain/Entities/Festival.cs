using NtbEvent.Domain.Common;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Domain.Entities;

public sealed class Festival : BaseEntity
{
    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string LongDescription { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public EventLifecycleStatus Status { get; set; } = EventLifecycleStatus.Draft;

    public DateTime DateAd { get; set; }

    public DateTime EndDateAd { get; set; }

    public string DateBs { get; set; } = string.Empty;

    public string EndDateBs { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string DateRangeLabel { get; set; } = string.Empty;

    public string DurationLabel { get; set; } = string.Empty;

    public List<string> Image { get; set; } = [];

    public string Organizer { get; set; } = string.Empty;

    public string OrganizerSubtitle { get; set; } = string.Empty;

    public bool OrganizerVerified { get; set; }

    public string? OrganizerImageUrl { get; set; }

    public List<FestivalHighlight> Highlights { get; set; } = [];

    public bool Featured { get; set; }

    public string ReadTime { get; set; } = string.Empty;

    public long? CreatedById { get; set; }

    public User? CreatedBy { get; set; }

    public long? UpdatedById { get; set; }

    public User? UpdatedBy { get; set; }
}
