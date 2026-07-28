<script lang="ts">
  import type { PublicEvent } from "./eventTypes";
  import { adToBS, toNepaliNumerals, BS_MONTH_NAMES } from "$lib/components/calendar/dateUtils";

  let {
    events = [],
    dateMode = "AD",
    onOpenEvent = () => {},
  } = $props<{
    events?: PublicEvent[];
    dateMode?: "AD" | "BS" | "BOTH";
    onOpenEvent?: (event: PublicEvent) => void;
  }>();

  // ── Pagination ───────────────────────────────────────────────────────────
  const PAGE_SIZE = 10;
  let currentPage = $state(1);

  const totalPages = $derived(Math.max(1, Math.ceil(events.length / PAGE_SIZE)));

  const pagedEvents = $derived(
    events.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE),
  );

  // Reset to page 1 whenever the filtered list changes
  $effect(() => {
    void events.length;
    currentPage = 1;
  });

  // ── Sort ─────────────────────────────────────────────────────────────────
  type SortKey = "date" | "title";
  type SortDir = "asc" | "desc";
  let sortKey = $state<SortKey>("date");
  let sortDir = $state<SortDir>("asc");

  function toggleSort(key: SortKey) {
    if (sortKey === key) {
      sortDir = sortDir === "asc" ? "desc" : "asc";
    } else {
      sortKey = key;
      sortDir = "asc";
    }
  }

  const sortedEvents = $derived(
    [...events].sort((a, b) => {
      let cmp = 0;
      if (sortKey === "title") {
        cmp = a.title.localeCompare(b.title);
      } else {
        // date
        cmp = new Date(a.date_ad).getTime() - new Date(b.date_ad).getTime();
      }
      return sortDir === "asc" ? cmp : -cmp;
    }),
  );

  const pagedSortedEvents = $derived(
    sortedEvents.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE),
  );

  // ── Helpers ───────────────────────────────────────────────────────────────
  function formatDate(value: Date | string): string {
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return "—";
    return d.toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
    });
  }

  function formatBSDate(value: Date | string): string {
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return "—";
    const bs = adToBS(d);
    return `${BS_MONTH_NAMES[bs.month - 1]} ${toNepaliNumerals(bs.day)}, ${toNepaliNumerals(bs.year)}`;
  }

  function imageOrFallback(images: string[]): string {
    return images?.[0] ?? "";
  }

  function sortIcon(key: SortKey): string {
    if (sortKey !== key) return "fi fi-rr-sort";
    return sortDir === "asc" ? "fi fi-rr-sort-amount-up" : "fi fi-rr-sort-amount-down";
  }

  function goToPage(p: number) {
    currentPage = Math.max(1, Math.min(p, totalPages));
  }
</script>

