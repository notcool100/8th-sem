using NtbEvent.Application.CalendarSettings.Dtos;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Services;

public sealed class CalendarFilterSettingsService : ICalendarFilterSettingsService
{
    private readonly ICalendarFilterSettingsRepository _repo;

    public CalendarFilterSettingsService(ICalendarFilterSettingsRepository repo)
    {
        _repo = repo;
    }

    public async Task<CalendarFilterSettingsDto> GetAsync()
    {
        var entity = await _repo.GetAsync();
        return ToDto(entity);
    }

    public async Task<CalendarFilterSettingsDto> UpdateAsync(CalendarFilterSettingsDto dto)
    {
        var entity = new CalendarFilterSettings
        {
            ShowCategory  = dto.ShowCategory,
            ShowDateRange = dto.ShowDateRange,
            ShowLocation  = dto.ShowLocation,
            ShowPrice     = dto.ShowPrice,
            ShowTags      = dto.ShowTags,
            IsSundayHoliday = dto.IsSundayHoliday,
        };

        var updated = await _repo.UpsertAsync(entity);
        return ToDto(updated);
    }

    private static CalendarFilterSettingsDto ToDto(CalendarFilterSettings e) => new()
    {
        ShowCategory  = e.ShowCategory,
        ShowDateRange = e.ShowDateRange,
        ShowLocation  = e.ShowLocation,
        ShowPrice     = e.ShowPrice,
        ShowTags      = e.ShowTags,
        IsSundayHoliday = e.IsSundayHoliday,
    };
}
