<script lang="ts">
    import {
        adToBS,
        BS_MONTH_NAMES,
        MONTHS,
        isDateInEventRange,
        type CalendarEvent,
    } from "./dateUtils";

    let {
        date,
        events = [],
        dateMode = "AD",
        onEventClick = () => {},
        onClose = () => {},
    } = $props<{
        date: Date;
        events: CalendarEvent[];
        dateMode?: "AD" | "BS" | "BOTH";
        onEventClick: (event: CalendarEvent) => void;
        onClose: () => void;
    }>();

    const dayEvents = $derived(
        events.filter((e: CalendarEvent) => isDateInEventRange(date, e)),
    );

    const bsDate = $derived(adToBS(date));

    const adLabel = $derived(
        date.toLocaleDateString("en-US", {
            weekday: "long",
            month: "long",
            day: "numeric",
            year: "numeric",
        }),
    );

    const bsLabel = $derived(
        `${BS_MONTH_NAMES[bsDate.month - 1]} ${bsDate.day}, ${bsDate.year}`,
    );

    function typeIcon(type: string): string {
        const map: Record<string, string> = {
            festival: "fi fi-rr-sparkles",
            meeting:  "fi fi-rr-users-alt",
            holiday:  "fi fi-rr-sun",
            event:    "fi fi-rr-calendar-day",
        };
        return map[type] ?? "fi fi-rr-calendar-day";
    }
</script>

