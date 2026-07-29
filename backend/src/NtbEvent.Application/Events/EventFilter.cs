namespace NtbEvent.Application.Events;

public sealed class EventFilter
{
    public string? Category { get; set; }

    public DateTime? FromDate { get; set; }

    public bool IncludeDrafts { get; set; }

    public string? Region { get; set; }

    public string? Search { get; set; }

    public string? Status { get; set; }

    public DateTime? ToDate { get; set; }

    // ─── Pagination ────────────────────────────────────────────────────────────

    /// <summary>1-based page number.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Maximum rows per page. Clamped to 1–100 in the controller.</summary>
    public int PageSize { get; set; } = 20;

    // ─── Sorting ───────────────────────────────────────────────────────────────

    /// <summary>Column to sort by: "date" | "title" | "rating" | "popularity". "popularity" is computed in-memory (see <see cref="NtbEvent.Application.Contracts.Services.IPopularityScoreService"/>) and only applies to the non-paged <c>GetEventsAsync</c> path.</summary>
    public string SortBy { get; set; } = "date";

    /// <summary>Sort direction: "asc" | "desc".</summary>
    public string SortDir { get; set; } = "asc";
}
