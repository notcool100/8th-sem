<script lang="ts">
  import type { PublicEvent } from "$lib/components/public/eventTypes";

  let {
    events = [],
    onOpenEvent = () => {},
  } = $props<{
    events: PublicEvent[];
    onOpenEvent: (event: PublicEvent) => void;
  }>();

  function eventImage(event: PublicEvent): string {
    return event.image?.[0] ?? "";
  }
</script>

<div class="featured-head">
  <div>
    <h2>Featured Events</h2>
    <p>Handpicked highlights for this season</p>
  </div>
  <button type="button">View All Events <i class="fi fi-rr-arrow-right"></i></button>
</div>

<div class="featured-grid">
  {#if events.length === 0}
    <div class="empty-state">No featured events match the current filters.</div>
  {:else}
    {#each events as event}
      <article class="event-card">
        <div class="event-image" style={`background-image:url('${eventImage(event)}');`}>
          <span class="type" style={`--type-color:${event.color || "#1c5c6d"}`}>
            {event.category.toUpperCase()}
          </span>
          <span class="date"><i class="fi fi-rr-calendar-day"></i> {event.dateRangeLabel}</span>
        </div>
        <div class="event-body">
          <h3>{event.title}</h3>
          <p class="location"><i class="fi fi-rr-marker"></i> {event.location}</p>
          <p class="description">{event.summary}</p>
          <div class="card-foot">
            <button type="button" class="details-btn" onclick={() => onOpenEvent(event)}>
              Read More
            </button>
          </div>
        </div>
      </article>
    {/each}
  {/if}
</div>

<style>
  .featured-head {
    margin-top: 1.45rem;
    display: flex;
    align-items: flex-end;
    justify-content: space-between;
    gap: 0.8rem;
  }

  .featured-head h2 {
    margin: 0;
    font-size: clamp(1.9rem, 2.3vw, 2.3rem);
    color: #3f515b;
    font-weight: 700;
  }

  .featured-head p {
    margin-top: 0.2rem;
    color: #6b7280;
    font-size: 1rem;
  }

  .featured-head button {
    min-height: 43px;
    border-radius: 999px;
    border: 1px solid #bd242b;
    color: #bd242b;
    font-weight: 700;
    padding: 0 1.1rem;
    display: inline-flex;
    align-items: center;
    gap: 0.45rem;
    background: #fff;
  }

  .featured-grid {
    margin-top: 0.85rem;
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 1rem;
  }

  .empty-state {
    grid-column: 1 / -1;
    background: #fff;
    border: 1px dashed #d1d5db;
    border-radius: 14px;
    padding: 1.4rem;
    color: #6b7280;
    font-weight: 600;
  }

  .event-card {
    background: white;
    border-radius: 16px;
    border: 1px solid #e5e7eb;
    overflow: hidden;
    display: flex;
    flex-direction: column;
  }

  .event-image {
    min-height: 190px;
    background-position: center;
    background-size: cover;
    background-repeat: no-repeat;
    position: relative;
    padding: 0.7rem;
  }

  .type {
    display: inline-flex;
    background: var(--type-color);
    color: #fff;
    border-radius: 999px;
    font-size: 0.72rem;
    font-weight: 700;
    letter-spacing: 0.06em;
    padding: 0.28rem 0.58rem;
  }

  .date {
    position: absolute;
    left: 0.7rem;
    bottom: 0.7rem;
    background: rgba(15, 23, 42, 0.68);
    color: white;
    border-radius: 8px;
    font-size: 0.8rem;
    padding: 0.3rem 0.52rem;
    display: inline-flex;
    align-items: center;
    gap: 0.32rem;
  }

  .event-body {
    padding: 0.9rem;
  }

  .event-body h3 {
    margin: 0;
    font-size: 1.5rem;
    color: #3f515b;
    font-weight: 700;
  }

  .location {
    margin-top: 0.28rem;
    color: #1c5c6d;
    font-size: 0.9rem;
    display: inline-flex;
    align-items: center;
    gap: 0.3rem;
    font-weight: 600;
  }

  .description {
    margin-top: 0.6rem;
    color: #6b7280;
    font-size: 0.93rem;
    line-height: 1.5;
    min-height: 84px;
  }

  .card-foot {
    margin-top: 0.6rem;
    display: flex;
    align-items: center;
    justify-content: flex-end;
    gap: 0.7rem;
  }

  .details-btn {
    min-height: 38px;
    border-radius: 10px;
    background: #f8ce1c;
    color: #263038;
    font-size: 0.9rem;
    font-weight: 700;
    padding: 0 0.85rem;
  }

  @media (max-width: 1180px) {
    .featured-grid {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
  }

  @media (max-width: 820px) {
    .featured-grid {
      grid-template-columns: 1fr;
    }
  }
</style>
