<script lang="ts">
    import {
        isToday,
        isSameMonth,
        isSameBSMonth,
        adToBS,
        toNepaliNumerals,
        isDateInEventRange,
        type CalendarEvent,
    } from "./dateUtils";

    let {
        date,
        currentYear,
        currentMonth,
        currentBSYear,
        currentBSMonth,
        events = [],
        dateMode = "AD",
        selectedDate = null,
        isSundayHoliday = true,
        onEventClick = () => {},
        onDaySelect = () => {},
    } = $props<{
        date: Date;
        currentYear: number;
        currentMonth: number;
        currentBSYear?: number;
        currentBSMonth?: number;
        events?: CalendarEvent[];
        dateMode?: "AD" | "BS" | "BOTH";
        selectedDate?: Date | null;
        isSundayHoliday?: boolean;
        onEventClick?: (event: CalendarEvent) => void;
        onDaySelect?: (date: Date) => void;
    }>();

    // Saturday is always a holiday; Sunday is holiday only when enabled by admin.
    const isWeeklyHoliday = $derived(
        date.getDay() === 6 || (date.getDay() === 0 && isSundayHoliday),
    );

    const isCurrentMonth = $derived(
        dateMode === "BS" && currentBSYear != null && currentBSMonth != null
            ? isSameBSMonth(date, currentBSYear, currentBSMonth)
            : isSameMonth(date, currentYear, currentMonth),
    );
    const isTodayDate = $derived(isToday(date));
    const bsDate = $derived(adToBS(date));
    const bsDay = $derived(bsDate.day);

    const isSelected = $derived(
        selectedDate !== null &&
            selectedDate !== undefined &&
            date.getFullYear() === selectedDate.getFullYear() &&
            date.getMonth() === selectedDate.getMonth() &&
            date.getDate() === selectedDate.getDate(),
    );

    const dayEvents = $derived(
        events.filter((event: CalendarEvent) =>
            isDateInEventRange(date, event),
        ),
    );

    const eventNamesLabel = $derived(
        dayEvents.map((e: CalendarEvent) => e.title).join(" / "),
    );

    const alignPreviewRight = $derived(date.getDay() >= 5);

    // ── Hover preview (0.5s delay) ────────────────────────────────────────
    let hoverTimer: ReturnType<typeof setTimeout> | null = null;
    let showPreview = $state(false);
    let alignPreviewTop = $state(false);
    let cellEl: HTMLElement;

    function updateAlignment() {
        const rect = cellEl?.getBoundingClientRect();
        if (rect) {
            alignPreviewTop = (window.innerHeight - rect.bottom) < 340;
        }
    }

    function handleMouseEnter() {
        hoverTimer = setTimeout(() => {
            updateAlignment();
            showPreview = true;
        }, 500);
    }

    function handleMouseLeave() {
        if (hoverTimer) {
            clearTimeout(hoverTimer);
            hoverTimer = null;
        }
        showPreview = false;
    }

    // Pin the popover open when this day is the selected/clicked day and has
    // events — stays visible until another cell is selected.
    const pinned = $derived(isSelected && dayEvents.length > 0);
    $effect(() => {
        if (pinned) updateAlignment();
    });

    const popoverVisible = $derived(showPreview || pinned);

    function handlePreviewEventClick(e: MouseEvent, event: CalendarEvent) {
        e.stopPropagation();
        if (hoverTimer) {
            clearTimeout(hoverTimer);
            hoverTimer = null;
        }
        showPreview = false;
        onEventClick(event);
    }

    // ── Icon mapping ──────────────────────────────────────────────────────
    function typeIcon(type: string): string {
        const map: Record<string, string> = {
            festival: "fi fi-rr-sparkles",
            meeting: "fi fi-rr-users-alt",
            holiday: "fi fi-rr-sun",
            event: "fi fi-rr-calendar-day",
        };
        return map[type] ?? "fi fi-rr-calendar-day";
    }

    const isHolidayDay = $derived(
        isWeeklyHoliday ||
            dayEvents.some((e: CalendarEvent) => (e as any)['isHoliday'] === true),
    );

    // ── Cell click (fires onDaySelect) ───────────────────────────────────
    function handleCellClick(e: MouseEvent) {
        // Only fire when clicking the cell background / header — not an event bar
        // (event bars have their own onclick and stopPropagation)
        onDaySelect(date);
    }

    function handleEventNamesClick(e: MouseEvent) {
        e.stopPropagation();
        if (dayEvents.length === 1) {
            onEventClick(dayEvents[0]);
        } else {
            onDaySelect(date);
        }
    }
