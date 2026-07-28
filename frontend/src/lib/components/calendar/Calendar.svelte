<script lang="ts">
    import {
        getDaysInMonth,
        getDaysInBSMonthGrid,
        adToBS,
        MONTHS,
        BS_MONTH_NAMES,
        toNepaliNumerals,
        isDateInEventRange,
        type CalendarEvent,
    } from "./dateUtils";
    import CalendarHeader from "./CalendarHeader.svelte";
    import CalendarGrid from "./CalendarGrid.svelte";
    import DateToggle from "./DateToggle.svelte";
    import DayEventsPanel from "./DayEventsPanel.svelte";
    import YearView from "./YearView.svelte";

    let {
        events = [],
        dateMode: initialDateMode = "AD",
        view: initialView = "month",
        isSundayHoliday = true,
        hideGrid = false,
        onEventClick = () => {},
        onNavigate = () => {},
    } = $props<{
        events?: CalendarEvent[];
        dateMode?: "AD" | "BS" | "BOTH";
        view?: "month" | "year";
        isSundayHoliday?: boolean;
        hideGrid?: boolean;
        onEventClick?: (event: CalendarEvent) => void;
        onNavigate?: (info: {
            view: "month" | "year";
            year: number;
            month: number;
            dateMode: "AD" | "BS" | "BOTH";
            bsYear: number;
            bsMonth: number;
        }) => void;
    }>();

    const now = new Date();
    const bsToday = adToBS(now);

    // ── Month view state ─────────────────────────────────────────────────
    let currentYear   = $state(now.getFullYear());
    let currentMonth  = $state(now.getMonth());
    let currentBSYear  = $state(bsToday.year);
    let currentBSMonth = $state(bsToday.month);

    // ── Year view state ──────────────────────────────────────────────────
    let yearViewYear   = $state(now.getFullYear());
    let yearViewBSYear = $state(bsToday.year);

    // ── Shared state ─────────────────────────────────────────────────────
    let currentView = $state<"month" | "year">(initialView === "year" ? "year" : "month");
    let dateMode    = $state<"AD" | "BS" | "BOTH">(initialDateMode);
    let selectedDate = $state<Date | null>(null);

    const isBSMode = $derived(dateMode === "BS");

    const days = $derived(
        isBSMode
            ? getDaysInBSMonthGrid(currentBSYear, currentBSMonth)
            : getDaysInMonth(currentYear, currentMonth),
    );

    function notify() {
        onNavigate({
            view: currentView,
            year: currentView === "year" ? yearViewYear : currentYear,
            month: currentMonth,
            dateMode,
            bsYear: currentView === "year" ? yearViewBSYear : currentBSYear,
            bsMonth: currentBSMonth,
        });
    }

    // ── Month view navigation ────────────────────────────────────────────
    function nextMonth() {
        if (isBSMode) {
            if (currentBSMonth === 12) { currentBSMonth = 1; currentBSYear++; }
            else currentBSMonth++;
        } else if (currentMonth === 11) { currentMonth = 0; currentYear++; }
        else currentMonth++;
        selectedDate = null;
        notify();
    }

    function prevMonth() {
        if (isBSMode) {
            if (currentBSMonth === 1) { currentBSMonth = 12; currentBSYear--; }
            else currentBSMonth--;
        } else if (currentMonth === 0) { currentMonth = 11; currentYear--; }
        else currentMonth--;
        selectedDate = null;
        notify();
    }

    function goToday() {
        currentYear    = now.getFullYear();
        currentMonth   = now.getMonth();
        currentBSYear  = bsToday.year;
        currentBSMonth = bsToday.month;
        yearViewYear   = now.getFullYear();
        yearViewBSYear = bsToday.year;
        selectedDate   = null;
        notify();
    }

    // ── Year view navigation ─────────────────────────────────────────────
    function nextYear() {
        yearViewYear++;
        yearViewBSYear++;
        notify();
    }

    function prevYear() {
        yearViewYear--;
        yearViewBSYear--;
        notify();
    }

    // ── View change ───────────────────────────────────────────────────────
    function handleViewChange(view: "month" | "year") {
        currentView = view;
        selectedDate = null;
        notify();
    }

    // ── Date mode change (AD/BS/BOTH) ────────────────────────────────────
    function handleDateModeChange(mode: "AD" | "BS" | "BOTH") {
        dateMode = mode;
        notify();
    }

    // ── Day select (month view) ──────────────────────────────────────────
    function handleDaySelect(date: Date) {
        const hasEvents = events.some((e: CalendarEvent) => isDateInEventRange(date, e));
        if (!hasEvents) {
            selectedDate = null;
            return;
        }
        if (
            selectedDate &&
            selectedDate.getFullYear() === date.getFullYear() &&
            selectedDate.getMonth()    === date.getMonth()    &&
            selectedDate.getDate()     === date.getDate()
        ) {
            selectedDate = null;
        } else {
            selectedDate = date;
        }
    }

    function closePanel() {
        selectedDate = null;
    }

    // ── Month select from YearView ────────────────────────────────────────
    function handleMonthSelect(adYear: number, adMonth: number) {
        currentYear  = adYear;
        currentMonth = adMonth;
        // Sync BS
        const bsD = adToBS(new Date(adYear, adMonth, 1));
        currentBSYear  = bsD.year;
        currentBSMonth = bsD.month;
        currentView  = "month";
        selectedDate = null;
        notify();
    }

    // ── Year label for header ─────────────────────────────────────────────
    const yearLabel = $derived(
        dateMode === "BS"
            ? toNepaliNumerals(yearViewBSYear)
            : dateMode === "BOTH"
                ? `${yearViewYear} · ${toNepaliNumerals(yearViewBSYear)}`
                : String(yearViewYear)
    );
