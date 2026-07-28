<script lang="ts">
    import { DAYS_OF_WEEK, type CalendarEvent } from "./dateUtils";
    import CalendarCell from "./CalendarCell.svelte";

    let {
        days = [],
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
        days: Date[];
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
</script>

<div class="calendar-grid-container">
    <div class="weekday-header">
        {#each DAYS_OF_WEEK as day, index}
            <div
                class="weekday"
                class:weekday-holiday={index === 6 || (index === 0 && isSundayHoliday)}
            >{day}</div>
        {/each}
    </div>

    <div class="grid">
        {#each days as date}
            <CalendarCell
                {date}
                {currentYear}
                {currentMonth}
                {currentBSYear}
                {currentBSMonth}
                {events}
                {dateMode}
                {selectedDate}
                {isSundayHoliday}
                {onEventClick}
                {onDaySelect}
            />
        {/each}
    </div>
</div>

<style>
    .calendar-grid-container {
        background: white;
        border-radius: 12px;
        overflow: visible;
        border: 1px solid #e2e8f0;
        box-shadow: 0 2px 6px rgba(0, 0, 0, 0.04);
    }

    .weekday-header {
        display: grid;
        grid-template-columns: repeat(7, 1fr);
        background: #f8fafc;
        border-bottom: 1px solid #e2e8f0;
    }

    .weekday {
        padding: 10px 6px;
        text-align: center;
        font-size: 0.78rem;
        font-weight: 700;
        color: #64748b;
        text-transform: uppercase;
        letter-spacing: 0.05em;
    }

    .weekday-holiday {
        color: #c8102e;
    }

    .grid {
        display: grid;
        grid-template-columns: repeat(7, 1fr);
    }

    @media (max-width: 700px) {
        .weekday {
            padding: 7px 2px;
            font-size: 0.65rem;
            letter-spacing: 0;
        }
    }
</style>
