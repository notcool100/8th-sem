import { error, fail, redirect } from "@sveltejs/kit";
import type { Actions, PageServerLoad } from "./$types";
import { BackendApiError, getFestivals, getFestivalById, createFestival, deleteFestival } from "$lib/server/auth/api";
import { can } from "$lib/utils/permissions";
import type { CreateFestivalPayload } from "$lib/types/festivals";

export const load: PageServerLoad = async ({ locals }) => {
	if (!locals.session) throw redirect(303, "/login");

	if (!can(locals.session.user, "festivals", "canView")) {
		throw error(403, "You don't have permission to view Festivals.");
	}

	try {
		const festivals = await getFestivals(locals.session.accessToken);
		return {
			festivals,
			canCreate: can(locals.session.user, "festivals", "canCreate"),
			canUpdate: can(locals.session.user, "festivals", "canUpdate"),
			canDelete: can(locals.session.user, "festivals", "canDelete")
		};
	} catch (err) {
		if (err instanceof BackendApiError) {
			return {
				festivals: [],
				canCreate: can(locals.session.user, "festivals", "canCreate"),
				canUpdate: can(locals.session.user, "festivals", "canUpdate"),
				canDelete: can(locals.session.user, "festivals", "canDelete"),
				error: { message: err.message, status: err.status }
			};
		}
		throw err;
	}
};

export const actions: Actions = {
	delete: async ({ request, locals }) => {
		if (!locals.session) throw redirect(303, "/login");

		if (!can(locals.session.user, "festivals", "canDelete")) {
			return fail(403, { message: "You don't have permission to delete festivals." });
		}

		const formData = await request.formData();
		const id = formData.get("id");
		if (!id) return fail(400, { message: "Festival ID is required." });

		try {
			await deleteFestival(locals.session.accessToken, Number(id));
			return { success: true };
		} catch (err) {
			if (err instanceof BackendApiError) {
				return fail(err.status, { message: err.message, errors: err.errors });
			}
			return fail(500, { message: "Failed to delete festival." });
		}
	},

	duplicate: async ({ request, locals }) => {
		if (!locals.session) throw redirect(303, "/login");

		if (!can(locals.session.user, "festivals", "canCreate")) {
			return fail(403, { message: "You don't have permission to create festivals." });
		}

		const formData = await request.formData();
		const id = formData.get("id");
		const dateAd = formData.get("dateAd")?.toString();
		const endDateAd = formData.get("endDateAd")?.toString();

		if (!id) return fail(400, { message: "Festival ID is required." });
		if (!dateAd) return fail(400, { message: "Start date is required." });

		try {
			const source = await getFestivalById(locals.session.accessToken, Number(id));

			const payload: CreateFestivalPayload = {
				title: `${source.title} (Copy)`,
				summary: source.summary,
				longDescription: source.longDescription,
				category: source.category,
				status: "draft",
				date_ad: dateAd,
				end_date_ad: endDateAd || undefined,
				color: source.color || undefined,
				region: source.region,
				address: source.address || undefined,
				latitude: source.latitude ?? undefined,
				longitude: source.longitude ?? undefined,
				image: source.image,
				organizer: source.organizer,
				organizerSubtitle: source.organizerSubtitle || undefined,
				organizerVerified: source.organizerVerified,
				organizerImageUrl: source.organizerImageUrl || undefined,
				highlights: source.highlights,
				featured: false,
				readTime: source.readTime || undefined,
				isHoliday: source.isHoliday
			};

			const festival = await createFestival(locals.session.accessToken, payload);
			return { success: true, duplicated: true, festival };
		} catch (err) {
			if (err instanceof BackendApiError) {
				return fail(err.status, { message: err.message, errors: err.errors });
			}
			return fail(500, { message: "Failed to duplicate festival." });
		}
	}
};
