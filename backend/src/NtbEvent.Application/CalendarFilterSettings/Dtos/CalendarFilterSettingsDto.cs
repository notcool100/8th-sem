namespace NtbEvent.Application.CalendarSettings.Dtos;

public sealed class CalendarFilterSettingsDto
{
    public bool ShowCategory  { get; set; } = true;
    public bool ShowDateRange { get; set; } = true;
    public bool ShowLocation  { get; set; } = true;
    public bool ShowPrice     { get; set; } = true;
    public bool ShowTags      { get; set; } = true;
    public bool IsSundayHoliday { get; set; } = true;
}
