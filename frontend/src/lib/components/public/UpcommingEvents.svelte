<script lang="ts">
  import type { PublicEvent } from "$lib/components/public/eventTypes";

  let {
    events = [],
    onOpenEvent = () => {},
    onViewAll = () => {},
  } = $props<{
    events: PublicEvent[];
    onOpenEvent: (event: PublicEvent) => void;
    onViewAll?: () => void;
  }>();

  function eventImage(event: PublicEvent): string {
    return event.image?.[0] ?? "";
  }
</script>

<section class="upcoming-section">
  <div class="container upcoming-container">

    <!-- Header -->
    <div class="section-head">
      <div class="head-left">
        <span class="section-label">
          <i class="fi fi-rr-calendar"></i> What's On
        </span>
        <h2>Upcoming Events</h2>
        <p>Handpicked highlights for this season</p>
      </div>
      <button type="button" class="view-all-btn" onclick={onViewAll}>
        View All Events <i class="fi fi-rr-arrow-right"></i>
      </button>
    </div>

    <!-- Cards -->
    {#if events.length === 0}
      <div class="empty-state">
        <i class="fi fi-rr-calendar-xmark"></i>
        <p>No upcoming events available right now.</p>
      </div>
    {:else}
      <div class="cards-grid">
        {#each events as event}
          <article class="event-card">
            <div
              class="card-img"
              style={`background-image:url('${eventImage(event)}')`}
            >
              <span class="cat-pill" style={`--cat:${event.color || "#1c5c6d"}`}>
                {event.category.toUpperCase()}
              </span>
              {#if event.featured}
                <span class="featured-badge">
                  <i class="fi fi-rr-star"></i> Featured
                </span>
              {/if}
              <span class="date-chip">
                <i class="fi fi-rr-calendar-day"></i>
                {event.dateRangeLabel}
              </span>
            </div>

            <div class="card-body">
              <h3>{event.title}</h3>
              <p class="card-loc">
                <i class="fi fi-rr-marker"></i> {event.location}
              </p>
              <p class="card-desc">{event.summary}</p>

              <div class="card-foot">
                {#if event.source !== "festival" && event.type !== "festival" && event.showEntryType !== false}
                  <span class="entry-tag" class:free={event.price === 0}>
                    {event.price === 0 ? "Free Entry" : `From $${event.price}`}
                  </span>
                {/if}
                <button
                  type="button"
                  class="read-btn"
                  onclick={() => onOpenEvent(event)}
                >
                  Read More
                </button>
              </div>
            </div>
          </article>
        {/each}
      </div>
    {/if}

  </div>
</section>

<style>
  /* ── Section shell ───────────────────────────────────────────────────────── */
  .upcoming-section {
    background: #fff;
    padding: 2rem 0 2.5rem;
    border-bottom: 1px solid #f1f5f9;
  }

  .upcoming-container {
    max-width: 1540px;
    margin: 0 auto;
    padding-inline: clamp(1rem, 2.5vw, 2.5rem);
  }

  /* ── Section header ──────────────────────────────────────────────────────── */
  .section-head {
    display: flex;
    align-items: flex-end;
    justify-content: space-between;
    gap: 1rem;
    margin-bottom: 1.25rem;
  }

  .head-left {
    display: flex;
    flex-direction: column;
    gap: 0.15rem;
  }

  .section-label {
    display: inline-flex;
    align-items: center;
    gap: 0.38rem;
    font-size: 0.75rem;
    font-weight: 800;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    color: #bd242b;
  }

  .section-label i {
    font-size: 0.7rem;
  }

  .section-head h2 {
    margin: 0;
    font-size: clamp(1.4rem, 2vw, 1.75rem);
    font-weight: 700;
    color: #1e293b;
    line-height: 1.2;
  }

  .section-head p {
    margin: 0;
    color: #6b7280;
    font-size: 0.9rem;
  }

  .view-all-btn {
    display: inline-flex;
    align-items: center;
    gap: 0.45rem;
    min-height: 40px;
    border-radius: 999px;
    border: 1.5px solid #bd242b;
    color: #bd242b;
    font-weight: 700;
    font-size: 0.88rem;
    padding: 0 1.1rem;
    background: #fff;
    cursor: pointer;
    white-space: nowrap;
    flex-shrink: 0;
    transition: background 0.15s, color 0.15s;
  }

  .view-all-btn:hover {
    background: #bd242b;
    color: #fff;
  }

  /* ── Empty state ─────────────────────────────────────────────────────────── */
  .empty-state {
    text-align: center;
    padding: 2.5rem 1rem;
    color: #9ca3af;
    border: 1px dashed #e5e7eb;
    border-radius: 16px;
    background: #fafafa;
  }

  .empty-state i {
    font-size: 2rem;
    display: block;
    margin-bottom: 0.5rem;
    color: #d1d5db;
  }

  .empty-state p {
    margin: 0;
    font-size: 0.9rem;
  }

  /* ── Cards grid ──────────────────────────────────────────────────────────── */
  .cards-grid {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 1rem;
  }

  /* ── Event card ──────────────────────────────────────────────────────────── */
  .event-card {
    background: #fff;
    border: 1px solid #e5e7eb;
    border-radius: 16px;
    overflow: hidden;
    display: flex;
    flex-direction: column;
    transition: box-shadow 0.18s, transform 0.18s;
  }

  .event-card:hover {
    box-shadow: 0 6px 20px rgba(0, 0, 0, 0.08);
    transform: translateY(-2px);
  }

  /* ── Card image ──────────────────────────────────────────────────────────── */
  .card-img {
    height: 180px;
    background-size: cover;
    background-position: center;
    background-color: #e2e8f0;
    position: relative;
    padding: 0.65rem;
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
  }

  .cat-pill {
    display: inline-flex;
    background: var(--cat);
    color: #fff;
    border-radius: 999px;
    font-size: 0.68rem;
    font-weight: 800;
    letter-spacing: 0.06em;
    padding: 0.22rem 0.55rem;
    align-self: flex-start;
  }

  .featured-badge {
    position: absolute;
    top: 0.65rem;
    right: 0.65rem;
    display: inline-flex;
    align-items: center;
    gap: 0.25rem;
    background: rgba(15, 23, 42, 0.6);
    color: #fbbf24;
    font-size: 0.68rem;
    font-weight: 700;
    padding: 0.22rem 0.55rem;
    border-radius: 999px;
    backdrop-filter: blur(4px);
  }

  .date-chip {
    position: absolute;
    bottom: 0.65rem;
    left: 0.65rem;
    background: rgba(15, 23, 42, 0.65);
    color: #fff;
    border-radius: 7px;
    font-size: 0.76rem;
    padding: 0.26rem 0.5rem;
    display: inline-flex;
    align-items: center;
    gap: 0.28rem;
    backdrop-filter: blur(4px);
  }

  /* ── Card body ───────────────────────────────────────────────────────────── */
  .card-body {
    padding: 0.85rem 1rem 1rem;
    display: flex;
    flex-direction: column;
    flex: 1;
  }

  .card-body h3 {
    margin: 0;
    font-size: 1.05rem;
    font-weight: 700;
    color: #1e293b;
    line-height: 1.3;
  }

  .card-loc {
    margin: 0.25rem 0 0;
    color: #1c5c6d;
    font-size: 0.82rem;
    font-weight: 600;
    display: inline-flex;
    align-items: center;
    gap: 0.28rem;
  }

  .card-desc {
    margin: 0.5rem 0 0;
    color: #64748b;
    font-size: 0.86rem;
    line-height: 1.55;
    flex: 1;
    display: -webkit-box;
    -webkit-line-clamp: 3;
    -webkit-box-orient: vertical;
    overflow: hidden;
  }

  /* ── Card footer ─────────────────────────────────────────────────────────── */
  .card-foot {
    margin-top: 0.85rem;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 0.5rem;
  }

  .entry-tag {
    font-size: 0.76rem;
    font-weight: 700;
    padding: 0.22rem 0.6rem;
    border-radius: 999px;
    background: #fee2e2;
    color: #be123c;
  }

  .entry-tag.free {
    background: #dcfce7;
    color: #15803d;
  }

  .read-btn {
    min-height: 34px;
    border-radius: 9px;
    background: #f8ce1c;
    color: #263038;
    font-size: 0.82rem;
    font-weight: 700;
    padding: 0 0.85rem;
    cursor: pointer;
    border: none;
    transition: background 0.15s;
    flex-shrink: 0;
  }

  .read-btn:hover {
    background: #e6b910;
  }

  /* ── Responsive ──────────────────────────────────────────────────────────── */
  @media (max-width: 1100px) {
    .cards-grid {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
  }

  @media (max-width: 640px) {
    .cards-grid {
      grid-template-columns: 1fr;
    }

    .section-head {
      flex-direction: column;
      align-items: flex-start;
      gap: 0.75rem;
    }

    .view-all-btn {
      width: 100%;
      justify-content: center;
    }
  }
</style>
