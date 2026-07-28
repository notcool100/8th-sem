using NtbEvent.Domain.Common;

namespace NtbEvent.Domain.Entities;

/// <summary>
/// Singleton settings row (always Id = 1) controlling which filter
/// sections are visible on the public calendar page.
/// </summary>
public sealed class CalendarFilterSettings : BaseEntity
{
    public bool ShowCategory  { get; set; } = true;
    public bool ShowDateRange { get; set; } = true;
    public bool ShowLocation  { get; set; } = true;
    public bool ShowPrice     { get; set; } = true;
    public bool ShowTags      { get; set; } = true;

    /// <summary>
    /// Whether Sunday is currently treated as a public holiday on the calendar.
    /// Saturday is always a holiday and is not configurable.
    /// </summary>
    public bool IsSundayHoliday { get; set; } = true;
}
