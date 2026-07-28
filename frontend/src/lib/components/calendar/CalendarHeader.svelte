<script lang="ts">
    import { MONTHS, BS_MONTH_NAMES, adToBS, toNepaliNumerals } from "./dateUtils";

    let {
        year,
        month,
        bsYear,
        bsMonth,
        dateMode = "AD",
        onPrev,
        onNext,
        onToday
    } = $props<{
        year: number;
        month: number;
        bsYear?: number;
        bsMonth?: number;
        dateMode?: "AD" | "BS" | "BOTH";
        onPrev: () => void;
        onNext: () => void;
        onToday: () => void;
    }>();

    const bsFirstDay = $derived(adToBS(new Date(year, month, 1)));
    const bsLabel = $derived(`${BS_MONTH_NAMES[bsFirstDay.month - 1]} ${toNepaliNumerals(bsFirstDay.year)}`);
    const adLabel = $derived(`${MONTHS[month]} ${year}`);
</script>

<div class="calendar-header animate-fade-in">
    <div class="header-left-section">
        <div class="date-display">
            <i class="fi fi-rr-calendar-day brand-icon"></i>
            {#if dateMode === "AD"}
                <h2>{MONTHS[month]} <span class="year-text">{year}</span></h2>
            {:else if dateMode === "BS"}
                <h2>{BS_MONTH_NAMES[(bsMonth ?? bsFirstDay.month) - 1]} <span class="year-text">{toNepaliNumerals(bsYear ?? bsFirstDay.year)}</span></h2>
            {:else}
                <h2>{MONTHS[month]} <span class="year-text">{year}</span> <span class="bs-badge">{bsLabel}</span></h2>
            {/if}
        </div>
    </div>

    <div class="header-right-section">
        <button class="btn-today-alt" onclick={onToday}>
            <i class="fi fi-rr-redo"></i>
            <span>Today</span>
        </button>

        <div class="navigation-controls">
            <button class="nav-arrow prev" onclick={onPrev} aria-label="Go to previous month">
                <i class="fi fi-rr-angle-small-left"></i>
            </button>
            <div class="nav-divider"></div>
            <button class="nav-arrow next" onclick={onNext} aria-label="Go to next month">
                <i class="fi fi-rr-angle-small-right"></i>
            </button>
        </div>
    </div>
</div>

<style>
    .calendar-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 1.5rem 0;
        /* border-bottom: 1px solid #f1f5f9;
        margin-bottom: 1.5rem; */
    }

    .header-left-section {
        display: flex;
        align-items: center;
    }

    .date-display {
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

    .date-display h2 {
        font-family: 'Quicksand', sans-serif;
        font-size: 1.4rem;
        font-weight: 800;
        color: var(--color-dark);
        margin: 0;
        letter-spacing: -0.3px;
        display: flex;
        align-items: baseline;
        gap: 0.45rem;
        flex-wrap: wrap;
    }

    .year-text {
        font-weight: 400;
        color: #94a3b8;
        margin-left: 2px;
    }

    .bs-badge {
        font-size: 0.78rem;
        font-weight: 700;
        color: var(--color-secondary);
        background: rgba(var(--color-secondary-rgb, 14, 165, 233), 0.08);
        border: 1px solid rgba(var(--color-secondary-rgb, 14, 165, 233), 0.15);
        padding: 3px 11px;
        border-radius: 20px;
        margin-left: 6px;
        white-space: nowrap;
        align-self: center;
    }

    .header-right-section {
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

    .navigation-controls {
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

    @media (max-width: 700px) {
        .calendar-header {
            flex-direction: row;
            align-items: center;
            gap: 0.5rem;
            padding: 0.75rem 0;
            flex-wrap: wrap;
        }

        .header-left-section {
            flex: 1;
            min-width: 0;
        }

        .date-display {
            gap: 0.55rem;
        }

        .brand-icon {
            width: 34px;
            height: 34px;
            font-size: 1.1rem;
            border-radius: 9px;
            flex-shrink: 0;
        }

        .date-display h2 {
            font-size: 1.05rem;
            letter-spacing: -0.2px;
        }

        .bs-badge {
            font-size: 0.68rem;
            padding: 2px 8px;
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

    @media (max-width: 420px) {
        .date-display h2 {
            font-size: 1rem;
        }
    }
</style>