<div class="list-view">
  <!-- ── Table header ──────────────────────────────────────────────────── -->
  <div class="list-header">
    <div class="col col-image"></div>
    <button class="col col-title sort-btn" onclick={() => toggleSort("title")}>
      Event <i class={sortIcon("title")}></i>
    </button>
    <button class="col col-date sort-btn" onclick={() => toggleSort("date")}>
      Date <i class={sortIcon("date")}></i>
    </button>
    <div class="col col-category">Category</div>
    <div class="col col-location">Location</div>
    <div class="col col-entry">Entry</div>
  </div>

  <!-- ── Rows ───────────────────────────────────────────────────────────── -->
  {#if pagedSortedEvents.length === 0}
    <div class="empty-state">
      <i class="fi fi-rr-calendar-xmark"></i>
      <p>No events match the current filters.</p>
    </div>
  {:else}
    {#each pagedSortedEvents as event (`${event['source'] ?? ''}-${event.id}`)}
      <button
        class="list-row"
        onclick={() => onOpenEvent(event)}
        type="button"
      >
        <!-- Thumbnail -->
        <div class="col col-image">
          {#if imageOrFallback(event.image)}
            <img src={imageOrFallback(event.image)} alt={event.title} class="thumb" />
          {:else}
            <div class="thumb thumb-placeholder" style="background:{event.color ?? '#e5e7eb'}">
              <i class="fi fi-rr-calendar"></i>
            </div>
          {/if}
        </div>

        <!-- Title + summary -->
        <div class="col col-title">
          <span
            class="category-dot"
            style="background:{event.color ?? '#94a3b8'}"
          ></span>
          <div class="title-block">
            <span class="event-title">{event.title}</span>
            {#if event.summary}
              <span class="event-summary">{event.summary}</span>
            {/if}
            {#if event.tags?.length}
              <div class="tag-list">
                {#each event.tags.slice(0, 3) as tag}
                  <span class="tag">{tag}</span>
                {/each}
              </div>
            {/if}
          </div>
        </div>

        <!-- Date -->
        <div class="col col-date">
          {#if dateMode === "BS"}
            <span class="date-primary">{formatBSDate(event.date_ad)}</span>
            <span class="date-secondary">{formatDate(event.date_ad)}</span>
          {:else}
            <span class="date-primary">{formatDate(event.date_ad)}</span>
            <span class="date-secondary">{formatBSDate(event.date_ad)}</span>
          {/if}
          {#if event.durationLabel}
            <span class="duration-label">{event.durationLabel}</span>
          {/if}
        </div>

        <!-- Category -->
        <div class="col col-category">
          {#each event.category.split(',') as cat}
            <span class="badge" style="background:{event.color}15; color:{event.color}; border-color:{event.color}40">
              {cat.trim()}
            </span>
          {/each}
        </div>

        <!-- Location -->
        <div class="col col-location">
          <i class="fi fi-rr-marker"></i>
          <span>{event.location ?? event.region}</span>
        </div>

        <!-- Entry -->
        <div class="col col-entry">
          {#if event.source !== "festival" && event.type !== "festival" && event.showEntryType !== false}
            {#if event.entryType === "Free Entry"}
              <span class="entry-free">Free</span>
            {:else}
              <span class="entry-paid">
                {event.price > 0 ? `$${event.price}` : "Paid"}
              </span>
            {/if}
          {:else}
            <span class="no-rating">—</span>
          {/if}
        </div>
      </button>
    {/each}
  {/if}

  <!-- ── Pagination ─────────────────────────────────────────────────────── -->
  {#if totalPages > 1}
    <div class="pagination">
      <button
        class="page-btn"
        disabled={currentPage === 1}
        onclick={() => goToPage(currentPage - 1)}
      >
        <i class="fi fi-rr-angle-left"></i>
      </button>

      {#each Array.from({ length: totalPages }, (_, i) => i + 1) as p}
        {#if p === 1 || p === totalPages || Math.abs(p - currentPage) <= 1}
          <button
            class="page-btn"
            class:active={p === currentPage}
            onclick={() => goToPage(p)}
          >{p}</button>
        {:else if Math.abs(p - currentPage) === 2}
          <span class="page-ellipsis">…</span>
        {/if}
      {/each}

      <button
        class="page-btn"
        disabled={currentPage === totalPages}
        onclick={() => goToPage(currentPage + 1)}
      >
        <i class="fi fi-rr-angle-right"></i>
      </button>

      <span class="page-info">
        {(currentPage - 1) * PAGE_SIZE + 1}–{Math.min(currentPage * PAGE_SIZE, events.length)} of {events.length}
      </span>
    </div>
  {/if}
</div>

<style>
  .list-view {
    background: white;
    border: 1px solid #e5e7eb;
    border-radius: 18px;
    overflow: hidden;
    margin-bottom: 0.85rem;
  }

  /* ── Column grid ──────────────────────────────────────────────────────── */
  .list-header,
  .list-row {
    display: grid;
    grid-template-columns:
      56px          /* image   */
      1fr           /* title   */
      140px         /* date    */
      120px         /* category*/
      160px         /* location*/
      72px          /* entry   */;
    align-items: center;
    gap: 0;
  }

  /* ── Header ──────────────────────────────────────────────────────────── */
  .list-header {
    background: #f8fafc;
    border-bottom: 1px solid #e5e7eb;
    padding: 0 0.5rem;
    min-height: 44px;
    position: sticky;
    top: 0;
    z-index: 1;
  }

  .list-header .col {
    padding: 0 0.75rem;
    font-size: 0.75rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.06em;
    color: #64748b;
  }

  .sort-btn {
    background: none;
    border: none;
    cursor: pointer;
    display: inline-flex;
    align-items: center;
    gap: 0.3rem;
    color: #64748b;
    font-size: 0.75rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.06em;
    padding: 0 0.75rem;
    min-height: 44px;
    white-space: nowrap;
    transition: color 0.15s;
  }

  .sort-btn:hover {
    color: #bd242b;
  }

  /* ── Row ─────────────────────────────────────────────────────────────── */
  .list-row {
    width: 100%;
    border: none;
    background: white;
    cursor: pointer;
    text-align: left;
    border-bottom: 1px solid #f1f5f9;
    transition: background 0.15s;
    min-height: 80px;
    padding: 0 0.5rem;
  }

  .list-row:last-child {
    border-bottom: none;
  }

  .list-row:hover {
    background: #fafbfc;
  }

  .col {
    padding: 0.75rem;
    overflow: hidden;
  }

  /* ── Image column ────────────────────────────────────────────────────── */
  .col-image {
    padding: 0.5rem 0.5rem 0.5rem 0.75rem;
  }

  .thumb {
    width: 44px;
    height: 44px;
    border-radius: 10px;
    object-fit: cover;
  }

  .thumb-placeholder {
    width: 44px;
    height: 44px;
    border-radius: 10px;
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
    font-size: 1.1rem;
  }

  /* ── Title column ────────────────────────────────────────────────────── */
  .col-title {
    display: flex;
    align-items: flex-start;
    gap: 0.55rem;
  }

  .category-dot {
    flex-shrink: 0;
    width: 8px;
    height: 8px;
    border-radius: 50%;
    margin-top: 6px;
  }

  .title-block {
    display: flex;
    flex-direction: column;
    gap: 0.15rem;
    min-width: 0;
  }

  .event-title {
    font-weight: 700;
    font-size: 0.92rem;
    color: #3f515b;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .event-summary {
    font-size: 0.78rem;
    color: #64748b;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .tag-list {
    display: flex;
    flex-wrap: wrap;
    gap: 0.3rem;
    margin-top: 0.2rem;
  }

  .tag {
    font-size: 0.68rem;
    background: #f1f5f9;
    color: #475569;
    padding: 1px 6px;
    border-radius: 5px;
    font-weight: 600;
    border: 1px solid #e2e8f0;
  }

  /* ── Date column ─────────────────────────────────────────────────────── */
  .col-date {
    display: flex;
    flex-direction: column;
    gap: 0.1rem;
  }

  .date-primary {
    font-size: 0.85rem;
    font-weight: 600;
    color: #334155;
  }

  .date-secondary {
    font-size: 0.72rem;
    color: #94a3b8;
  }

  .duration-label {
    font-size: 0.72rem;
    color: #94a3b8;
    font-style: italic;
  }

  /* ── Category badge ──────────────────────────────────────────────────── */
  .col-category {
    display: flex;
    flex-wrap: wrap;
    gap: 0.3rem;
    align-items: center;
  }

  .badge {
    font-size: 0.72rem;
    font-weight: 700;
    padding: 3px 8px;
    border-radius: 6px;
    border: 1px solid;
    white-space: nowrap;
  }

  /* ── Location column ─────────────────────────────────────────────────── */
  .col-location {
    display: flex;
    align-items: flex-start;
    gap: 0.3rem;
    color: #475569;
    font-size: 0.82rem;
  }

  .col-location i {
    color: #94a3b8;
    font-size: 0.8rem;
    flex-shrink: 0;
    margin-top: 2px;
  }

  .col-location span {
    overflow: hidden;
    text-overflow: ellipsis;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
  }

  .no-rating {
    color: #cbd5e1;
    font-size: 0.85rem;
  }

  /* ── Entry column ────────────────────────────────────────────────────── */
  .entry-free {
    font-size: 0.78rem;
    font-weight: 700;
    color: #15803d;
    background: #dcfce7;
    padding: 2px 8px;
    border-radius: 6px;
    white-space: nowrap;
  }

  .entry-paid {
    font-size: 0.78rem;
    font-weight: 700;
    color: #b45309;
    background: #fef3c7;
    padding: 2px 8px;
    border-radius: 6px;
    white-space: nowrap;
  }

  /* ── Empty state ─────────────────────────────────────────────────────── */
  .empty-state {
    padding: 3.5rem 1rem;
    text-align: center;
    color: #94a3b8;
  }

  .empty-state i {
    font-size: 2.5rem;
    margin-bottom: 0.75rem;
    display: block;
  }

  .empty-state p {
    font-size: 1rem;
    font-weight: 500;
  }

  /* ── Pagination ──────────────────────────────────────────────────────── */
  .pagination {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.35rem;
    padding: 1rem;
    border-top: 1px solid #f1f5f9;
    flex-wrap: wrap;
  }

  .page-btn {
    min-width: 36px;
    height: 36px;
    border-radius: 9px;
    border: 1px solid #e2e8f0;
    background: white;
    color: #475569;
    font-weight: 600;
    font-size: 0.88rem;
    cursor: pointer;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    padding: 0 0.5rem;
    transition: all 0.15s;
  }

  .page-btn:hover:not(:disabled) {
    border-color: #bd242b;
    color: #bd242b;
  }

  .page-btn.active {
    background: #bd242b;
    border-color: #bd242b;
    color: white;
  }

  .page-btn:disabled {
    opacity: 0.4;
    cursor: not-allowed;
  }

  .page-ellipsis {
    color: #94a3b8;
    padding: 0 0.2rem;
    line-height: 36px;
  }

  .page-info {
    margin-left: 0.5rem;
    color: #94a3b8;
    font-size: 0.82rem;
  }

  /* ── Responsive ──────────────────────────────────────────────────────── */
  @media (max-width: 1100px) {
    .list-header,
    .list-row {
      grid-template-columns:
        56px
        1fr
        130px
        110px
        0
        80px
        68px;
    }

    .col-location {
      display: none;
    }
  }

  @media (max-width: 820px) {
    .list-header,
    .list-row {
      grid-template-columns:
        44px
        1fr
        0
        0
        0
        80px
        64px;
    }

    .col-date,
    .col-category,
    .col-location {
      display: none;
    }

    .thumb {
      width: 36px;
      height: 36px;
    }
  }
</style>
