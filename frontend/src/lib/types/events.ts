export type EventType = "festival" | "meeting" | "holiday" | "event";
export type EventStatus = "draft" | "published" | "archived" | "pendingapproval";

export type CreateEventHighlightPayload = {
	icon?: string;
	title: string;
	description: string;
	tone?: "orange" | "blue" | "purple" | "green" | "red" | string;
};

export type CreateEventPayload = {
	slug?: string;
	title: string;
	summary: string;
	longDescription: string;
	category: string;
	type: EventType;
	status?: EventStatus;
	date_ad: string;
	end_date_ad?: string;
	date_bs?: string;
	end_date_bs?: string;
	color?: string;
	location: string;
	region: string;
	address?: string;
	latitude?: number;
	longitude?: number;
	dateRangeLabel?: string;
	durationLabel?: string;
	attendanceLabel?: string;
	attendanceNote?: string;
	entryType?: "Free Entry" | "Paid Entry";
	showEntryType?: boolean;
	price: number;
	rating: number;
	reviewsLabel?: string;
	tags: string[];
	image: string[];
	mapImage?: string;
	organizer: string;
	organizerSubtitle?: string;
	organizerVerified: boolean;
	organizerImageUrl?: string;
	highlights: CreateEventHighlightPayload[];
	featured: boolean;
	readTime?: string;
	requiresRegistration: boolean;
	requiresInvitation: boolean;
	/** Optional per-event override for the invitation email subject. Leave blank to use the global default. */
	invitationEmailSubject?: string | null;
	/** Optional per-event override for the invitation email body (HTML). Leave blank to use the global default. */
	invitationEmailBodyHtml?: string | null;
	/** Keys of the optional self-registration fields enabled for this event's public registration form. */
	selfRegistrationFields: string[];
};

export type EventHighlightDto = {
	icon: string;
	title: string;
	description: string;
	tone: string;
};

export type EventDto = {
	id: number;
	createdById: number | null;
	slug: string;
	title: string;
	date_ad: string;
	end_date_ad: string;
	date_bs: string;
	end_date_bs: string;
	color: string;
	category: string;
	type: EventType;
	status: EventStatus;
	summary: string;
	longDescription: string;
	location: string;
	region: string;
	address: string;
	latitude: number | null;
	longitude: number | null;
	dateRangeLabel: string;
	durationLabel: string;
	attendanceLabel: string;
	attendanceNote: string;
	entryType: string;
	showEntryType: boolean;
	price: number;
	rating: number;
	reviewsLabel: string;
	tags: string[];
	image: string[];
	mapImage: string;
	organizer: string;
	organizerSubtitle: string;
	organizerVerified: boolean;
	organizerImageUrl?: string | null;
	highlights: EventHighlightDto[];
	featured: boolean;
	readTime: string;
	requiresRegistration: boolean;
	requiresInvitation: boolean;
	invitationEmailSubject: string | null;
	invitationEmailBodyHtml: string | null;
	selfRegistrationFields: string[];
};
