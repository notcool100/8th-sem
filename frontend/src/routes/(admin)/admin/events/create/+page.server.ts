import { error, fail, redirect } from "@sveltejs/kit";
import type { Actions, PageServerLoad } from "./$types";
import { BackendApiError, createEvent, fetchCategories, getEventById, updateEvent } from "$lib/server/auth/api";
import { can } from "$lib/utils/permissions";
import type { CreateEventHighlightPayload, CreateEventPayload, EventDto, EventStatus, EventType } from "$lib/types/events";

type EventCreateFormValues = {
	slug: string;
	title: string;
	summary: string;
	longDescription: string;
	category: string;
	type: EventType;
	status: EventStatus;
	dateAd: string;
	endDateAd: string;
	dateBs: string;
	endDateBs: string;
	color: string;
	location: string;
	region: string;
	address: string;
	latitude: number | null;
	longitude: number | null;
	dateRangeLabel: string;
	durationLabel: string;
	attendanceLabel: string;
	attendanceNote: string;
	entryType: "Free Entry" | "Paid Entry";
	price: string;
	tags: string;
	imageUrls: string;
	mapImage: string;
	organizer: string;
	organizerSubtitle: string;
	organizerVerified: boolean;
	organizerImageUrl: string;
	featured: boolean;
	readTime: string;
	highlights: {
		icon: string;
		title: string;
		description: string;
		tone: string;
	}[];
	requiresRegistration: boolean;
	requiresInvitation: boolean;
	showEntryType: boolean;
	invitationEmailSubject: string;
	invitationEmailBodyHtml: string;
	selfRegistrationFields: string[];
};

const DEFAULT_VALUES: EventCreateFormValues = {
	slug: "",
	title: "",
	summary: "",
	longDescription: "",
	category: "Festival",
	type: "event",
	status: "draft",
	dateAd: "",
	endDateAd: "",
	dateBs: "",
	endDateBs: "",
	color: "",
	location: "",
	region: "",
	address: "",
	latitude: null,
	longitude: null,
	dateRangeLabel: "",
	durationLabel: "",
	attendanceLabel: "",
	attendanceNote: "",
	entryType: "Free Entry",
	price: "0",
	tags: "",
	imageUrls: "",
	mapImage: "",
	organizer: "Nepal Tourism Board (NTB)",
	organizerSubtitle: "Official Nepal government tourism authority",
	organizerVerified: true,
	organizerImageUrl: "",
	featured: false,
	readTime: "",
	highlights: [{ icon: "fi fi-rr-star", title: "", description: "", tone: "orange" }],
	requiresRegistration: false,
	requiresInvitation: false,
	showEntryType: true,
	invitationEmailSubject: "",
	invitationEmailBodyHtml: "",
	selfRegistrationFields: []
};

export const actions: Actions = {
	create: async ({ request, locals }) => {
		if (!can(locals.session?.user, "events", "canCreate")) {
			return fail(403, { message: "You don't have permission to create events." });
		}
		return saveEvent({ request, locals });
	},
	update: async ({ request, locals, url }) => {
		if (!can(locals.session?.user, "events", "canUpdate")) {
			return fail(403, { message: "You don't have permission to update events." });
		}
		const eventId = Number(url.searchParams.get("edit"));
		if (!Number.isInteger(eventId) || eventId <= 0) {
			return fail(400, { message: "Valid event ID is required." });
		}

		return saveEvent({ request, locals, eventId });
	}
};

