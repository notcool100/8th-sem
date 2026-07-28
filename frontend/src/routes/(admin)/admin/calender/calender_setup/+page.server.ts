import { error, fail, redirect } from "@sveltejs/kit";
import type { PageServerLoad, Actions } from "./$types";
import {
	BackendApiError,
	getEvents,
	getFestivals,
	fetchCalendarFilterSettings,
	updateCalendarFilterSettings
} from "$lib/server/auth/api";
import { can } from "$lib/utils/permissions";

export const load: PageServerLoad = async ({ locals }) => {
	if (!locals.session) throw redirect(303, "/login");

	if (!can(locals.session.user, "calendar", "canView")) {
		throw error(403, "You don't have permission to view Calendar Management.");
	}

	try {
		const [events, festivals, filterSettings] = await Promise.all([
			getEvents(locals.session.accessToken),
			getFestivals(locals.session.accessToken),
			fetchCalendarFilterSettings()
		]);

		return { events, festivals, filterSettings };
	} catch (error) {
		if (error instanceof BackendApiError) {
			return {
				events: [],
				festivals: [],
				filterSettings: {
					showCategory: true,
					showDateRange: true,
					showLocation: true,
					showPrice: true,
					showTags: true,
					isSundayHoliday: true
				},
				error: {
					message: error.message,
					status: error.status
				}
			};
		}

		throw error;
	}
};

export const actions: Actions = {
	saveFilterSettings: async ({ request, locals }) => {
		if (!locals.session) return fail(401, { message: "Unauthorized." });
		if (!can(locals.session.user, "calendar", "canUpdate")) {
			return fail(403, { message: "You don't have permission to update calendar settings." });
		}

		const formData = await request.formData();

		const payload = {
			showCategory:    formData.get("showCategory")    === "true",
			showDateRange:   formData.get("showDateRange")   === "true",
			showLocation:    formData.get("showLocation")    === "true",
			showPrice:       formData.get("showPrice")       === "true",
			showTags:        formData.get("showTags")        === "true",
			isSundayHoliday: formData.get("isSundayHoliday") === "true"
		};

		try {
			await updateCalendarFilterSettings(locals.session.accessToken, payload);
			return { success: true, message: "Filter settings saved." };
		} catch (error) {
			if (error instanceof BackendApiError) {
				return fail(error.status || 400, { message: error.message });
			}
			return fail(500, { message: "Failed to save filter settings." });
		}
	}
};
