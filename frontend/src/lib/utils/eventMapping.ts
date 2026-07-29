import type { EventDto, EventType } from "$lib/types/events";
import type { FestivalDto } from "$lib/types/festivals";
import type { EventHighlight, PublicEvent } from "$lib/components/public/eventTypes";

const CATEGORY_COLORS = ["#c8102e", "#1c5c6d", "#3f515b", "#7c3aed", "#15803d", "#b45309", "#0369a1"];

export const EVENT_TYPE_COLORS = {
	event: "#1c5c6d",
	festival: "#92400e"
} as const;

export function colorForType(type: string): string {
	return type === "festival" ? EVENT_TYPE_COLORS.festival : EVENT_TYPE_COLORS.event;
}

const DEFAULT_MAP_IMAGE =
	"https://staticmap.openstreetmap.de/staticmap.php?center=27.7172,85.3240&zoom=13&size=1200x500&markers=27.7172,85.3240,red-pushpin";
const DEFAULT_IMAGE =
	"https://images.unsplash.com/photo-1516450360452-9312f5463805?auto=format&fit=crop&w=1400&q=80";

export function mapEventDtoToPublicEvent(event: EventDto): PublicEvent {
	const images = event.image?.length ? event.image : [DEFAULT_IMAGE];
	const type = normalizeEventType(event.type);

	return {
		id: event.id,
		source: "event",
		slug: event.slug,
		title: event.title,
		date_ad: event.date_ad,
		end_date_ad: event.end_date_ad,
		date_bs: event.date_bs || "",
		category: event.category || "Uncategorized",
		type,
		color: colorForType(type),
		summary: event.summary || "No summary provided.",
		longDescription: event.longDescription || event.summary || "No description provided.",
		location: event.location || event.region || "Location not specified",
		region: event.region || "Unassigned",
		address: event.address || "",
		latitude: event.latitude ?? null,
		longitude: event.longitude ?? null,
		dateRangeLabel: event.dateRangeLabel || formatDisplayDate(event.date_ad),
		durationLabel: event.durationLabel || "Duration not specified",
		attendanceLabel: event.attendanceLabel || "Attendance not specified",
		attendanceNote: event.attendanceNote || "",
		entryType: event.entryType === "Paid Entry" ? "Paid Entry" : "Free Entry",
		price: Number(event.price) || 0,
		rating: Number(event.rating) || 0,
		reviewsLabel: event.reviewsLabel || "0",
		tags: event.tags || [],
		image: images,
		mapImage: event.mapImage || DEFAULT_MAP_IMAGE,
		organizer: event.organizer || "Nepal Tourism Board (NTB)",
		organizerSubtitle: event.organizerSubtitle || "Official event organizer",
		organizerVerified: event.organizerVerified,
		organizerImageUrl: event.organizerImageUrl || undefined,
		highlights: (event.highlights || []).map(mapHighlight),
		featured: event.featured,
		popularityScore: event.popularityScore ?? 0,
		readTime: event.readTime || undefined,
		requiresRegistration: event.requiresRegistration,
		requiresInvitation: event.requiresInvitation
	};
}

export function mapFestivalDtoToPublicEvent(festival: FestivalDto): PublicEvent {
	const images = festival.image?.length ? festival.image : [DEFAULT_IMAGE];

	return {
		id: festival.id,
		source: "festival",
		slug: festival.slug,
		title: festival.title,
		date_ad: festival.date_ad,
		end_date_ad: festival.end_date_ad,
		date_bs: festival.date_bs || "",
		category: festival.category || "Festival",
		type: "festival",
		color: colorForType("festival"),
		summary: festival.summary || "No summary provided.",
		longDescription: festival.longDescription || festival.summary || "No description provided.",
		location: "Location not specified",
		region: "Unassigned",
		address: "",
		dateRangeLabel: festival.dateRangeLabel || formatDisplayDate(festival.date_ad),
		durationLabel: festival.durationLabel || "Duration not specified",
		attendanceLabel: "Attendance not specified",
		attendanceNote: "",
		entryType: "Free Entry",
		price: 0,
		rating: 0,
		reviewsLabel: "0",
		tags: [],
		image: images,
		mapImage: DEFAULT_MAP_IMAGE,
		organizer: festival.organizer || "Nepal Tourism Board (NTB)",
		organizerSubtitle: festival.organizerSubtitle || "Official festival organizer",
		organizerVerified: festival.organizerVerified,
		organizerImageUrl: festival.organizerImageUrl || undefined,
		highlights: (festival.highlights || []).map(mapHighlight),
		featured: festival.featured,
		readTime: festival.readTime || undefined,
		isHoliday: festival.isHoliday ?? false
	};
}

export function colorForCategory(category: string, index = 0): string {
	const normalized = category.toLowerCase();
	if (normalized.includes("festival")) return "#c8102e";
	if (normalized.includes("meeting")) return "#3f515b";
	if (normalized.includes("promotion")) return "#1c5c6d";
	return CATEGORY_COLORS[index % CATEGORY_COLORS.length];
}

export function slugifyCategory(value: string): string {
	return value.toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-+|-+$/g, "") || "uncategorized";
}

function mapHighlight(highlight: { icon: string; title: string; description: string; tone: string }): EventHighlight {
	return {
		icon: highlight.icon,
		title: highlight.title,
		description: highlight.description,
		tone: normalizeTone(highlight.tone)
	};
}

function normalizeEventType(type: string): EventType {
	if (type === "festival" || type === "meeting" || type === "holiday" || type === "event") return type;
	return "event";
}

function normalizeTone(tone: string | undefined): "orange" | "blue" | "purple" | "green" | "red" {
	if (tone === "blue" || tone === "purple" || tone === "green" || tone === "red") return tone;
	return "orange";
}

function formatDisplayDate(value: string | Date): string {
	const date = new Date(value);
	if (Number.isNaN(date.getTime())) return "Date not specified";
	return date.toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" });
}
