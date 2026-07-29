using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Events;
using NtbEvent.Application.Events.Dtos;

namespace NtbEvent.Application.Services;

/// <summary>BM25 (k1=1.5, b=0.75) smart search over title + summary + location + region.</summary>
public sealed class SearchRankingService : ISearchRankingService
{
    private const double K1 = 1.5;
    private const double B = 0.75;

    private readonly IEventService _eventService;

    public SearchRankingService(IEventService eventService)
    {
        _eventService = eventService;
    }

    public async Task<IReadOnlyList<EventDto>> SearchAsync(string? query, CancellationToken cancellationToken = default)
    {
        var published = await _eventService.GetEventsAsync(
            new EventFilter { IncludeDrafts = false, Status = "published" },
            cancellationToken);

        var queryTerms = TextTokenizer.Tokenize(query);
        if (queryTerms.Count == 0)
        {
            return published;
        }

        var documents = published
            .Select(e => TextTokenizer.Tokenize(string.Join(' ', e.Title, e.Summary, e.Location, e.Region)))
            .ToList<IReadOnlyList<string>>();

        var ranked = Bm25Ranker.Rank(documents, queryTerms, K1, B);
        return ranked.Select(r => published[r.DocumentIndex]).ToList();
    }
}
