using Microsoft.EntityFrameworkCore;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Domain.Entities;

namespace NtbEvent.Infrastructure.Persistence;

public sealed class CalendarFilterSettingsRepository : ICalendarFilterSettingsRepository
{
    private readonly NtbEventDbContext _db;

    public CalendarFilterSettingsRepository(NtbEventDbContext db)
    {
        _db = db;
    }

    public async Task<CalendarFilterSettings> GetAsync()
    {
        var settings = await _db.CalendarFilterSettings.FirstOrDefaultAsync();

        if (settings is not null)
            return settings;

        // Seed default row
        settings = new CalendarFilterSettings { Id = 1 };
        _db.CalendarFilterSettings.Add(settings);
        await _db.SaveChangesAsync();
        return settings;
    }

    public async Task<CalendarFilterSettings> UpsertAsync(CalendarFilterSettings incoming)
    {
        var existing = await _db.CalendarFilterSettings.FirstOrDefaultAsync();

        if (existing is null)
        {
            incoming.Id = 1;
            _db.CalendarFilterSettings.Add(incoming);
            await _db.SaveChangesAsync();
            return incoming;
        }

        existing.ShowCategory  = incoming.ShowCategory;
        existing.ShowDateRange = incoming.ShowDateRange;
        existing.ShowLocation  = incoming.ShowLocation;
        existing.ShowPrice     = incoming.ShowPrice;
        existing.ShowTags      = incoming.ShowTags;
        existing.IsSundayHoliday = incoming.IsSundayHoliday;
        existing.UpdatedAtUtc  = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return existing;
    }
}
