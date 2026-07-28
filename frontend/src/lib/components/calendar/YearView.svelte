<script lang="ts">
    import {
        getDaysInMonth,
        getDaysInBSMonthGrid,
        adToBS,
        isToday,
        isSameMonth,
        isSameBSMonth,
        isDateInEventRange,
        MONTHS,
        BS_MONTH_NAMES,
        toNepaliNumerals,
        type CalendarEvent,
    } from "./dateUtils";

    let {
        year,
        bsYear,
        events = [],
        dateMode = "AD",
        onMonthSelect = () => {},
    } = $props<{
        year: number;
        bsYear: number;
        events?: CalendarEvent[];
        dateMode?: "AD" | "BS" | "BOTH";
        onMonthSelect?: (year: number, month: number) => void;
    }>();

    const now = new Date();
    const WEEKDAYS = ["S", "M", "T", "W", "T", "F", "S"];

    // Months to render: 0–11 for AD months, 0–11 for BS months (1-indexed internally)
    const MONTH_COUNT = 12;

    function getMonthDays(monthIdx: number): Date[] {
        if (dateMode === "BS") {
            return getDaysInBSMonthGrid(bsYear, monthIdx + 1);
        }
        return getDaysInMonth(year, monthIdx);
    }

    function isInMonth(date: Date, monthIdx: number): boolean {
        if (dateMode === "BS") {
            return isSameBSMonth(date, bsYear, monthIdx + 1);
        }
        return isSameMonth(date, year, monthIdx);
    }

    function getEventsForDay(date: Date): CalendarEvent[] {
        return events.filter((e) => isDateInEventRange(date, e));
    }

    function monthLabel(monthIdx: number): string {
        if (dateMode === "BS") return BS_MONTH_NAMES[monthIdx];
        return MONTHS[monthIdx];
    }

    function isTodayAD(year: number, month: number): boolean {
        return (
            now.getFullYear() === year &&
            now.getMonth() === month
        );
    }

    function isTodayBSMonth(bsYear: number, bsMonth: number): boolean {
        const todayBS = adToBS(now);
        return todayBS.year === bsYear && todayBS.month === bsMonth;
    }

    function isCurrentMonth(monthIdx: number): boolean {
        if (dateMode === "BS") return isTodayBSMonth(bsYear, monthIdx + 1);
        return isTodayAD(year, monthIdx);
    }

    function handleMonthClick(monthIdx: number) {
        if (dateMode === "BS") {
            const days = getDaysInBSMonthGrid(bsYear, monthIdx + 1);
            const firstInMonth = days.find((d) => isSameBSMonth(d, bsYear, monthIdx + 1));
            if (firstInMonth) {
                onMonthSelect(firstInMonth.getFullYear(), firstInMonth.getMonth());
            }
        } else {
            onMonthSelect(year, monthIdx);
        }
    }

    function monthEventCounts(
        days: Date[],
        monthIdx: number,
    ): { eventCount: number; festivalCount: number } {
        let eventCount = 0;
        let festivalCount = 0;
        days
            .filter((d) => isInMonth(d, monthIdx))
            .forEach((d) => {
                getEventsForDay(d).forEach((e) => {
                    if (e.type === "festival") festivalCount++;
                    else eventCount++;
                });
            });
        return { eventCount, festivalCount };
    }

    const months = $derived(
        Array.from({ length: MONTH_COUNT }, (_, i) => {
            const days = getMonthDays(i);
            const { eventCount, festivalCount } = monthEventCounts(days, i);
            return {
                idx: i,
                label: monthLabel(i),
                days,
                isCurrentM: isCurrentMonth(i),
                eventCount,
                festivalCount,
            };
        })
    );
</script>

