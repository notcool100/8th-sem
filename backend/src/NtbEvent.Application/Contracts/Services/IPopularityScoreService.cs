using NtbEvent.Application.Events.Dtos;

namespace NtbEvent.Application.Contracts.Services;

public interface IPopularityScoreService
{
    /// <summary>
    /// Computes a 0-1 popularity score for every event in <paramref name="events"/>, relative to
    /// the rest of the set (rating and engagement are min-max normalized across the corpus), and
    /// assigns it to <see cref="EventDto.PopularityScore"/> on each item in place.
    /// </summary>
    void ScoreAll(IReadOnlyList<EventDto> events, DateTime? asOfUtc = null);

    /// <summary>
    /// Returns <paramref name="events"/> re-ordered by popularity score, descending.
    /// Scores are computed (and assigned) as a side effect, same as <see cref="ScoreAll"/>.
    /// </summary>
    IReadOnlyList<EventDto> RankByPopularity(IReadOnlyList<EventDto> events, DateTime? asOfUtc = null);
}
