using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Events;
using NtbEvent.Application.Tags.Dtos;

namespace NtbEvent.Application.Services;

/// <summary>
/// Extracts candidate tags from event title/description via TF-IDF (scored against the published-event
/// corpus), then fuzzy-matches each extracted keyword to an existing tag via Levenshtein distance so
/// near-duplicate tags ("workshop" vs "workshops") collapse onto the canonical existing one.
/// </summary>
public sealed class TagSuggestionService : ITagSuggestionService
{
    private const double FuzzyMatchThreshold = 0.72;

    private readonly IEventService _eventService;
    private readonly ITagsRepository _tagsRepository;

    public TagSuggestionService(IEventService eventService, ITagsRepository tagsRepository)
    {
        _eventService = eventService;
        _tagsRepository = tagsRepository;
    }

    public async Task<IReadOnlyList<SuggestedTagDto>> SuggestTagsAsync(
        string title,
        string description,
        int maxSuggestions = 8,
        CancellationToken cancellationToken = default)
    {
        var candidateTokens = TextTokenizer.Tokenize(title).Concat(TextTokenizer.Tokenize(description)).ToList();
        if (candidateTokens.Count == 0)
        {
            return [];
        }

        var published = await _eventService.GetEventsAsync(
            new EventFilter { IncludeDrafts = false, Status = "published" },
            cancellationToken);

        var corpusDocuments = published
            .Select(e => TextTokenizer.Tokenize(e.Title).Concat(TextTokenizer.Tokenize(e.LongDescription)).ToList())
            .ToList<IReadOnlyList<string>>();
        corpusDocuments.Add(candidateTokens);

        var vectors = TfIdfVectorizer.ComputeTfIdf(corpusDocuments);
        var candidateVector = vectors[^1];
        if (candidateVector.Count == 0)
        {
            return [];
        }

        var maxWeight = candidateVector.Values.Max();
        var rankedKeywords = candidateVector
            .OrderByDescending(kvp => kvp.Value)
            .Take(Math.Max(1, maxSuggestions * 2))
            .Select(kvp => (Keyword: kvp.Key, Score: maxWeight == 0 ? 0 : kvp.Value / maxWeight))
            .ToList();

        var existingTags = await _tagsRepository.GetAllNamesAsync();

        var suggestions = new List<SuggestedTagDto>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (keyword, score) in rankedKeywords)
        {
            if (suggestions.Count >= maxSuggestions)
            {
                break;
            }

            var bestMatch = FindBestExistingTag(keyword, existingTags);
            var resolvedTag = bestMatch ?? Capitalize(keyword);

            if (!seen.Add(resolvedTag))
            {
                continue;
            }

            suggestions.Add(new SuggestedTagDto
            {
                Tag = resolvedTag,
                IsExistingTag = bestMatch is not null,
                Score = Math.Round(score, 4)
            });
        }

        return suggestions;
    }

    private static string? FindBestExistingTag(string keyword, IReadOnlyList<string> existingTags)
    {
        string? best = null;
        var bestSimilarity = 0.0;

        foreach (var tag in existingTags)
        {
            var similarity = LevenshteinDistance.NormalizedSimilarity(keyword, tag);
            if (similarity > bestSimilarity)
            {
                bestSimilarity = similarity;
                best = tag;
            }
        }

        return bestSimilarity >= FuzzyMatchThreshold ? best : null;
    }

    private static string Capitalize(string keyword)
    {
        return keyword.Length == 0 ? keyword : char.ToUpperInvariant(keyword[0]) + keyword[1..];
    }
}
