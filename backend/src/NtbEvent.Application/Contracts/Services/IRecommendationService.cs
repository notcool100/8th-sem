using NtbEvent.Application.Events.Dtos;

namespace NtbEvent.Application.Contracts.Services;

public interface IRecommendationService
{
    /// <summary>
    /// Returns the top <paramref name="topN"/> published events most similar to <paramref name="eventId"/>,
    /// by TF-IDF + cosine similarity over each event's title/summary/description/category/region/tags.
    /// Returns an empty list if the event doesn't exist or there's nothing else to compare it against.
    /// </summary>
    Task<IReadOnlyList<EventDto>> GetRecommendationsAsync(long eventId, int topN = 5, CancellationToken cancellationToken = default);
}