async function saveEvent({
	request,
	locals,
	eventId
}: {
	request: Request;
	locals: App.Locals;
	eventId?: number;
}) {
		if (!locals.session) {
			throw redirect(303, "/login");
		}

		const formData = await request.formData();
		const values = readValues(formData);

		// Default location to region since location input was removed from UI
		values.location = values.region;

		const requiredFields = [
			{ key: "title", label: "Title" },
			{ key: "summary", label: "Summary" },
			{ key: "longDescription", label: "Long description" },
			{ key: "category", label: "Category" },
			{ key: "dateAd", label: "Start date" },
			{ key: "region", label: "Region" },
			{ key: "organizer", label: "Organizer" }
		] as const;

		const missingField = requiredFields.find(({ key }) => !values[key]);
		if (missingField) {
			return fail(400, {
				message: `${missingField.label} is required.`,
				values
			});
		}

		if (values.requiresRegistration && values.requiresInvitation) {
			return fail(400, {
				message: "An event cannot require both self-registration and invitation-only access.",
				values
			});
		}

		if (values.endDateAd && values.endDateAd < values.dateAd) {
			return fail(400, {
				message: "End date cannot be earlier than the start date.",
				values
			});
		}

		const highlights = buildHighlights(values.highlights);
		const payload: CreateEventPayload = {
			slug: values.slug || undefined,
			title: values.title,
			summary: values.summary,
			longDescription: values.longDescription,
			category: values.category,
			type: values.type,
			status: values.status,
			date_ad: values.dateAd,
			end_date_ad: values.endDateAd || undefined,
			date_bs: values.dateBs || undefined,
			end_date_bs: values.endDateBs || undefined,
			color: values.color || undefined,
			location: values.location,
			region: values.region,
			address: values.address || undefined,
			latitude: values.latitude ?? undefined,
			longitude: values.longitude ?? undefined,
			dateRangeLabel: values.dateRangeLabel || undefined,
			durationLabel: values.durationLabel || undefined,
			attendanceLabel: values.attendanceLabel || undefined,
			attendanceNote: values.attendanceNote || undefined,
			entryType: values.entryType,
			price: toNumber(values.price),
			rating: 0,
			tags: parseDelimited(values.tags),
			image: parseDelimited(values.imageUrls),
			mapImage: values.mapImage || undefined,
			organizer: values.organizer,
			organizerSubtitle: values.organizerSubtitle || undefined,
			organizerVerified: values.organizerVerified,
			organizerImageUrl: values.organizerImageUrl || undefined,
			highlights,
			featured: values.featured,
			readTime: values.readTime || undefined,
			requiresRegistration: values.requiresRegistration,
			requiresInvitation: values.requiresInvitation,
			showEntryType: values.showEntryType,
			invitationEmailSubject: values.invitationEmailSubject || undefined,
			invitationEmailBodyHtml: values.invitationEmailBodyHtml || undefined,
			selfRegistrationFields: values.selfRegistrationFields
		};

		try {
			if (eventId) {
				const result = await updateEvent(locals.session.accessToken, eventId, payload);
				if (result.requiresApproval) {
					return { success: true, requiresApproval: true, message: "Event update submitted for approval. An admin will review it shortly." };
				}
			} else {
				const result = await createEvent(locals.session.accessToken, payload);
				if (result.requiresApproval) {
					return { success: true, requiresApproval: true, message: "Event submitted for approval. An admin will review it shortly." };
				}
			}
		} catch (requestError) {
			if (requestError instanceof BackendApiError) {
				return fail(requestError.status || 400, {
					message: requestError.message,
					values
				});
			}

			return fail(500, {
				message: `Unable to ${eventId ? "update" : "create"} the event right now.`,
				values
			});
		}

		throw redirect(303, "/admin/events");
}