</script>

<!-- ── Desktop cell ──────────────────────────────────────────────────────── -->
<div
    bind:this={cellEl}
    class="calendar-cell"
    class:not-current={!isCurrentMonth}
    class:today={isTodayDate}
    class:selected={isSelected}
    class:mode-both={dateMode === "BOTH"}
    class:has-events={dayEvents.length > 0}
    class:holiday={isHolidayDay}
    role="button"
    tabindex="0"
    aria-label="{date.toDateString()}, {dayEvents.length} event{dayEvents.length !==
    1
        ? 's'
        : ''}"
    onclick={handleCellClick}
    onmouseenter={handleMouseEnter}
    onmouseleave={handleMouseLeave}
    onkeydown={(e) => {
        if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            handleCellClick(e as unknown as MouseEvent);
        }
    }}
>
    <!-- ── Event name(s) row (top) ───────────────────────────────────────── -->
    <div class="cell-event-names">
        {#if dayEvents.length > 0}
            <button
                type="button"
                class="event-names-text"
                style:color={dayEvents[0].color}
                title={eventNamesLabel}
                onclick={handleEventNamesClick}
            >
                {eventNamesLabel}
            </button>
        {:else}
            <span class="event-names-text placeholder">--</span>
        {/if}
    </div>

    <!-- ── Big day number (center) ───────────────────────────────────────── -->
    <div class="cell-day-number">
        {#if dateMode === "BS"}
            <span
                class="day-number-big"
                class:holiday-number={isHolidayDay}
                title="Bikram Sambat: {bsDate.year}-{bsDate.month}-{bsDate.day}"
                >{toNepaliNumerals(bsDay)}</span
            >
        {:else}
            <span class="day-number-big" class:holiday-number={isHolidayDay}
                >{date.getDate()}</span
            >
        {/if}
    </div>

    <!-- ── Secondary calendar corner badge (bottom-right) ───────────────────── -->
    {#if dateMode === "BS"}
        <span class="corner-date" class:holiday-number={isHolidayDay} title="AD: {date.toDateString()}"
            >{date.getDate()}</span
        >
    {:else}
        <span
            class="corner-date"
            class:holiday-number={isHolidayDay}
            title="Bikram Sambat: {bsDate.year}-{bsDate.month}-{bsDate.day}"
            >{toNepaliNumerals(bsDay)}</span
        >
    {/if}

    <!-- ── Hover preview popover (2s delay) ─────────────────────────────── -->
    {#if popoverVisible}
        <div
            class="day-preview-popover"
            class:align-right={alignPreviewRight}
            class:align-top={alignPreviewTop}
            role="dialog"
            aria-label="Events on {date.toDateString()}"
        >
            <div class="preview-header">
                <span class="preview-date">{date.toLocaleDateString("en-US", { weekday: "short", month: "short", day: "numeric" })}</span>
                {#if dayEvents.length > 0}
                    <span class="preview-count">{dayEvents.length} event{dayEvents.length !== 1 ? "s" : ""}</span>
                {/if}
            </div>
            {#if dayEvents.length === 0}
                <p class="preview-empty">No events or festivals this date</p>
            {/if}
            <ul class="preview-list" role="list">
                {#each dayEvents as event (`${event['source'] ?? ''}-${event.id}`)}
                    {@const thumb = Array.isArray(event['image']) ? event['image'][0] : event['image']}
                    {@const desc = event['summary'] || event['dateRangeLabel'] || ""}
                    <li>
                        <button
                            type="button"
                            class="preview-card"
                            style="--accent:{event.color ?? '#64748b'}"
                            onclick={(e) => handlePreviewEventClick(e, event)}
                        >
                            {#if thumb}
                                <img class="preview-thumb" src={thumb} alt={event.title} />
                            {:else}
                                <span class="preview-thumb-fallback" style="background:{event.color ?? '#64748b'}20">
                                    <i class={typeIcon(event.type)} style="color:{event.color ?? '#64748b'}"></i>
                                </span>
                            {/if}
                            <div class="preview-body">
                                <span class="preview-type-badge" style="background:{event.color ?? '#64748b'}18; color:{event.color ?? '#64748b'}">
                                    <i class={typeIcon(event.type)}></i>
                                    {event.type}
                                </span>
                                <span class="preview-card-title">{event.title}</span>
                                {#if desc}
                                    <span class="preview-desc">{desc}</span>
                                {/if}
                            </div>
                        </button>
                    </li>
                {/each}
            </ul>
        </div>
    {/if}

</div>

<style>
    /* ── Base cell ────────────────────────────────────────────────────────── */
    .calendar-cell {
        height: 108px;
        min-width: 0;
        background: white;
        border: 1px solid #edf2f7;
        padding: 6px 4px;
        transition:
            background 0.15s,
            box-shadow 0.15s;
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        text-align: center;
        gap: 2px;
        position: relative;
        cursor: pointer;
        outline: none;
    }

    .calendar-cell:hover {
        background: var(--color-primary, #f8ce1c);
        z-index: 2;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.06);
    }

    .calendar-cell:focus-visible {
        box-shadow: inset 0 0 0 2px var(--color-primary, #f8ce1c);
    }

    .not-current {
        background: #f8fafc;
        opacity: 0.45;
    }

    .today {
        background: rgba(22, 163, 74, 0.28);
    }

    .today:hover {
        background: rgba(22, 163, 74, 0.36);
    }

    .today .day-number-big {
        color: #16a34a;
        font-weight: 700;
        opacity: 1;
    }

    .selected {
        background: #f8fafc !important;
        box-shadow: inset 0 0 0 2px #cbd5e1 !important;
        z-index: 3;
    }

    .selected:hover {
        background: #f8fafc !important;
    }

    .selected .day-number-big {
        color: #1e293b;
        font-weight: 800;
        opacity: 1;
    }

    /* ── Event name(s) row ─────────────────────────────────────────────────── */
    .cell-event-names {
        display: flex;
        align-items: flex-start;
        justify-content: center;
        min-height: 28px;
        width: 100%;
        flex-shrink: 0;
    }

    .event-names-text {
        font-size: 0.64rem;
        line-height: 1.3;
        font-weight: 600;
        color: #64748b;
        background: none;
        border: none;
        padding: 0 2px;
        margin: 0;
        cursor: pointer;
        width: 100%;
        overflow: hidden;
        display: -webkit-box;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
        text-align: center;
    }

    .event-names-text.placeholder {
        cursor: default;
        color: #cbd5e1;
        -webkit-line-clamp: 1;
    }

    /* ── Big day number ───────────────────────────────────────────────────── */
    .cell-day-number {
        display: flex;
        align-items: center;
        justify-content: center;
        flex: 1;
    }

    .day-number-big {
        font-size: 1.65rem;
        font-weight: 500;
        color: #334155;
        line-height: 1;
    }

    /* ── Secondary calendar corner badge ──────────────────────────────────── */
    .corner-date {
        position: absolute;
        bottom: 3px;
        right: 5px;
        font-size: 0.6rem;
        line-height: 1;
        font-weight: 600;
        color: #94a3b8;
        pointer-events: none;
        user-select: none;
    }

    @media (max-width: 700px) {
        .corner-date {
            font-size: 0.5rem;
            bottom: 2px;
            right: 3px;
        }
    }

    @media (max-width: 420px) {
        .corner-date {
            display: none;
        }
    }

    /* ── Holiday cell styles ──────────────────────────────────────────────── */
    .day-number-big.holiday-number,
    .corner-date.holiday-number {
        color: #dc2626 !important;
    }

    .day-number-big.holiday-number {
        font-weight: 700;
    }

    /* Today keeps its green background even on holidays; only the date number turns red */
    .calendar-cell.today.holiday {
        background: rgba(22, 163, 74, 0.28);
    }

    .calendar-cell.today.holiday:hover {
        background: rgba(22, 163, 74, 0.36);
    }

    /* ── Hover preview popover ────────────────────────────────────────────── */
    .day-preview-popover {
        position: absolute;
        top: calc(100% + 6px);
        left: 0;
        width: 360px;
        background: white;
        border: 1px solid #e2e8f0;
        border-radius: 14px;
        box-shadow: 0 16px 40px rgba(0, 0, 0, 0.16);
        z-index: 20;
        padding: 0.75rem;
        animation: popoverFade 0.15s ease-out both;
    }

    .day-preview-popover.align-right {
        left: auto;
        right: 0;
    }

    .day-preview-popover.align-top {
        top: auto;
        bottom: calc(100% + 6px);
    }

    @keyframes popoverFade {
        from {
            opacity: 0;
            transform: translateY(-6px) scale(0.97);
        }
        to {
            opacity: 1;
            transform: translateY(0) scale(1);
        }
    }

    .preview-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 0 0.3rem 0.5rem;
        border-bottom: 1px solid #f1f5f9;
        margin-bottom: 0.4rem;
    }

    .preview-date {
        font-size: 0.88rem;
        font-weight: 700;
        color: #1e293b;
    }

    .preview-count {
        font-size: 0.75rem;
        background: #f1f5f9;
        border-radius: 10px;
        padding: 3px 10px;
        font-weight: 700;
        color: #64748b;
    }

    .preview-empty {
        margin: 0;
        padding: 0.5rem 0.3rem 0.3rem;
        text-align: center;
        font-size: 0.85rem;
        color: #94a3b8;
    }

    .preview-list {
        list-style: none;
        margin: 0;
        padding: 0;
        display: flex;
        flex-direction: column;
        gap: 6px;
        max-height: 380px;
        overflow-y: auto;
    }

    .preview-card {
        width: 100%;
        display: flex;
        align-items: flex-start;
        gap: 0.75rem;
        border: none;
        background: transparent;
        cursor: pointer;
        text-align: left;
        padding: 0.55rem 0.5rem;
        border-radius: 10px;
        transition: background 0.12s;
        border-left: 3px solid var(--accent);
    }

    .preview-card:hover {
        background: #f8fafc;
    }

    .preview-thumb {
        width: 72px;
        height: 72px;
        border-radius: 10px;
        object-fit: cover;
        flex-shrink: 0;
    }

    .preview-thumb-fallback {
        width: 72px;
        height: 72px;
        border-radius: 10px;
        flex-shrink: 0;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 1.6rem;
    }

    .preview-body {
        flex: 1;
        min-width: 0;
        display: flex;
        flex-direction: column;
        gap: 4px;
    }

    .preview-type-badge {
        display: inline-flex;
        align-items: center;
        gap: 4px;
        font-size: 0.7rem;
        font-weight: 700;
        text-transform: uppercase;
        letter-spacing: 0.4px;
        padding: 3px 8px;
        border-radius: 4px;
        width: fit-content;
    }

    .preview-card-title {
        font-size: 0.92rem;
        font-weight: 700;
        color: #1e293b;
        line-height: 1.35;
        overflow: hidden;
        display: -webkit-box;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
    }

    .preview-desc {
        font-size: 0.78rem;
        color: #64748b;
        line-height: 1.45;
        overflow: hidden;
        display: -webkit-box;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
    }

    /* ── Responsive ───────────────────────────────────────────────────────── */
    @media (max-width: 700px) {
        .calendar-cell {
            height: 64px;
            padding: 4px 3px;
            gap: 1px;
        }

        .cell-event-names {
            min-height: 16px;
        }

        .event-names-text {
            font-size: 0.5rem;
            -webkit-line-clamp: 1;
        }

        .day-number-big {
            font-size: 1.05rem;
        }
    }

    @media (max-width: 420px) {
        .calendar-cell {
            height: 54px;
            padding: 3px 2px;
        }

        .cell-event-names {
            min-height: 12px;
        }

        .event-names-text {
            font-size: 0.42rem;
        }

        .day-number-big {
            font-size: 0.9rem;
        }
    }
</style>