<div class="day-panel" role="region" aria-label="Events for {adLabel}">
    <div class="panel-header">
        <div class="panel-date-block">
            {#if dateMode === "BS"}
                <p class="date-main">{bsLabel}</p>
            {:else if dateMode === "BOTH"}
                <p class="date-main">{adLabel}</p>
                <p class="date-sub">{bsLabel} (BS)</p>
            {:else}
                <p class="date-main">{adLabel}</p>
            {/if}
        </div>

        <div class="panel-actions">
            <span class="event-count-chip">
                {dayEvents.length} event{dayEvents.length !== 1 ? "s" : ""}
            </span>
            <button
                type="button"
                class="close-btn"
                onclick={onClose}
                aria-label="Close day panel"
            >
                <i class="fi fi-rr-cross-small"></i>
            </button>
        </div>
    </div>

    {#if dayEvents.length === 0}
        <div class="empty-day">
            <i class="fi fi-rr-calendar-xmark"></i>
            <span>No events on this day</span>
        </div>
    {:else}
        <ul class="events-list" role="list">
            {#each dayEvents as event (`${event['source'] ?? ''}-${event.id}`)}
                <li>
                    <button
                        type="button"
                        class="event-row"
                        onclick={() => onEventClick(event)}
                    >
                        <span
                            class="icon-wrap"
                            style="background:{event.color ?? '#94a3b8'}18; color:{event.color ?? '#94a3b8'}"
                        >
                            <i class={typeIcon(event.type)}></i>
                        </span>

                        <div class="event-body">
                            <span class="event-name">{event.title}</span>
                            {#if (event as any).location}
                                <span class="event-loc">
                                    <i class="fi fi-rr-marker"></i>
                                    {(event as any).location}
                                </span>
                            {/if}
                        </div>

                        {#if event.category}
                            <span
                                class="cat-badge"
                                style="background:{event.color ?? '#94a3b8'}14; color:{event.color ?? '#64748b'}"
                            >{event.category}</span>
                        {/if}

                        <i class="fi fi-rr-angle-small-right chevron"></i>
                    </button>
                </li>
            {/each}
        </ul>
    {/if}
</div>

<style>
    /* ── Container ────────────────────────────────────────────────────────── */
    .day-panel {
        background: white;
        border: 1px solid #e5e7eb;
        border-top: 3px solid var(--color-primary, #f8ce1c);
        border-radius: 0 0 16px 16px;
        overflow: hidden;
        animation: slideDown 0.22s cubic-bezier(0.4, 0, 0.2, 1) both;
    }

    @keyframes slideDown {
        from {
            opacity: 0;
            transform: translateY(-10px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    /* ── Header ───────────────────────────────────────────────────────────── */
    .panel-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 0.75rem;
        padding: 0.9rem 1.1rem 0.8rem;
        border-bottom: 1px solid #f1f5f9;
        background: #fafbfc;
    }

    .panel-date-block {
        display: flex;
        flex-direction: column;
        gap: 0.1rem;
        min-width: 0;
    }

    .date-main {
        margin: 0;
        font-size: 0.97rem;
        font-weight: 700;
        color: #1e293b;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    .date-sub {
        margin: 0;
        font-size: 0.78rem;
        color: #64748b;
    }

    .panel-actions {
        display: flex;
        align-items: center;
        gap: 0.65rem;
        flex-shrink: 0;
    }

    .event-count-chip {
        font-size: 0.75rem;
        font-weight: 700;
        color: #64748b;
        background: #f1f5f9;
        border: 1px solid #e2e8f0;
        border-radius: 20px;
        padding: 2px 10px;
        white-space: nowrap;
    }

    .close-btn {
        width: 30px;
        height: 30px;
        border-radius: 8px;
        border: 1px solid #e2e8f0;
        background: white;
        color: #64748b;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        font-size: 1rem;
        cursor: pointer;
        transition: all 0.15s;
        flex-shrink: 0;
    }

    .close-btn:hover {
        background: #f8fafc;
        border-color: #c8102e;
        color: #c8102e;
    }

    /* ── Empty state ──────────────────────────────────────────────────────── */
    .empty-day {
        display: flex;
        align-items: center;
        gap: 0.6rem;
        padding: 1.1rem 1.1rem;
        color: #94a3b8;
        font-size: 0.9rem;
    }

    .empty-day i {
        font-size: 1.1rem;
    }

    /* ── Event list ───────────────────────────────────────────────────────── */
    .events-list {
        list-style: none;
        margin: 0;
        padding: 0.45rem 0.65rem;
        display: flex;
        flex-direction: column;
        gap: 0.3rem;
    }

    .event-row {
        width: 100%;
        border: 1px solid #f1f5f9;
        border-radius: 11px;
        background: white;
        display: grid;
        grid-template-columns: 38px 1fr auto auto;
        align-items: center;
        gap: 0.6rem;
        padding: 0.55rem 0.6rem 0.55rem 0.5rem;
        text-align: left;
        cursor: pointer;
        transition: all 0.15s;
    }

    .event-row:hover {
        background: #f8fafc;
        border-color: #e2e8f0;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
    }

    /* ── Icon wrap ────────────────────────────────────────────────────────── */
    .icon-wrap {
        width: 36px;
        height: 36px;
        border-radius: 10px;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 0.9rem;
        flex-shrink: 0;
    }

    /* ── Event body ───────────────────────────────────────────────────────── */
    .event-body {
        display: flex;
        flex-direction: column;
        gap: 0.12rem;
        min-width: 0;
    }

    .event-name {
        font-size: 0.9rem;
        font-weight: 700;
        color: #1e293b;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    .event-loc {
        font-size: 0.74rem;
        color: #94a3b8;
        display: inline-flex;
        align-items: center;
        gap: 0.22rem;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    .event-loc i {
        font-size: 0.68rem;
        flex-shrink: 0;
    }

    /* ── Category badge ───────────────────────────────────────────────────── */
    .cat-badge {
        font-size: 0.7rem;
        font-weight: 700;
        border-radius: 6px;
        padding: 2px 7px;
        white-space: nowrap;
        flex-shrink: 0;
    }

    /* ── Chevron ──────────────────────────────────────────────────────────── */
    .chevron {
        color: #cbd5e1;
        font-size: 0.9rem;
        flex-shrink: 0;
        transition: transform 0.15s;
    }

    .event-row:hover .chevron {
        transform: translateX(3px);
        color: #94a3b8;
    }

    /* ── Responsive ───────────────────────────────────────────────────────── */
    @media (max-width: 640px) {
        .panel-header {
            padding: 0.75rem 0.9rem 0.7rem;
        }

        .date-main {
            font-size: 0.88rem;
        }

        .cat-badge {
            display: none;
        }

        .event-row {
            grid-template-columns: 34px 1fr auto;
        }
    }
</style>