<div class="year-view">
    {#each months as m}
        {@const monthDays = m.days}
        <div
            class="month-card"
            class:is-current-month={m.isCurrentM}
            role="button"
            tabindex="0"
            aria-label="Go to {m.label}"
            onclick={() => handleMonthClick(m.idx)}
            onkeydown={(e) => {
                if (e.key === "Enter" || e.key === " ") {
                    e.preventDefault();
                    handleMonthClick(m.idx);
                }
            }}
        >
            <!-- Month header -->
            <div class="month-header">
                <span class="month-name">{m.label}</span>
                {#if dateMode === "BOTH"}
                    <span class="month-bs-label">
                        {BS_MONTH_NAMES[m.idx]}
                    </span>
                {/if}
                {#if m.isCurrentM}
                    <span class="current-badge">Now</span>
                {/if}
            </div>

            <!-- Weekday headers -->
            <div class="weekday-row">
                {#each WEEKDAYS as wd}
                    <div class="wd">{wd}</div>
                {/each}
            </div>

            <!-- Day cells -->
            <div class="days-grid">
                {#each monthDays as date}
                    {@const inMonth = isInMonth(date, m.idx)}
                    {@const todayFlag = isToday(date)}
                    {@const dayEvts = inMonth ? getEventsForDay(date) : []}
                    {@const bsD = adToBS(date)}
                    <div
                        class="day-cell"
                        class:out-of-month={!inMonth}
                        class:is-today={todayFlag}
                        class:has-events={dayEvts.length > 0}
                        title={dayEvts.length > 0
                            ? dayEvts.map((e) => e.title).join(", ")
                            : undefined}
                        onclick={(e) => {
                            if (inMonth) {
                                e.stopPropagation();
                                onMonthSelect(date.getFullYear(), date.getMonth());
                            }
                        }}
                        role="presentation"
                    >
                        <span class="day-num">
                            {#if dateMode === "BS"}
                                {toNepaliNumerals(bsD.day)}
                            {:else}
                                {date.getDate()}
                            {/if}
                        </span>
                        {#if dateMode === "BOTH" && inMonth}
                            <span class="bs-day-num">{toNepaliNumerals(bsD.day)}</span>
                        {/if}

                        {#if dayEvts.length > 0}
                            <div class="event-dots">
                                {#each dayEvts.slice(0, 3) as evt}
                                    <span
                                        class="dot"
                                        style:background={evt.color || "var(--color-secondary)"}
                                    ></span>
                                {/each}
                                {#if dayEvts.length > 3}
                                    <span class="dot-plus">+</span>
                                {/if}
                            </div>
                        {/if}
                    </div>
                {/each}
            </div>

            <!-- Month event summary bar -->
            {#if m.eventCount > 0 || m.festivalCount > 0}
                <div class="month-footer">
                    {#if m.eventCount > 0}
                        <span class="event-count-label">
                            <i class="fi fi-rr-calendar-day"></i>
                            {m.eventCount} event{m.eventCount !== 1 ? "s" : ""}
                        </span>
                    {/if}
                    {#if m.festivalCount > 0}
                        <span class="festival-count-label">
                            <i class="fi fi-rr-sparkles"></i>
                            {m.festivalCount} festival{m.festivalCount !== 1 ? "s" : ""}
                        </span>
                    {/if}
                </div>
            {/if}
        </div>
    {/each}
</div>

<style>
    .year-view {
        display: grid;
        grid-template-columns: repeat(4, 1fr);
        gap: 14px;
        padding: 4px 0 8px;
        animation: fadeIn 0.3s ease both;
    }

    @keyframes fadeIn {
        from { opacity: 0; transform: translateY(6px); }
        to   { opacity: 1; transform: translateY(0); }
    }

    /* ── Month card ──────────────────────────────────────────────────────── */
    .month-card {
        background: white;
        border: 1px solid #e2e8f0;
        border-radius: 16px;
        padding: 14px 12px 10px;
        display: flex;
        flex-direction: column;
        gap: 8px;
        cursor: pointer;
        transition: box-shadow 0.2s, transform 0.2s, border-color 0.2s;
        outline: none;
        position: relative;
        overflow: hidden;
    }

    .month-card::before {
        content: "";
        position: absolute;
        inset: 0;
        background: linear-gradient(135deg, rgba(248, 206, 28, 0.04) 0%, transparent 60%);
        opacity: 0;
        transition: opacity 0.2s;
        pointer-events: none;
    }

    .month-card:hover {
        box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
        transform: translateY(-2px);
        border-color: #cbd5e1;
    }

    .month-card:hover::before {
        opacity: 1;
    }

    .month-card:focus-visible {
        box-shadow: 0 0 0 3px rgba(200, 16, 46, 0.2);
        border-color: #c8102e;
    }

    .month-card.is-current-month {
        border-color: var(--color-primary, #f8ce1c);
        background: linear-gradient(135deg, rgba(248, 206, 28, 0.06) 0%, white 50%);
        box-shadow: 0 4px 16px rgba(248, 206, 28, 0.15);
    }

    /* ── Month header ────────────────────────────────────────────────────── */
    .month-header {
        display: flex;
        align-items: center;
        gap: 6px;
        padding-bottom: 6px;
        border-bottom: 1px solid #f1f5f9;
    }

    .month-name {
        font-size: 0.82rem;
        font-weight: 800;
        color: #1e293b;
        letter-spacing: 0.3px;
        text-transform: uppercase;
        flex: 1;
    }

    .is-current-month .month-name {
        color: var(--color-dark, #1a1a2e);
    }

    .month-bs-label {
        font-size: 0.66rem;
        color: var(--color-secondary, #0ea5e9);
        font-weight: 600;
        opacity: 0.8;
        white-space: nowrap;
    }

    .current-badge {
        font-size: 0.6rem;
        font-weight: 800;
        background: var(--color-primary, #f8ce1c);
        color: var(--color-dark, #1a1a2e);
        border-radius: 20px;
        padding: 2px 7px;
        text-transform: uppercase;
        letter-spacing: 0.4px;
        flex-shrink: 0;
    }

    /* ── Weekday row ─────────────────────────────────────────────────────── */
    .weekday-row {
        display: grid;
        grid-template-columns: repeat(7, 1fr);
        gap: 1px;
    }

    .wd {
        text-align: center;
        font-size: 0.6rem;
        font-weight: 700;
        color: #94a3b8;
        text-transform: uppercase;
        letter-spacing: 0.3px;
        padding: 1px 0;
    }

    /* ── Days grid ───────────────────────────────────────────────────────── */
    .days-grid {
        display: grid;
        grid-template-columns: repeat(7, 1fr);
        gap: 1px;
    }

    .day-cell {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 1px;
        padding: 2px 1px;
        border-radius: 6px;
        cursor: pointer;
        transition: background 0.15s;
        min-height: 28px;
        justify-content: center;
        overflow: hidden;
    }

    .day-cell:hover:not(.out-of-month) {
        background: #f1f5f9;
    }

    .day-cell.out-of-month {
        opacity: 0.2;
        cursor: default;
        pointer-events: none;
    }

    .day-cell.is-today {
        background: var(--color-primary, #f8ce1c);
        border-radius: 6px;
    }

    .day-cell.is-today .day-num {
        font-weight: 800;
        color: var(--color-dark, #1a1a2e);
    }

    .day-cell.has-events:not(.is-today) .day-num {
        font-weight: 700;
        color: #1e293b;
    }

    .day-num {
        font-size: 0.7rem;
        font-weight: 400;
        color: #475569;
        line-height: 1;
    }

    .bs-day-num {
        font-size: 0.54rem;
        font-weight: 500;
        color: var(--color-secondary, #0ea5e9);
        line-height: 1;
    }

    .day-cell.is-today .bs-day-num {
        color: var(--color-dark, #1a1a2e);
    }

    /* ── Event dots ──────────────────────────────────────────────────────── */
    .event-dots {
        display: flex;
        gap: 2px;
        align-items: center;
        justify-content: center;
        flex-wrap: nowrap;
    }

    .dot {
        width: 4px;
        height: 4px;
        border-radius: 50%;
        flex-shrink: 0;
    }

    .dot-plus {
        font-size: 0.5rem;
        font-weight: 800;
        color: #94a3b8;
        line-height: 1;
    }

    /* ── Month footer ────────────────────────────────────────────────────── */
    .month-footer {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding-top: 6px;
        border-top: 1px solid #f1f5f9;
        margin-top: 2px;
    }

    .event-count-label,
    .festival-count-label {
        display: flex;
        align-items: center;
        gap: 4px;
        font-size: 0.62rem;
        font-weight: 700;
        color: #94a3b8;
    }

    .event-count-label i {
        font-size: 0.65rem;
        color: var(--color-secondary, #0ea5e9);
    }

    .festival-count-label i {
        font-size: 0.65rem;
        color: var(--color-primary, #f8ce1c);
    }

    /* ── Responsive ──────────────────────────────────────────────────────── */
    @media (max-width: 1200px) {
        .year-view {
            grid-template-columns: repeat(3, 1fr);
        }
    }

    @media (max-width: 800px) {
        .year-view {
            grid-template-columns: repeat(2, 1fr);
            gap: 10px;
        }

        .month-card {
            padding: 12px 10px 8px;
            border-radius: 12px;
        }

        .day-num {
            font-size: 0.64rem;
        }
    }

    @media (max-width: 480px) {
        .year-view {
            grid-template-columns: 1fr 1fr;
            gap: 8px;
        }

        .month-card {
            padding: 10px 8px 7px;
            border-radius: 10px;
            gap: 5px;
        }

        .month-name {
            font-size: 0.72rem;
        }

        .wd {
            font-size: 0.55rem;
        }

        .day-num {
            font-size: 0.58rem;
        }

        .dot {
            width: 3px;
            height: 3px;
        }
    }
</style>
