using NtbEvent.Application.CalendarSettings.Dtos;

namespace NtbEvent.Application.Contracts.Services;

public interface ICalendarFilterSettingsService
{
    Task<CalendarFilterSettingsDto> GetAsync();
    Task<CalendarFilterSettingsDto> UpdateAsync(CalendarFilterSettingsDto dto);
}
