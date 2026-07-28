import { error, fail, redirect } from "@sveltejs/kit";
import type { Actions, PageServerLoad } from "./$types";
import { BackendApiError, createFestival, fetchCategories, getFestivalById, updateFestival } from "$lib/server/auth/api";
import { can } from "$lib/utils/permissions";
import type { CreateFestivalHighlightPayload, CreateFestivalPayload, FestivalDto, FestivalStatus } from "$lib/types/festivals";

type FestivalCreateFormValues = {
	slug: string;
	title: string;
	summary: string;
	longDescription: string;
	category: string;
	status: FestivalStatus;
	dateAd: string;
	endDateAd: string;
	dateBs: string;
	endDateBs: string;
	color: string;
	region: string;
	address: string;
	latitude: number | null;
	longitude: number | null;
	dateRangeLabel: string;
	durationLabel: string;
	imageUrls: string;
	organizer: string;
	organizerSubtitle: string;
	organizerVerified: boolean;
	organizerImageUrl: string;
	featured: boolean;
	readTime: string;
	isHoliday: boolean;
	highlights: {
		icon: string;
		title: string;
		description: string;
		tone: string;
	}[];
};

const DEFAULT_VALUES: FestivalCreateFormValues = {
	slug: "",
	title: "",
	summary: "",
	longDescription: "",
	category: "Festival",
	status: "draft",
	dateAd: "",
	endDateAd: "",
	dateBs: "",
	endDateBs: "",
	color: "",
	region: "",
	address: "",
	latitude: null,
	longitude: null,
	dateRangeLabel: "",
	durationLabel: "",
	imageUrls: "",
	organizer: "Nepal Tourism Board (NTB)",
	organizerSubtitle: "Official Nepal government tourism authority",
	organizerVerified: true,
	organizerImageUrl: "",
	featured: false,
	readTime: "",
	isHoliday: false,
	highlights: [{ icon: "fi fi-rr-star", title: "", description: "", tone: "orange" }]
};

export const actions: Actions = {
	create: async ({ request, locals }) => {
		if (!can(locals.session?.user, "festivals", "canCreate")) {
			return fail(403, { message: "You don't have permission to create festivals." });
		}
		return saveFestival({ request, locals });
	},
	update: async ({ request, locals, url }) => {
		if (!can(locals.session?.user, "festivals", "canUpdate")) {
			return fail(403, { message: "You don't have permission to update festivals." });
		}
		const festivalId = Number(url.searchParams.get("edit"));
		if (!Number.isInteger(festivalId) || festivalId <= 0) {
			return fail(400, { message: "Valid festival ID is required." });
		}

		return saveFestival({ request, locals, festivalId });
	}
};

async function saveFestival({
	request,
	locals,
	festivalId
}: {
	request: Request;
	locals: App.Locals;
	festivalId?: number;
}) {
		if (!locals.session) {
			throw redirect(303, "/login");
		}

		const formData = await request.formData();
		const values = readValues(formData);

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

		if (values.endDateAd && values.endDateAd < values.dateAd) {
			return fail(400, {
				message: "End date cannot be earlier than the start date.",
				values
			});
		}

		const highlights = buildHighlights(values.highlights);
		const payload: CreateFestivalPayload = {
			slug: values.slug || undefined,
			title: values.title,
			summary: values.summary,
			longDescription: values.longDescription,
			category: values.category,
			status: values.status,
			date_ad: values.dateAd,
			end_date_ad: values.endDateAd || undefined,
			date_bs: values.dateBs || undefined,
			end_date_bs: values.endDateBs || undefined,
			color: values.color || undefined,
			region: values.region,
			address: values.address || undefined,
			latitude: values.latitude ?? undefined,
			longitude: values.longitude ?? undefined,
			dateRangeLabel: values.dateRangeLabel || undefined,
			durationLabel: values.durationLabel || undefined,
			image: parseDelimited(values.imageUrls),
			organizer: values.organizer,
			organizerSubtitle: values.organizerSubtitle || undefined,
			organizerVerified: values.organizerVerified,
			organizerImageUrl: values.organizerImageUrl || undefined,
			highlights,
			featured: values.featured,
			readTime: values.readTime || undefined,
			isHoliday: values.isHoliday
		};

		try {
			if (festivalId) {
				await updateFestival(locals.session.accessToken, festivalId, payload);
			} else {
				await createFestival(locals.session.accessToken, payload);
			}
		} catch (requestError) {
			if (requestError instanceof BackendApiError) {
				return fail(requestError.status || 400, {
					message: requestError.message,
					values
				});
			}

			return fail(500, {
				message: `Unable to ${festivalId ? "update" : "create"} the festival right now.`,
				values
			});
		}

		throw redirect(303, "/admin/festivals");
}

