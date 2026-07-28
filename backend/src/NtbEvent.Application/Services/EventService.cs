using System.Globalization;
using System.Text;
using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Events;
using NtbEvent.Application.Events.Dtos;
using NtbEvent.Domain.Entities;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Application.Services;

public sealed class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<EventDto> CreateEventAsync(
        SaveEventRequest request,
        long createdByUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = BuildEntity(request, userId: createdByUserId);
        await EnsureSlugAvailableAsync(entity.Slug, entity.Id, cancellationToken);

        var created = await _eventRepository.AddAsync(entity, cancellationToken);
        return Map(created);
    }

    public async Task<EventDto?> GetEventByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _eventRepository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<EventDto?> GetEventBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var entity = await _eventRepository.GetBySlugAsync(slug, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<IReadOnlyList<EventDto>> GetEventsAsync(
        EventFilter filter,
        CancellationToken cancellationToken = default)
    {
        var events = await _eventRepository.GetAsync(filter, cancellationToken);
        return events.Select(Map).ToList();
    }

    public async Task<PagedResult<EventDto>> GetPagedEventsAsync(
        EventFilter filter,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _eventRepository.GetPagedAsync(filter, cancellationToken);
        return new PagedResult<EventDto>
        {
            Items = items.Select(Map).ToList(),
            TotalCount = totalCount,
            Page = Math.Max(1, filter.Page),
            PageSize = Math.Clamp(filter.PageSize, 1, 100)
        };
    }

    public async Task<EventDto?> UpdateEventAsync(
        long id,
        SaveEventRequest request,
        long updatedByUserId,
        CancellationToken cancellationToken = default)
    {
        var current = await _eventRepository.GetByIdAsync(id, cancellationToken);
        if (current is null)
        {
            return null;
        }

        var entity = BuildEntity(request, current, updatedByUserId);
        entity.Id = id;

        await EnsureSlugAvailableAsync(entity.Slug, id, cancellationToken);

        var updated = await _eventRepository.UpdateAsync(entity, cancellationToken);
        return updated ? Map(entity) : null;
    }

    public Task<bool> DeleteEventAsync(long id, CancellationToken cancellationToken = default)
    {
        return _eventRepository.DeleteAsync(id, cancellationToken);
    }

    private async Task EnsureSlugAvailableAsync(
        string slug,
        long currentEventId,
        CancellationToken cancellationToken)
    {
        var existing = await _eventRepository.GetBySlugAsync(slug, cancellationToken);
        if (existing is not null && existing.Id != currentEventId)
        {
            throw new InvalidOperationException($"An event with slug '{slug}' already exists.");
        }
    }

    private static Event BuildEntity(SaveEventRequest request, Event? current = null, long? userId = null)
    {
        if (request.RequiresRegistration && request.RequiresInvitation)
        {
            throw new ArgumentException("An event cannot require both self-registration and invitation-only access.");
        }

        var startDate = request.DateAd.Date;
        var endDate = request.EndDateAd?.Date ?? startDate;
        var now = DateTime.UtcNow;
        var parsedType = ParseType(request.Type);
        var description = request.LongDescription.Trim();
        var normalizedImages = NormalizeStringList(request.Image);
        var createdById = current?.CreatedById ?? userId ?? 0;
        var updatedById = userId ?? current?.UpdatedById ?? 0;

        return new Event
        {
            Id = current?.Id ?? 0,
            Slug = BuildSlug(request.Slug, request.Title),
            Title = request.Title.Trim(),
            Summary = request.Summary.Trim(),
            LongDescription = description,
            Category = request.Category.Trim(),
            Type = parsedType,
            Status = ParseStatus(request.Status, current?.Status ?? EventLifecycleStatus.Draft),
            DateAd = startDate,
            EndDateAd = endDate,
            DateBs = string.IsNullOrWhiteSpace(request.DateBs)
                ? BuildApproximateBsDate(startDate)
                : request.DateBs.Trim(),
            EndDateBs = string.IsNullOrWhiteSpace(request.EndDateBs)
                ? BuildApproximateBsDate(endDate)
                : request.EndDateBs.Trim(),
            Color = ResolveColor(request.Color, parsedType),
            Location = request.Location.Trim(),
            Region = request.Region.Trim(),
            Address = request.Address?.Trim() ?? string.Empty,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            DateRangeLabel = string.IsNullOrWhiteSpace(request.DateRangeLabel)
                ? BuildDateRangeLabel(startDate, endDate)
                : request.DateRangeLabel.Trim(),
            DurationLabel = string.IsNullOrWhiteSpace(request.DurationLabel)
                ? BuildDurationLabel(startDate, endDate)
                : request.DurationLabel.Trim(),
            AttendanceLabel = request.AttendanceLabel?.Trim() ?? string.Empty,
            AttendanceNote = request.AttendanceNote?.Trim() ?? string.Empty,
            EntryType = ResolveEntryType(request.EntryType, request.Price),
            Price = request.Price,
            Rating = request.Rating,
            ReviewsLabel = request.ReviewsLabel?.Trim() ?? "0",
            // Tags = request.Tags
            //     .Where(tag => !string.IsNullOrWhiteSpace(tag))
            //     .Select(tag => tag.Trim())
            //     .Distinct(StringComparer.OrdinalIgnoreCase)
            //     .ToList(),
            Image = normalizedImages,
            MapImage = request.MapImage?.Trim() ?? string.Empty,
            Organizer = request.Organizer.Trim(),
            OrganizerSubtitle = request.OrganizerSubtitle?.Trim() ?? "Event organizer",
            OrganizerVerified = request.OrganizerVerified,
            OrganizerImageUrl = string.IsNullOrWhiteSpace(request.OrganizerImageUrl) ? null : request.OrganizerImageUrl.Trim(),
            Highlights = request.Highlights
                .Where(highlight =>
                    !string.IsNullOrWhiteSpace(highlight.Title) &&
                    !string.IsNullOrWhiteSpace(highlight.Description))
                .Select(highlight => new EventHighlight
                {
                    Icon = highlight.Icon?.Trim() ?? string.Empty,
                    Title = highlight.Title.Trim(),
                    Description = highlight.Description.Trim(),
                    Tone = highlight.Tone?.Trim() ?? string.Empty
                })
                .ToList(),
            Featured = request.Featured,
            ReadTime = string.IsNullOrWhiteSpace(request.ReadTime)
                ? BuildReadTimeLabel(description)
                : request.ReadTime.Trim(),
            RequiresRegistration = request.RequiresRegistration,
            RequiresInvitation = request.RequiresInvitation,
            ShowEntryType = request.ShowEntryType,
            InvitationEmailSubject = string.IsNullOrWhiteSpace(request.InvitationEmailSubject)
                ? null
                : request.InvitationEmailSubject.Trim(),
            InvitationEmailBodyHtml = string.IsNullOrWhiteSpace(request.InvitationEmailBodyHtml)
                ? null
                : request.InvitationEmailBodyHtml,
            SelfRegistrationFields = request.SelfRegistrationFields ?? [],
            CreatedAtUtc = current?.CreatedAtUtc ?? now,
            UpdatedAtUtc = now,
            CreatedById = createdById,
            UpdatedById = updatedById
        };
    }

    private static string BuildApproximateBsDate(DateTime date)
    {
        var year = date.Year + 56;
        var month = ((date.Month + 8) % 12) + 1;
        return $"{year}-{month:00}-{date.Day:00}";
    }

    private static string BuildDateRangeLabel(DateTime startDate, DateTime endDate)
    {
        if (startDate.Date == endDate.Date)
        {
            return startDate.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture);
        }

        if (startDate.Year == endDate.Year && startDate.Month == endDate.Month)
        {
            return $"{startDate:MMMM dd} - {endDate:dd, yyyy}";
        }

        if (startDate.Year == endDate.Year)
        {
            return $"{startDate:MMMM dd} - {endDate:MMMM dd, yyyy}";
        }

        return $"{startDate:MMMM dd, yyyy} - {endDate:MMMM dd, yyyy}";
    }

    private static string BuildDurationLabel(DateTime startDate, DateTime endDate)
    {
        var totalDays = (endDate.Date - startDate.Date).Days + 1;
        return totalDays <= 1 ? "1 day event" : $"{totalDays} day program";
    }

    private static string BuildSlug(string? proposedSlug, string title)
    {
        var source = string.IsNullOrWhiteSpace(proposedSlug) ? title : proposedSlug;
        var builder = new StringBuilder();
        var previousWasDash = false;

        foreach (var character in source.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasDash = false;
                continue;
            }

            if (!previousWasDash)
            {
                builder.Append('-');
                previousWasDash = true;
            }
        }

        return builder.ToString().Trim('-');
    }

    private static EventDto Map(Event entity)
    {
        return new EventDto
        {
            Id = entity.Id,
            CreatedById = entity.CreatedById,
            Slug = entity.Slug,
            Title = entity.Title,
            DateAd = entity.DateAd,
            EndDateAd = entity.EndDateAd,
            DateBs = entity.DateBs,
            EndDateBs = entity.EndDateBs,
            Color = entity.Color,
            Category = entity.Category,
            Type = entity.Type.ToString().ToLowerInvariant(),
            Status = entity.Status.ToString().ToLowerInvariant(),
            Summary = entity.Summary,
            LongDescription = entity.LongDescription,
            Location = entity.Location,
            Region = entity.Region,
            Address = entity.Address,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            DateRangeLabel = entity.DateRangeLabel,
            DurationLabel = entity.DurationLabel,
            AttendanceLabel = entity.AttendanceLabel,
            AttendanceNote = entity.AttendanceNote,
            EntryType = entity.EntryType,
            Price = entity.Price,
            Rating = entity.Rating,
            ReviewsLabel = entity.ReviewsLabel,
            // Tags = entity.Tags.ToList(),
            Image = entity.Image.ToList(),
            MapImage = entity.MapImage,
            Organizer = entity.Organizer,
            OrganizerSubtitle = entity.OrganizerSubtitle,
            OrganizerVerified = entity.OrganizerVerified,
            OrganizerImageUrl = entity.OrganizerImageUrl,
            Highlights = entity.Highlights
                .Select(highlight => new EventHighlightDto
                {
                    Icon = highlight.Icon,
                    Title = highlight.Title,
                    Description = highlight.Description,
                    Tone = highlight.Tone
                })
                .ToList(),
            Featured = entity.Featured,
            ReadTime = entity.ReadTime,
            RequiresRegistration = entity.RequiresRegistration,
            RequiresInvitation = entity.RequiresInvitation,
            ShowEntryType = entity.ShowEntryType,
            InvitationEmailSubject = entity.InvitationEmailSubject,
            InvitationEmailBodyHtml = entity.InvitationEmailBodyHtml,
            SelfRegistrationFields = entity.SelfRegistrationFields ?? []
        };
    }

    private static EventLifecycleStatus ParseStatus(string? value, EventLifecycleStatus fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        return Enum.TryParse<EventLifecycleStatus>(value, true, out var parsed)
            ? parsed
            : throw new ArgumentException($"Unsupported event status '{value}'.");
    }

    private static EventType ParseType(string value)
    {
        return Enum.TryParse<EventType>(value, true, out var parsed)
            ? parsed
            : throw new ArgumentException($"Unsupported event type '{value}'.");
    }

    private static string ResolveEntryType(string? entryType, decimal price)
    {
        if (!string.IsNullOrWhiteSpace(entryType))
        {
            return entryType.Trim();
        }

        return price > 0 ? "Paid Entry" : "Free Entry";
    }

    private static List<string> NormalizeStringList(IEnumerable<string>? values)
    {
        if (values is null)
        {
            return [];
        }

        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string BuildReadTimeLabel(string description)
    {
        var wordCount = description
            .Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Length;

        var minutes = Math.Max(1, (int)Math.Ceiling(wordCount / 220d));
        return $"{minutes} min read";
    }

    private static string ResolveColor(string? color, EventType type)
    {
        if (!string.IsNullOrWhiteSpace(color))
        {
            return color.Trim();
        }

        return type switch
        {
            EventType.Festival => "#d97706",
            EventType.Holiday => "#7c3aed",
            EventType.Meeting => "#0369a1",
            _ => "#0f766e"
        };
    }
}
