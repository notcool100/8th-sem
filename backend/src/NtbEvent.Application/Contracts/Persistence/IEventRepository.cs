using NtbEvent.Application.Events;
using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Contracts.Persistence;

public interface IEventRepository
{
    Task<IReadOnlyList<Event>> GetAsync(EventFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paginated slice of events together with the total matching row count.
    /// Pagination and sorting are driven by <see cref="EventFilter.Page"/>,
    /// <see cref="EventFilter.PageSize"/>, <see cref="EventFilter.SortBy"/>,
    /// and <see cref="EventFilter.SortDir"/>.
    /// </summary>
    Task<(IReadOnlyList<Event> Items, int TotalCount)> GetPagedAsync(EventFilter filter, CancellationToken cancellationToken = default);

    Task<Event?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<Event?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<Event> AddAsync(Event entity, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Event entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
