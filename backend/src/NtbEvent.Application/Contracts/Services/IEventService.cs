using NtbEvent.Application.Common;
using NtbEvent.Application.Events;
using NtbEvent.Application.Events.Dtos;

namespace NtbEvent.Application.Contracts.Services;

public interface IEventService
{
    Task<IReadOnlyList<EventDto>> GetEventsAsync(EventFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paginated, sorted slice of events matching <paramref name="filter"/>.
    /// Pagination and sorting are driven by the filter's Page/PageSize/SortBy/SortDir fields.
    /// </summary>
    Task<PagedResult<EventDto>> GetPagedEventsAsync(EventFilter filter, CancellationToken cancellationToken = default);

    Task<EventDto?> GetEventByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<EventDto?> GetEventBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<EventDto> CreateEventAsync(SaveEventRequest request, long createdByUserId, CancellationToken cancellationToken = default);

    Task<EventDto?> UpdateEventAsync(long id, SaveEventRequest request, long updatedByUserId, CancellationToken cancellationToken = default);

    Task<bool> DeleteEventAsync(long id, CancellationToken cancellationToken = default);
}