</script>

<div class="calendar-module animate-fade-in">
    <!-- ── Top bar ──────────────────────────────────────────────────────── -->
    <div class="calendar-top-bar">
        {#if currentView === "month"}
            <CalendarHeader
                year={currentYear}
                month={currentMonth}
                bsYear={currentBSYear}
                bsMonth={currentBSMonth}
                {dateMode}
                onNext={nextMonth}
                onPrev={prevMonth}
                onToday={goToday}
            />
        {:else}
            <!-- Year view header -->
            <div class="year-header">
                <div class="year-title-wrap">
                    <i class="fi fi-rr-calendar brand-icon"></i>
                    <h2 class="year-title">{yearLabel}</h2>
                </div>
                <div class="year-nav-group">
                    <button class="btn-today-alt" onclick={goToday}>
                        <i class="fi fi-rr-redo"></i>
                        <span>Today</span>
                    </button>
                    <div class="year-nav-arrows">
                        <button class="nav-arrow" onclick={prevYear} aria-label="Previous year">
                            <i class="fi fi-rr-angle-small-left"></i>
                        </button>
                        <div class="nav-divider"></div>
                        <button class="nav-arrow" onclick={nextYear} aria-label="Next year">
                            <i class="fi fi-rr-angle-small-right"></i>
                        </button>
                    </div>
                </div>
            </div>
        {/if}

        <div class="calendar-controls">
            <div class="date-mode-selector">
                <button class:active={dateMode === "AD"}   onclick={() => handleDateModeChange("AD")}>AD</button>
                <button class:active={dateMode === "BS"}   onclick={() => handleDateModeChange("BS")}>BS</button>
            </div>
            <DateToggle view={currentView} onViewChange={handleViewChange} />
        </div>
    </div>

    <!-- ── Calendar content ───────────────────────────────────────────── -->
    {#if !hideGrid}
        {#if currentView === "month"}
            <CalendarGrid
                {days}
                {currentYear}
                {currentMonth}
                {currentBSYear}
                {currentBSMonth}
                {events}
                {dateMode}
                {selectedDate}
                {isSundayHoliday}
                {onEventClick}
                onDaySelect={handleDaySelect}
            />

            {#if selectedDate}
                <DayEventsPanel
                    date={selectedDate}
                    {events}
                    {dateMode}
                    onEventClick={(event) => {
                        closePanel();
                        onEventClick(event);
                    }}
                    onClose={closePanel}
                />
            {/if}
        {:else}
            <YearView
                year={yearViewYear}
                bsYear={yearViewBSYear}
                {events}
                {dateMode}
                onMonthSelect={handleMonthSelect}
            />
        {/if}
    {/if}
</div>

<style>
    .calendar-module {
        width: 100%;
        display: flex;
        flex-direction: column;
    }

    /* ── Top bar ──────────────────────────────────────────────────────────── */
    .calendar-top-bar {
        display: flex;
        justify-content: space-between;
        align-items: center;
      
        gap: 0.75rem;
        margin-bottom: 0.5rem;
    }

    .calendar-controls {
        display: flex;
        align-items: center;
        gap: 1rem;
    }

    /* ── Year header ──────────────────────────────────────────────────────── */
    .year-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 1rem;
        flex: 1;
        padding: 1.5rem 0;
        border-bottom: 1px solid #f1f5f9;
        margin-bottom: 1.5rem;
        flex-wrap: wrap;
    }

    .year-title-wrap {
        display: flex;
        align-items: center;
        gap: 1rem;
    }

    .brand-icon {
        font-size: 1.25rem;
        color: var(--color-primary);
        background: rgba(248, 206, 28, 0.1);
        width: 38px;
        height: 38px;
        display: flex;
        align-items: center;
        justify-content: center;
        border-radius: 11px;
        flex-shrink: 0;
    }

    .year-title {
        font-family: 'Quicksand', sans-serif;
        font-size: 1.4rem;
        font-weight: 800;
        color: var(--color-dark);
        margin: 0;
        letter-spacing: -0.3px;
    }

    .year-nav-group {
        display: flex;
        align-items: center;
        gap: 1rem;
    }

    .btn-today-alt {
        display: flex;
        align-items: center;
        gap: 8px;
        padding: 10px 18px;
        background: white;
        border: 1px solid #e2e8f0;
        border-radius: 12px;
        font-weight: 700;
        font-size: 0.9rem;
        color: #475569;
        cursor: pointer;
        transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
        box-shadow: 0 2px 4px rgba(0,0,0,0.02);
    }

    .btn-today-alt:hover {
        background: #f8fafc;
        border-color: var(--color-primary);
        color: var(--color-dark);
        transform: translateY(-1px);
        box-shadow: 0 4px 12px rgba(0,0,0,0.05);
    }

    .year-nav-arrows {
        display: flex;
        align-items: center;
        background: white;
        border: 1px solid #e2e8f0;
        border-radius: 12px;
        padding: 4px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.02);
    }

    .nav-arrow {
        width: 36px;
        height: 36px;
        display: flex;
        align-items: center;
        justify-content: center;
        background: transparent;
        border: none;
        color: #64748b;
        cursor: pointer;
        border-radius: 8px;
        transition: all 0.2s;
        font-size: 1.1rem;
    }

    .nav-arrow:hover {
        background: #f1f5f9;
        color: var(--color-primary);
    }

    .nav-divider {
        width: 1px;
        height: 20px;
        background: #e2e8f0;
        margin: 0 4px;
    }

    /* ── Date mode selector ───────────────────────────────────────────────── */
    .date-mode-selector {
        display: flex;
        background: #f1f5f9;
        padding: 3px;
        border-radius: 10px;
        border: 1px solid #e2e8f0;
    }

    .date-mode-selector button {
        padding: 5px 11px;
        border-radius: 7px;
        font-size: 0.78rem;
        font-weight: 700;
        color: #64748b;
        cursor: pointer;
        border: none;
        background: transparent;
        transition: all 0.18s;
        text-transform: uppercase;
        letter-spacing: 0.4px;
    }

    .date-mode-selector button.active {
        background: white;
        color: var(--color-dark);
        box-shadow: 0 1px 4px rgba(0, 0, 0, 0.07);
    }

    .date-mode-selector button:hover:not(.active) {
        color: var(--color-secondary);
    }

    /* ── Responsive ───────────────────────────────────────────────────────── */
    @media (max-width: 700px) {
        .calendar-top-bar {
            flex-direction: column;
            align-items: flex-start;
            gap: 0.6rem;
        }

        .calendar-controls {
            width: 100%;
            justify-content: space-between;
        }

        .year-header {
            padding: 0.75rem 0;
            margin-bottom: 0.75rem;
            gap: 0.5rem;
        }

        .year-title {
            font-size: 1.1rem;
        }

        .btn-today-alt span {
            display: none;
        }

        .btn-today-alt {
            padding: 8px 10px;
            gap: 0;
            min-width: 36px;
        }

        .nav-arrow {
            width: 30px;
            height: 30px;
        }
    }
</style>