function readValues(formData: FormData): EventCreateFormValues {
	const highlightIcons = formData.getAll("highlightIcon").map((value) => value.toString().trim());
	const highlightTitles = formData.getAll("highlightTitle").map((value) => value.toString().trim());
	const highlightDescriptions = formData
		.getAll("highlightDescription")
		.map((value) => value.toString().trim());
	const highlightTones = formData.getAll("highlightTone").map((value) => value.toString().trim());

	const maxHighlights = Math.max(
		highlightIcons.length,
		highlightTitles.length,
		highlightDescriptions.length,
		highlightTones.length,
		1
	);

	const highlights = Array.from({ length: maxHighlights }, (_, index) => ({
		icon: highlightIcons[index] ?? "",
		title: highlightTitles[index] ?? "",
		description: highlightDescriptions[index] ?? "",
		tone: normalizeTone(highlightTones[index] ?? "orange")
	}));

	return {
		slug: getValue(formData, "slug"),
		title: getValue(formData, "title"),
		summary: getValue(formData, "summary"),
		longDescription: getValue(formData, "longDescription"),
		category: getValue(formData, "category"),
		type: normalizeType(getValue(formData, "type")),
		status: normalizeStatus(getValue(formData, "status")),
		dateAd: getValue(formData, "dateAd"),
		endDateAd: getValue(formData, "endDateAd"),
		dateBs: getValue(formData, "dateBs"),
		endDateBs: getValue(formData, "endDateBs"),
		color: getValue(formData, "color"),
		location: getValue(formData, "location"),
		region: getValue(formData, "region"),
		address: getValue(formData, "address"),
		latitude: parseNullableNumber(getValue(formData, "latitude")),
		longitude: parseNullableNumber(getValue(formData, "longitude")),
		dateRangeLabel: getValue(formData, "dateRangeLabel"),
		durationLabel: getValue(formData, "durationLabel"),
		attendanceLabel: getValue(formData, "attendanceLabel"),
		attendanceNote: getValue(formData, "attendanceNote"),
		entryType: normalizeEntryType(getValue(formData, "entryType")),
		price: getValue(formData, "price") || "0",
		tags: getValue(formData, "tags"),
		imageUrls: getValue(formData, "imageUrls"),
		mapImage: getValue(formData, "mapImage"),
		organizer: getValue(formData, "organizer"),
		organizerSubtitle: getValue(formData, "organizerSubtitle"),
		organizerVerified: formData.get("organizerVerified") === "on",
		organizerImageUrl: getValue(formData, "organizerImageUrl"),
		featured: formData.get("featured") === "on",
		readTime: getValue(formData, "readTime"),
		highlights,
		requiresRegistration: formData.get("requiresRegistration") === "on",
		requiresInvitation: formData.get("requiresInvitation") === "on",
		showEntryType: formData.get("showEntryType") === "on",
		invitationEmailSubject: getValue(formData, "invitationEmailSubject"),
		invitationEmailBodyHtml: getValue(formData, "invitationEmailBodyHtml"),
		selfRegistrationFields: formData.getAll("selfRegistrationFields").map((value) => value.toString())
	};
}

function buildHighlights(highlights: EventCreateFormValues["highlights"]): CreateEventHighlightPayload[] {
	return highlights
		.filter((highlight) => highlight.title && highlight.description)
		.map((highlight) => ({
			icon: highlight.icon || undefined,
			title: highlight.title,
			description: highlight.description,
			tone: highlight.tone || undefined
		}));
}

function parseDelimited(rawValue: string): string[] {
	if (!rawValue) {
		return [];
	}

	return rawValue
		.split(/\r?\n|,/)
		.map((value) => value.trim())
		.filter(Boolean);
}

function toNumber(rawValue: string): number {
	const parsed = Number(rawValue);
	if (!Number.isFinite(parsed)) {
		return 0;
	}

	return parsed;
}

function getValue(formData: FormData, key: string): string {
	return formData.get(key)?.toString().trim() ?? "";
}

function parseNullableNumber(rawValue: string): number | null {
	if (!rawValue) return null;
	const parsed = Number(rawValue);
	return Number.isFinite(parsed) ? parsed : null;
}

function normalizeType(rawValue: string): EventType {
	const value = rawValue.toLowerCase();
	if (value === "festival" || value === "meeting" || value === "holiday" || value === "event") {
		return value;
	}

	return "event";
}

function normalizeStatus(rawValue: string): EventStatus {
	const value = rawValue.toLowerCase();
	if (value === "draft" || value === "published" || value === "archived") {
		return value;
	}

	return "draft";
}

function normalizeTone(rawValue: string): string {
	const value = rawValue.toLowerCase();
	if (value === "orange" || value === "blue" || value === "purple" || value === "green" || value === "red") {
		return value;
	}

	return "orange";
}