function readValues(formData: FormData): FestivalCreateFormValues {
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
		status: normalizeStatus(getValue(formData, "status")),
		dateAd: getValue(formData, "dateAd"),
		endDateAd: getValue(formData, "endDateAd"),
		dateBs: getValue(formData, "dateBs"),
		endDateBs: getValue(formData, "endDateBs"),
		color: getValue(formData, "color"),
		region: getValue(formData, "region"),
		address: getValue(formData, "address"),
		latitude: parseNullableNumber(getValue(formData, "latitude")),
		longitude: parseNullableNumber(getValue(formData, "longitude")),
		dateRangeLabel: getValue(formData, "dateRangeLabel"),
		durationLabel: getValue(formData, "durationLabel"),
		imageUrls: getValue(formData, "imageUrls"),
		organizer: getValue(formData, "organizer"),
		organizerSubtitle: getValue(formData, "organizerSubtitle"),
		organizerVerified: formData.get("organizerVerified") === "on",
		organizerImageUrl: getValue(formData, "organizerImageUrl"),
		featured: formData.get("featured") === "on",
		readTime: getValue(formData, "readTime"),
		isHoliday: formData.get("isHoliday") === "on",
		highlights
	};
}

function buildHighlights(highlights: FestivalCreateFormValues["highlights"]): CreateFestivalHighlightPayload[] {
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

function getValue(formData: FormData, key: string): string {
	return formData.get(key)?.toString().trim() ?? "";
}

function parseNullableNumber(rawValue: string): number | null {
	if (!rawValue) return null;
	const parsed = Number(rawValue);
	return Number.isFinite(parsed) ? parsed : null;
}

function normalizeStatus(rawValue: string): FestivalStatus {
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

export const prerender = false;

export const load: PageServerLoad = async ({ locals, url }) => {
	if (!locals.session) throw redirect(303, "/login");

	const editId = url.searchParams.get("edit");
	const isEditing = !!editId;
	const requiredOp = isEditing ? "canUpdate" : "canCreate";

	if (!can(locals.session.user, "festivals", requiredOp)) {
		throw error(403, `You don't have permission to ${isEditing ? "edit" : "create"} festivals.`);
	}

	let categories: any[] = [];
	const festivalId = editId ? Number(editId) : null;

	try {
		categories = await fetchCategories(locals.session.accessToken);
	} catch (e) {
		console.error("Failed to load categories in festivals create load:", e);
	}

	if (festivalId) {
		try {
			const festival = await getFestivalById(locals.session.accessToken, festivalId);
			return {
				defaults: mapFestivalToValues(festival),
				categories,
				isEditing: true,
				festivalId
			};
		} catch (error) {
			if (error instanceof BackendApiError && error.status === 404) {
				throw redirect(303, "/admin/festivals");
			}

			throw error;
		}
	}

	return {
		defaults: DEFAULT_VALUES,
		categories,
		isEditing: false,
		festivalId: null
	};
};

function mapFestivalToValues(festival: FestivalDto): FestivalCreateFormValues {
	return {
		slug: festival.slug ?? "",
		title: festival.title ?? "",
		summary: festival.summary ?? "",
		longDescription: festival.longDescription ?? "",
		category: festival.category ?? DEFAULT_VALUES.category,
		status: normalizeStatus(festival.status ?? ""),
		dateAd: toDateInputValue(festival.date_ad),
		endDateAd: toDateInputValue(festival.end_date_ad),
		dateBs: festival.date_bs ?? "",
		endDateBs: festival.end_date_bs ?? "",
		color: festival.color ?? "",
		region: festival.region ?? "",
		address: festival.address ?? "",
		latitude: festival.latitude ?? null,
		longitude: festival.longitude ?? null,
		dateRangeLabel: festival.dateRangeLabel ?? "",
		durationLabel: festival.durationLabel ?? "",
		imageUrls: festival.image?.join("\n") ?? "",
		organizer: festival.organizer || DEFAULT_VALUES.organizer,
		organizerSubtitle: festival.organizerSubtitle ?? "",
		organizerVerified: festival.organizerVerified ?? true,
		organizerImageUrl: festival.organizerImageUrl ?? "",
		featured: festival.featured ?? false,
		readTime: festival.readTime ?? "",
		isHoliday: festival.isHoliday ?? false,
		highlights: festival.highlights?.length ? festival.highlights : DEFAULT_VALUES.highlights
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
