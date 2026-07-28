import { error, redirect } from "@sveltejs/kit";
import type { PageServerLoad } from "./$types";
import { can } from "$lib/utils/permissions";

export const load: PageServerLoad = async ({ locals }) => {
	if (!locals.session) throw redirect(303, "/login");

	if (!can(locals.session.user, "check_in", "canView")) {
		throw error(403, "You don't have permission to access Event Check-in.");
	}

	return {};
};

export const prerender = false;
