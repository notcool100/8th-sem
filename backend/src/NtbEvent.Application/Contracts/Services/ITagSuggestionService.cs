using NtbEvent.Application.Tags.Dtos;

namespace NtbEvent.Application.Contracts.Services;

public interface ITagSuggestionService
{
    /// <summary>
    /// Extracts TF-IDF keywords from <paramref name="title"/> + <paramref name="description"/>
    /// (scored against the published-event corpus) and fuzzy-matches each one to an existing tag
    /// via Levenshtein distance. Unmatched high-weight keywords are still returned as new-tag suggestions.
    /// </summary>
    Task<IReadOnlyList<SuggestedTagDto>> SuggestTagsAsync(
        string title,
        string description,
        int maxSuggestions = 8,
        CancellationToken cancellationToken = default);
}
