using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Contracts.Persistence;

public interface ICalendarFilterSettingsRepository
{
    /// <summary>Returns the single settings row, creating defaults if absent.</summary>
    Task<CalendarFilterSettings> GetAsync();

    /// <summary>Upserts the single settings row (id = 1).</summary>
    Task<CalendarFilterSettings> UpsertAsync(CalendarFilterSettings settings);
}
