import type { CalendarEvent } from "$lib/components/calendar/dateUtils";

export type PriceFilter = "all" | "free" | "paid";

export interface EventHighlight {
  icon: string;
  title: string;
  description: string;
  tone?: "orange" | "blue" | "purple" | "green" | "red";
}

export interface PublicEvent extends CalendarEvent {
  /** Which backend table this record came from — distinct from `type`, which is a thematic category that can itself be "festival" for an Event. */
  source?: "event" | "festival";
  slug: string;
  summary: string;
  longDescription: string;
  location: string;
  region: string;
  address: string;
  latitude?: number | null;
  longitude?: number | null;
  dateRangeLabel: string;
  durationLabel: string;
  attendanceLabel: string;
  attendanceNote: string;
  entryType: "Free Entry" | "Paid Entry";
  showEntryType?: boolean;
  price: number;
  rating: number;
  reviewsLabel: string;
  tags: string[];
  image: string[];
  mapImage: string;
  organizer: string;
  organizerSubtitle: string;
  organizerVerified: boolean;
  organizerImageUrl?: string;
  highlights: EventHighlight[];
  featured?: boolean;
  readTime?: string;
  requiresRegistration?: boolean;
  requiresInvitation?: boolean;
  isHoliday?: boolean;
}

export interface CategoryFilterOption {
  id: string;
  label: string;
  count: number;
  color: string;
  checked: boolean;
  isAll?: boolean;
}
