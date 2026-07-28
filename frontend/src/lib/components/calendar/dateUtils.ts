/**
 * Calendar utilities for NTB Portal
 */

export interface CalendarEvent {
    id: string | number;
    title: string;
    date_ad: Date | string;
    end_date_ad?: Date | string;
    date_bs: string;
    category: string;
    type: 'festival' | 'meeting' | 'holiday' | 'event';
    color?: string;
    [key: string]: any;
}

// ─── Bikram Sambat (BS) Conversion ───────────────────────────────────────────

/**
 * Days per month for each BS year from 2000 to 2090.
 * Months: Baishakh, Jestha, Ashadh, Shrawan, Bhadra, Ashwin,
 *         Kartik, Mangsir, Poush, Magh, Falgun, Chaitra
 *
 * Reference: BS 2000/1/1 = AD 1943/4/14
 * Algorithm: daysPassed (1-indexed from 1943-04-13) → AD date = 1943-04-13 + daysPassed
 */
const BS_START_YEAR = 2000;

const BS_DATA: number[][] = [
    [30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31], // 2000
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2001
    [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30], // 2002
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31], // 2003
    [30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31], // 2004
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2005
    [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30], // 2006
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31], // 2007
    [31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 29, 31], // 2008
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2009
    [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30], // 2010
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31], // 2011
    [31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30], // 2012
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2013
    [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30], // 2014
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31], // 2015
    [31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30], // 2016
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2017
    [31, 32, 31, 32, 31, 30, 30, 29, 30, 29, 30, 30], // 2018
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31], // 2019
    [31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30], // 2020
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2021
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30], // 2022
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31], // 2023
    [31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30], // 2024
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2025
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31], // 2026
    [30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31], // 2027
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2028
    [31, 31, 32, 31, 32, 30, 30, 29, 30, 29, 30, 30], // 2029
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31], // 2030
    [30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31], // 2031
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2032
    [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30], // 2033
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31], // 2034
    [30, 32, 31, 32, 31, 31, 29, 30, 30, 29, 29, 31], // 2035
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2036
    [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30], // 2037
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31], // 2038
    [31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30], // 2039
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2040
    [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30], // 2041
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31], // 2042
    [31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30], // 2043
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2044
    [31, 32, 31, 32, 31, 30, 30, 29, 30, 29, 30, 30], // 2045
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31], // 2046
    [31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30], // 2047
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2048
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30], // 2049
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31], // 2050
    [31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30], // 2051
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2052
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30], // 2053
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31], // 2054
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2055
    [31, 31, 32, 31, 32, 30, 30, 29, 30, 29, 30, 30], // 2056
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31], // 2057
    [30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31], // 2058
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2059
    [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30], // 2060
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31], // 2061
    [30, 32, 31, 32, 31, 31, 29, 30, 29, 30, 29, 31], // 2062
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2063
    [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30], // 2064
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31], // 2065
    [31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 29, 31], // 2066
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2067
    [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30], // 2068
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31], // 2069
    [31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30], // 2070
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2071
    [31, 32, 31, 32, 31, 30, 30, 29, 30, 29, 30, 30], // 2072
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31], // 2073
    [31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30], // 2074
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2075
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30], // 2076
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31], // 2077
    [31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30], // 2078
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2079
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30], // 2080
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31], // 2081
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2082
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2083
    [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31], // 2084
    [30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31], // 2085
    [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30], // 2086
    [31, 31, 32, 31, 31, 31, 30, 30, 29, 30, 30, 30], // 2087
    [30, 31, 32, 32, 30, 31, 30, 30, 29, 30, 30, 30], // 2088
    [30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 30, 30], // 2089
    [30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 30, 30], // 2090
];

export const BS_MONTH_NAMES = [
    'Baishakh', 'Jestha', 'Ashadh', 'Shrawan',
    'Bhadra', 'Ashwin', 'Kartik', 'Mangsir',
    'Poush', 'Magh', 'Falgun', 'Chaitra',
];

// ─── Nepali numeral conversion ────────────────────────────────────────────────

const NEPALI_DIGITS = ['०', '१', '२', '३', '४', '५', '६', '७', '८', '९'];

/** Convert an integer or numeric string to Nepali (Devanagari) numerals. */
export function toNepaliNumerals(value: number | string): string {
    return String(value).replace(/\d/g, (d) => NEPALI_DIGITS[Number(d)]);
}

// AD 1943-04-13 is the base; BS day N maps to 1943-04-13 + N
const BS_AD_REF_MS = Date.UTC(1943, 3, 13);

export interface BSDate {
    year: number;
    month: number; // 1-indexed
    day: number;   // 1-indexed
}

export function adToBS(date: Date): BSDate {
    const utcMs = Date.UTC(date.getFullYear(), date.getMonth(), date.getDate());
    let daysPassed = Math.round((utcMs - BS_AD_REF_MS) / 86400000);

    let yi = 0;
    while (yi < BS_DATA.length) {
        const daysInYear = BS_DATA[yi].reduce((a, b) => a + b, 0);
        if (daysPassed <= daysInYear) break;
        daysPassed -= daysInYear;
        yi++;
    }

    let mi = 0;
    while (mi < 12) {
        if (daysPassed <= BS_DATA[yi][mi]) break;
        daysPassed -= BS_DATA[yi][mi];
        mi++;
    }

    return { year: BS_START_YEAR + yi, month: mi + 1, day: daysPassed };
}

