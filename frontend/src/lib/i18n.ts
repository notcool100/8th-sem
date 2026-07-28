import { browser } from '$app/environment';
import { derived, writable } from 'svelte/store';
import { toNepaliNumerals } from '$lib/components/calendar/dateUtils';

export type Locale = 'en' | 'ne';
export const SUPPORTED_LOCALES: Locale[] = ['en', 'ne'];
export const DEFAULT_LOCALE: Locale = 'en';
export const locale = writable<Locale>(DEFAULT_LOCALE);

let activeLocale: Locale = DEFAULT_LOCALE;
locale.subscribe((value) => {
  activeLocale = value ?? DEFAULT_LOCALE;
});

const TRANSLATIONS = {
  en: {
    home: 'Home',
    events: 'Events',
    searchPlaceholder: 'Search Nepal...',
    filters: 'Filters',
    resetAll: 'Reset All',
    category: 'Category',
    dateRange: 'Date Range',
    location: 'Location',
    price: 'Price',
    from: 'From',
    to: 'To',
    allPrices: 'All Prices',
    freeEntry: 'Free Entry',
    paidEvents: 'Paid Events',
    upTo: 'Up to',
    tags: 'Tags',
    travelAdvisory: 'Travel Advisory',
    viewTravelGuide: 'View Travel Guide',
    language: 'Language',
    english: 'English',
    nepali: 'नेपाली',
    destinations: 'Destinations',
    planYourTrip: 'Plan Your Trip',
  },
  ne: {
    home: 'गृहपृष्ठ',
    events: 'कार्यक्रमहरू',
    searchPlaceholder: 'नेपाल खोज्नुहोस्...',
    filters: 'फिल्टरहरू',
    resetAll: 'सबै रिसेट गर्नुहोस्',
    category: 'श्रेणी',
    dateRange: 'मिति दायरा',
    location: 'स्थान',
    price: 'मूल्य',
    from: 'देखि',
    to: 'सम्म',
    allPrices: 'सबै मूल्यहरू',
    freeEntry: 'नि: शुल्क प्रवेश',
    paidEvents: 'पैसावाला कार्यक्रम',
    upTo: 'सम्म',
    tags: 'ट्यागहरू',
    travelAdvisory: 'यात्रा सल्लाह',
    viewTravelGuide: 'यात्रा गाइड हेर्नुहोस्',
    language: 'भाषा',
    english: 'English',
    nepali: 'नेपाली',
    destinations: 'गन्तव्यहरू',
    planYourTrip: 'यात्रा योजना बनाउनुहोस्',
  },
} as const;

export const t = derived(locale, ($locale) => {
  return (
    key: keyof typeof TRANSLATIONS.en | string,
    params?: Record<string, string | number>,
  ) => {
    const translation =
      (TRANSLATIONS[$locale as Locale] as Record<string, string>)[key] ??
      (TRANSLATIONS[DEFAULT_LOCALE] as Record<string, string>)[key] ??
      key;

    if (!params) {
      return translation;
    }

    return Object.entries(params).reduce((result, [paramName, value]) => {
      return result.replaceAll(`{${paramName}}`, String(value));
    }, translation);
  };
});

export function getBrowserLocale(): Locale {
  if (!browser) return DEFAULT_LOCALE;
  const navigatorLocale = navigator.language.toLowerCase();
  if (navigatorLocale.startsWith('ne')) return 'ne';
  return 'en';
}

export function setLocale(value: Locale): void {
  if (!SUPPORTED_LOCALES.includes(value)) {
    value = DEFAULT_LOCALE;
  }

  locale.set(value);
  if (browser) {
    localStorage.setItem('ntb-locale', value);
  }
}

export function initLocale(): void {
  if (!browser) return;

  const url = new URL(location.href);
  const langParam = url.searchParams.get('lang');
  const stored = localStorage.getItem('ntb-locale') as Locale | null;
  const chosen = (langParam || stored || getBrowserLocale()) as Locale;

  if (SUPPORTED_LOCALES.includes(chosen)) {
    locale.set(chosen);
    localStorage.setItem('ntb-locale', chosen);
  } else {
    locale.set(DEFAULT_LOCALE);
  }
}

export function formatDate(value: Date | string, options?: Intl.DateTimeFormatOptions): string {
  const date = typeof value === 'string' ? new Date(value) : value;
  const localeTag = activeLocale === 'ne' ? 'ne-NP' : 'en-US';
  const formatted = new Intl.DateTimeFormat(localeTag, options).format(date);
  return activeLocale === 'ne' ? toNepaliNumerals(formatted) : formatted;
}

export function formatNumber(value: number): string {
  const formatted = new Intl.NumberFormat(activeLocale === 'ne' ? 'ne-NP' : 'en-US').format(value);
  return activeLocale === 'ne' ? toNepaliNumerals(formatted) : formatted;
}
