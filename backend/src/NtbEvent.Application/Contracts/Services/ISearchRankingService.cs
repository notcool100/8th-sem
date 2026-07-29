using NtbEvent.Application.Events.Dtos;

namespace NtbEvent.Application.Contracts.Services;

public interface ISearchRankingService
{
    /// <summary>
    /// Ranks published events against <paramref name="query"/> using BM25 over each event's
    /// title/summary/location/region. A blank query returns all published events, unranked.
    /// </summary>
    Task<IReadOnlyList<EventDto>> SearchAsync(string? query, CancellationToken cancellationToken = default);
}