export function bsToAD(year: number, month: number, day: number): Date {
    const yi = year - BS_START_YEAR;
    let daysPassed = day;

    for (let m = 0; m < month - 1; m++) {
        daysPassed += BS_DATA[yi][m];
    }
    for (let y = 0; y < yi; y++) {
        daysPassed += BS_DATA[y].reduce((a, b) => a + b, 0);
    }

    return new Date(BS_AD_REF_MS + daysPassed * 86400000);
}

export function getDaysInBSMonth(year: number, month: number): number {
    const yi = year - BS_START_YEAR;
    if (yi < 0 || yi >= BS_DATA.length) return 30;
    return BS_DATA[yi][month - 1];
}

/** Get the AD date of the 1st of a BS month. */
export function bsMonthStartAD(year: number, month: number): Date {
    return bsToAD(year, month, 1);
}

/** Build a 42-cell (6×7) grid of AD dates for a BS month. */
export function getDaysInBSMonthGrid(bsYear: number, bsMonth: number): Date[] {
    const firstDay = bsToAD(bsYear, bsMonth, 1);
    const totalDays = getDaysInBSMonth(bsYear, bsMonth);
    const lastDay = bsToAD(bsYear, bsMonth, totalDays);

    const firstDow = firstDay.getDay(); // 0 = Sunday
    const days: Date[] = [];

    // Padding from previous month
    for (let i = firstDow - 1; i >= 0; i--) {
        days.push(new Date(firstDay.getTime() - (i + 1) * 86400000));
    }

    // Days in current BS month
    for (let d = 0; d < totalDays; d++) {
        days.push(new Date(firstDay.getTime() + d * 86400000));
    }

    // Padding to complete 42 cells
    const remaining = 42 - days.length;
    for (let i = 1; i <= remaining; i++) {
        days.push(new Date(lastDay.getTime() + i * 86400000));
    }

    return days;
}

// ─── Legacy helper (now uses real BS conversion) ─────────────────────────────

export function getMockBSDate(date: Date): string {
    const bs = adToBS(date);
    return `${bs.year}-${bs.month}-${bs.day}`;
}

// ─── AD calendar helpers ──────────────────────────────────────────────────────

export const DAYS_OF_WEEK = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
export const MONTHS = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
];

export function getDaysInMonth(year: number, month: number): Date[] {
    const date = new Date(year, month, 1);
    const days: Date[] = [];

    const firstDayOfWeek = date.getDay();

    const prevMonthLastDate = new Date(year, month, 0).getDate();
    for (let i = firstDayOfWeek - 1; i >= 0; i--) {
        days.push(new Date(year, month - 1, prevMonthLastDate - i));
    }

    while (date.getMonth() === month) {
        days.push(new Date(date));
        date.setDate(date.getDate() + 1);
    }

    const remainingCells = 42 - days.length;
    for (let i = 1; i <= remainingCells; i++) {
        days.push(new Date(year, month + 1, i));
    }

    return days;
}

export function isSameDay(d1: Date, d2: Date): boolean {
    return (
        d1.getFullYear() === d2.getFullYear() &&
        d1.getMonth() === d2.getMonth() &&
        d1.getDate() === d2.getDate()
    );
}

export function isToday(date: Date): boolean {
    return isSameDay(date, new Date());
}

export function isSameMonth(date: Date, year: number, month: number): boolean {
    return date.getFullYear() === year && date.getMonth() === month;
}

export function isSameBSMonth(date: Date, bsYear: number, bsMonth: number): boolean {
    const bs = adToBS(date);
    return bs.year === bsYear && bs.month === bsMonth;
}

export function startOfLocalDay(value: Date | string | undefined): Date | null {
    if (!value) return null;

    if (value instanceof Date) {
        if (Number.isNaN(value.getTime())) return null;
        return new Date(value.getFullYear(), value.getMonth(), value.getDate());
    }

    const dateOnlyMatch = value.match(/^(\d{4})-(\d{2})-(\d{2})/);
    if (dateOnlyMatch) {
        const [, year, month, day] = dateOnlyMatch;
        return new Date(Number(year), Number(month) - 1, Number(day));
    }

    const parsed = new Date(value);
    if (Number.isNaN(parsed.getTime())) return null;
    return new Date(parsed.getFullYear(), parsed.getMonth(), parsed.getDate());
}

export function getEventDateRange(event: CalendarEvent): { start: Date; end: Date } | null {
    const start = startOfLocalDay(event.date_ad);
    const parsedEnd = startOfLocalDay(event.end_date_ad);
    if (!start) return null;

    if (!parsedEnd || parsedEnd < start) {
        return { start, end: start };
    }

    return { start, end: parsedEnd };
}

export function isDateInEventRange(date: Date, event: CalendarEvent): boolean {
    const range = getEventDateRange(event);
    if (!range) return false;

    const day = startOfLocalDay(date);
    if (!day) return false;

    return day >= range.start && day <= range.end;
}

export function eventOverlapsRange(event: CalendarEvent, start: Date | null, end: Date | null): boolean {
    const range = getEventDateRange(event);
    if (!range) return false;

    const rangeStart = start ? startOfLocalDay(start) : null;
    const rangeEnd = end ? startOfLocalDay(end) : null;

    if (rangeStart && range.end < rangeStart) return false;
    if (rangeEnd && range.start > rangeEnd) return false;
    return true;
}
