using System.Globalization;
using System.Text;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Festivals.Dtos;
using NtbEvent.Domain.Entities;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Application.Services;

public sealed class FestivalService : IFestivalService
{
    private readonly IFestivalRepository _festivalRepository;

    public FestivalService(IFestivalRepository festivalRepository)
    {
        _festivalRepository = festivalRepository;
    }

    public async Task<FestivalDto> CreateFestivalAsync(
        SaveFestivalRequest request,
        long createdByUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = BuildEntity(request, userId: createdByUserId);
        await EnsureSlugAvailableAsync(entity.Slug, entity.Id, cancellationToken);

        var created = await _festivalRepository.AddAsync(entity, cancellationToken);
        return Map(created);
    }

    public async Task<FestivalDto?> GetFestivalByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _festivalRepository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<IReadOnlyList<FestivalDto>> GetFestivalsAsync(CancellationToken cancellationToken = default)
    {
        var festivals = await _festivalRepository.GetAllAsync(cancellationToken);
        return festivals.Select(Map).ToList();
    }

    public async Task<FestivalDto?> UpdateFestivalAsync(
        long id,
        SaveFestivalRequest request,
        long updatedByUserId,
        CancellationToken cancellationToken = default)
    {
        var current = await _festivalRepository.GetByIdAsync(id, cancellationToken);
        if (current is null)
        {
            return null;
        }

        var entity = BuildEntity(request, current, updatedByUserId);
        entity.Id = id;

        await EnsureSlugAvailableAsync(entity.Slug, id, cancellationToken);

        var updated = await _festivalRepository.UpdateAsync(entity, cancellationToken);
        return updated ? Map(entity) : null;
    }

    public Task<bool> DeleteFestivalAsync(long id, CancellationToken cancellationToken = default)
    {
        return _festivalRepository.DeleteAsync(id, cancellationToken);
    }

    private async Task EnsureSlugAvailableAsync(
        string slug,
        long currentFestivalId,
        CancellationToken cancellationToken)
    {
        var existing = await _festivalRepository.GetBySlugAsync(slug, cancellationToken);
        if (existing is not null && existing.Id != currentFestivalId)
        {
            throw new InvalidOperationException($"A festival with slug '{slug}' already exists.");
        }
    }

    private static Festival BuildEntity(SaveFestivalRequest request, Festival? current = null, long? userId = null)
    {
        var startDate = request.DateAd.Date;
        var endDate = request.EndDateAd?.Date ?? startDate;
        var now = DateTime.UtcNow;
        var description = request.LongDescription.Trim();
        var normalizedImages = NormalizeStringList(request.Image);
        var createdById = current?.CreatedById ?? userId ?? 0;
        var updatedById = userId ?? current?.UpdatedById ?? 0;

        return new Festival
        {
            Id = current?.Id ?? 0,
            Slug = BuildSlug(request.Slug, request.Title),
            Title = request.Title.Trim(),
            Summary = request.Summary.Trim(),
            LongDescription = description,
            Category = request.Category.Trim(),
            Status = ParseStatus(request.Status, current?.Status ?? EventLifecycleStatus.Draft),
            DateAd = startDate,
            EndDateAd = endDate,
            DateBs = string.IsNullOrWhiteSpace(request.DateBs)
                ? BuildApproximateBsDate(startDate)
                : request.DateBs.Trim(),
            EndDateBs = string.IsNullOrWhiteSpace(request.EndDateBs)
                ? BuildApproximateBsDate(endDate)
                : request.EndDateBs.Trim(),
            Color = string.IsNullOrWhiteSpace(request.Color) ? "#d97706" : request.Color.Trim(),
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
            Image = normalizedImages,
            Organizer = request.Organizer.Trim(),
            OrganizerSubtitle = request.OrganizerSubtitle?.Trim() ?? "Festival organizer",
            OrganizerVerified = request.OrganizerVerified,
            OrganizerImageUrl = string.IsNullOrWhiteSpace(request.OrganizerImageUrl) ? null : request.OrganizerImageUrl.Trim(),
            Highlights = request.Highlights
                .Where(highlight =>
                    !string.IsNullOrWhiteSpace(highlight.Title) &&
                    !string.IsNullOrWhiteSpace(highlight.Description))
                .Select(highlight => new FestivalHighlight
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

    private static FestivalDto Map(Festival entity)
    {
        return new FestivalDto
        {
            Id = entity.Id,
            Slug = entity.Slug,
            Title = entity.Title,
            DateAd = entity.DateAd,
            EndDateAd = entity.EndDateAd,
            DateBs = entity.DateBs,
            EndDateBs = entity.EndDateBs,
            Color = entity.Color,
            Region = entity.Region,
            Address = entity.Address,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            Category = entity.Category,
            Status = entity.Status.ToString().ToLowerInvariant(),
            Summary = entity.Summary,
            LongDescription = entity.LongDescription,
            DateRangeLabel = entity.DateRangeLabel,
            DurationLabel = entity.DurationLabel,
            Image = entity.Image.ToList(),
            Organizer = entity.Organizer,
            OrganizerSubtitle = entity.OrganizerSubtitle,
            OrganizerVerified = entity.OrganizerVerified,
            OrganizerImageUrl = entity.OrganizerImageUrl,
            Highlights = entity.Highlights
                .Select(highlight => new FestivalHighlightDto
                {
                    Icon = highlight.Icon,
                    Title = highlight.Title,
                    Description = highlight.Description,
                    Tone = highlight.Tone
                })
                .ToList(),
            Featured = entity.Featured,
            ReadTime = entity.ReadTime
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
            : throw new ArgumentException($"Unsupported festival status '{value}'.");
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
}