function normalizeEntryType(rawValue: string): "Free Entry" | "Paid Entry" {
	return rawValue === "Paid Entry" ? "Paid Entry" : "Free Entry";
}

export const prerender = false;

export const load: PageServerLoad = async ({ locals, url }) => {
	if (!locals.session) throw redirect(303, "/login");

	const editId = url.searchParams.get("edit");
	const isEditing = !!editId;
	const requiredOp = isEditing ? "canUpdate" : "canCreate";

	if (!can(locals.session.user, "events", requiredOp)) {
		throw error(403, `You don't have permission to ${isEditing ? "edit" : "create"} events.`);
	}

	let categories: any[] = [];
	const eventId = editId ? Number(editId) : null;

	try {
		categories = await fetchCategories(locals.session.accessToken);
	} catch (e) {
		console.error("Failed to load categories in events create load:", e);
	}

	const needsApproval = locals.session.user.permissions.some(
		p => p.navItemKey === "events" && p.needsApproval
	);

	if (eventId) {
		try {
			const event = await getEventById(locals.session.accessToken, eventId);
			return {
				defaults: mapEventToValues(event),
				categories,
				isEditing: true,
				eventId,
				needsApproval
			};
		} catch (error) {
			if (error instanceof BackendApiError && error.status === 404) {
				throw redirect(303, "/admin/events");
			}

			throw error;
		}
	}

	return {
		defaults: DEFAULT_VALUES,
		categories,
		isEditing: false,
		eventId: null,
		needsApproval
	};
};

function mapEventToValues(event: EventDto): EventCreateFormValues {
	return {
		slug: event.slug ?? "",
		title: event.title ?? "",
		summary: event.summary ?? "",
		longDescription: event.longDescription ?? "",
		category: event.category ?? DEFAULT_VALUES.category,
		type: normalizeType(event.type ?? ""),
		status: normalizeStatus(event.status ?? ""),
		dateAd: toDateInputValue(event.date_ad),
		endDateAd: toDateInputValue(event.end_date_ad),
		dateBs: event.date_bs ?? "",
		endDateBs: event.end_date_bs ?? "",
		color: event.color ?? "",
		location: event.location ?? "",
		region: event.region ?? "",
		address: event.address ?? "",
		latitude: event.latitude ?? null,
		longitude: event.longitude ?? null,
		dateRangeLabel: event.dateRangeLabel ?? "",
		durationLabel: event.durationLabel ?? "",
		attendanceLabel: event.attendanceLabel ?? "",
		attendanceNote: event.attendanceNote ?? "",
		entryType: normalizeEntryType(event.entryType ?? ""),
		price: String(event.price ?? 0),
		tags: event.tags?.join("\n") ?? "",
		imageUrls: event.image?.join("\n") ?? "",
		mapImage: event.mapImage ?? "",
		organizer: event.organizer ?? DEFAULT_VALUES.organizer,
		organizerSubtitle: event.organizerSubtitle ?? "",
		organizerVerified: event.organizerVerified ?? true,
		organizerImageUrl: event.organizerImageUrl ?? "",
		featured: event.featured ?? false,
		readTime: event.readTime ?? "",
		highlights: event.highlights?.length ? event.highlights : DEFAULT_VALUES.highlights,
		requiresRegistration: event.requiresRegistration ?? false,
		requiresInvitation: event.requiresInvitation ?? false,
		showEntryType: event.showEntryType ?? true,
		invitationEmailSubject: event.invitationEmailSubject ?? "",
		invitationEmailBodyHtml: event.invitationEmailBodyHtml ?? "",
		selfRegistrationFields: event.selfRegistrationFields ?? []
	};
}

function toDateInputValue(value: string | Date | null | undefined): string {
	if (!value) {
		return "";
	}

	if (value instanceof Date) {
		const dateValue = value.toISOString().slice(0, 10);
		return dateValue.startsWith("0001-") ? "" : dateValue;
	}

	const dateValue = value.toString().split("T")[0];
	return dateValue.startsWith("0001-") ? "" : dateValue;
}
