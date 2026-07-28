namespace NtbEvent.Application.Common;

/// <summary>
/// A paginated slice of <typeparamref name="T"/> with metadata about the full result set.
/// </summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];

    public int TotalCount { get; init; }

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    public bool HasPrevious => Page > 1;

    public bool HasNext => Page < TotalPages;
}
