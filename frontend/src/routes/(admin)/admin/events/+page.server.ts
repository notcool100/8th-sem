import { error, fail, redirect } from "@sveltejs/kit";
import type { Actions, PageServerLoad } from "./$types";
import { BackendApiError, getEvents, getEventById, createEvent, deleteEvent } from "$lib/server/auth/api";
import { can } from "$lib/utils/permissions";
import type { CreateEventPayload } from "$lib/types/events";

export const load: PageServerLoad = async ({ locals }) => {
	if (!locals.session) throw redirect(303, "/login");

	if (!can(locals.session.user, "events", "canView")) {
		throw error(403, "You don't have permission to view Events.");
	}

	try {
		const events = await getEvents(locals.session.accessToken);
		return {
			events,
			canCreate: can(locals.session.user, "events", "canCreate"),
			canUpdate: can(locals.session.user, "events", "canUpdate"),
			canDelete: can(locals.session.user, "events", "canDelete"),
			currentUserId: locals.session.user.id,
			isSuperAdmin: locals.session.user.role === "superadmin"
		};
	} catch (err) {
		if (err instanceof BackendApiError) {
			return {
				events: [],
				canCreate: can(locals.session.user, "events", "canCreate"),
				canUpdate: can(locals.session.user, "events", "canUpdate"),
				canDelete: can(locals.session.user, "events", "canDelete"),
				currentUserId: locals.session.user.id,
				isSuperAdmin: locals.session.user.role === "superadmin",
				error: { message: err.message, status: err.status }
			};
		}
		throw err;
	}
};

export const actions: Actions = {
	delete: async ({ request, locals }) => {
		if (!locals.session) throw redirect(303, "/login");

		if (!can(locals.session.user, "events", "canDelete")) {
			return fail(403, { message: "You don't have permission to delete events." });
		}

		const formData = await request.formData();
		const id = formData.get("id");
		if (!id) return fail(400, { message: "Event ID is required." });

		try {
			await deleteEvent(locals.session.accessToken, Number(id));
			return { success: true };
		} catch (err) {
			if (err instanceof BackendApiError) {
				return fail(err.status, { message: err.message, errors: err.errors });
			}
			return fail(500, { message: "Failed to delete event." });
		}
	},

	duplicate: async ({ request, locals }) => {
		if (!locals.session) throw redirect(303, "/login");

		if (!can(locals.session.user, "events", "canCreate")) {
			return fail(403, { message: "You don't have permission to create events." });
		}

		const formData = await request.formData();
		const id = formData.get("id");
		const dateAd = formData.get("dateAd")?.toString();
		const endDateAd = formData.get("endDateAd")?.toString();

		if (!id) return fail(400, { message: "Event ID is required." });
		if (!dateAd) return fail(400, { message: "Start date is required." });

		try {
			const source = await getEventById(locals.session.accessToken, Number(id));

			const payload: CreateEventPayload = {
				title: `${source.title} (Copy)`,
				summary: source.summary,
				longDescription: source.longDescription,
				category: source.category,
				type: source.type,
				status: "draft",
				date_ad: dateAd,
				end_date_ad: endDateAd || undefined,
				color: source.color || undefined,
				location: source.location,
				region: source.region,
				address: source.address || undefined,
				latitude: source.latitude ?? undefined,
				longitude: source.longitude ?? undefined,
				attendanceLabel: source.attendanceLabel || undefined,
				attendanceNote: source.attendanceNote || undefined,
				entryType: (source.entryType as CreateEventPayload["entryType"]) || undefined,
				showEntryType: source.showEntryType,
				price: source.price,
				rating: 0,
				tags: source.tags,
				image: source.image,
				mapImage: source.mapImage || undefined,
				organizer: source.organizer,
				organizerSubtitle: source.organizerSubtitle || undefined,
				organizerVerified: source.organizerVerified,
				highlights: source.highlights,
				featured: false,
				readTime: source.readTime || undefined,
				requiresRegistration: source.requiresRegistration,
				requiresInvitation: source.requiresInvitation,
				selfRegistrationFields: source.selfRegistrationFields ?? []
			};

			const result = await createEvent(locals.session.accessToken, payload);
			return { success: true, duplicated: true, event: result.event, requiresApproval: result.requiresApproval };
		} catch (err) {
			if (err instanceof BackendApiError) {
				return fail(err.status, { message: err.message, errors: err.errors });
			}
			return fail(500, { message: "Failed to duplicate event." });
		}
	}
};
