using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Events;
using NtbEvent.Application.Events.Dtos;

namespace NtbEvent.Application.Services;

/// <summary>
/// TF-IDF + cosine similarity recommendation engine. Each event's corpus document is built by
/// repeating its title tokens 3x and summary tokens 2x (a standard cheap substitute for
/// per-field IDF weighting in a bag-of-words model), then appending description/category/region/tag
/// tokens once each.
/// </summary>
public sealed class RecommendationService : IRecommendationService
{
    private const int TitleWeight = 3;
    private const int SummaryWeight = 2;

    private readonly IEventService _eventService;
    private readonly ITagsRepository _tagsRepository;

    public RecommendationService(IEventService eventService, ITagsRepository tagsRepository)
    {
        _eventService = eventService;
        _tagsRepository = tagsRepository;
    }

    public async Task<IReadOnlyList<EventDto>> GetRecommendationsAsync(
        long eventId,
        int topN = 5,
        CancellationToken cancellationToken = default)
    {
        var target = await _eventService.GetEventByIdAsync(eventId, cancellationToken);
        if (target is null)
        {
            return [];
        }

        var published = await _eventService.GetEventsAsync(
            new EventFilter { IncludeDrafts = false, Status = "published" },
            cancellationToken);

        // Candidates are every other published event; if the target itself isn't published
        // (e.g. still a draft), it's included only to seed the TF-IDF corpus, not as a candidate.
        var candidates = published.Where(e => e.Id != target.Id).ToList();
        if (candidates.Count == 0)
        {
            return [];
        }

        var corpus = candidates.Any(e => e.Id == target.Id) ? candidates : candidates.Prepend(target).ToList();
        var corpusIds = corpus.Select(e => e.Id).ToList();
        var tagsByEventId = await _tagsRepository.GetTagNamesByEventIdsAsync(corpusIds);

        var documents = corpus
            .Select(e => BuildCorpusDocument(e, tagsByEventId.GetValueOrDefault(e.Id, [])))
            .ToList();

        var vectors = TfIdfVectorizer.ComputeTfIdf(documents);

        var targetIndex = corpus.FindIndex(e => e.Id == target.Id);
        var targetVector = vectors[targetIndex];

        var scored = corpus
            .Select((e, i) => (Event: e, Score: TfIdfVectorizer.CosineSimilarity(targetVector, vectors[i])))
            .Where(x => x.Event.Id != target.Id && x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(topN)
            .Select(x => x.Event)
            .ToList();

        return scored;
    }

    private static List<string> BuildCorpusDocument(EventDto @event, IReadOnlyList<string> tags)
    {
        var tokens = new List<string>();

        for (var i = 0; i < TitleWeight; i++)
        {
            tokens.AddRange(TextTokenizer.Tokenize(@event.Title));
        }

        for (var i = 0; i < SummaryWeight; i++)
        {
            tokens.AddRange(TextTokenizer.Tokenize(@event.Summary));
        }

        tokens.AddRange(TextTokenizer.Tokenize(@event.LongDescription));
        tokens.AddRange(TextTokenizer.Tokenize(@event.Category));
        tokens.AddRange(TextTokenizer.Tokenize(@event.Region));
        foreach (var tag in tags)
        {
            tokens.AddRange(TextTokenizer.Tokenize(tag));
        }

        return tokens;
    }
}
