using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Events;
using NtbEvent.Domain.Entities;

namespace NtbEvent.Tests.TestSupport;

/// <summary>
/// In-memory stand-in for <see cref="IEventRepository"/>, mirroring the filter semantics of the real
/// Dapper-backed <c>EventRepository.BuildWhereClause</c> closely enough to drive system-level tests
/// (see <see cref="Unit.System.EventSystemFlowTests"/>) without a live Postgres instance.
/// </summary>
public sealed class FakeEventRepository : IEventRepository
{
    private readonly List<Event> _events = [];
    private long _nextId = 1;

    public Task<Event> AddAsync(Event entity, CancellationToken cancellationToken = default)
    {
        entity.Id = _nextId++;
        _events.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<Event?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_events.FirstOrDefault(e => e.Id == id));
    }

    public Task<Event?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalized = slug.Trim().ToLowerInvariant();
        return Task.FromResult(_events.FirstOrDefault(e => e.Slug == normalized));
    }

    public Task<bool> UpdateAsync(Event entity, CancellationToken cancellationToken = default)
    {
        var index = _events.FindIndex(e => e.Id == entity.Id);
        if (index < 0)
        {
            return Task.FromResult(false);
        }

        _events[index] = entity;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var removed = _events.RemoveAll(e => e.Id == id) > 0;
        return Task.FromResult(removed);
    }

    public Task<IReadOnlyList<Event>> GetAsync(EventFilter filter, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Event> result = ApplyFilter(filter).ToList();
        return Task.FromResult(result);
    }

    public Task<(IReadOnlyList<Event> Items, int TotalCount)> GetPagedAsync(
        EventFilter filter,
        CancellationToken cancellationToken = default)
    {
        var filtered = ApplyFilter(filter).ToList();
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var items = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult<(IReadOnlyList<Event>, int)>((items, filtered.Count));
    }

    private IEnumerable<Event> ApplyFilter(EventFilter filter)
    {
        var query = _events.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(e => string.Equals(e.Status.ToString(), filter.Status, StringComparison.OrdinalIgnoreCase));
        }
        else if (!filter.IncludeDrafts)
        {
            query = query.Where(e => e.Status == Domain.Enums.EventLifecycleStatus.Published);
        }

        if (!string.IsNullOrWhiteSpace(filter.Region))
        {
            query = query.Where(e => string.Equals(e.Region, filter.Region, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(e => string.Equals(e.Category, filter.Category, StringComparison.OrdinalIgnoreCase));
        }

        return query;
    }
}
