import { error, redirect } from "@sveltejs/kit";
import type { PageServerLoad } from "./$types";
import { BackendApiError, getReportsSummary } from "$lib/server/auth/api";
import { can } from "$lib/utils/permissions";

export const load: PageServerLoad = async ({ locals, url }) => {
	if (!locals.session) throw redirect(303, "/login");

	if (!can(locals.session.user, "dashboard", "canView")) {
		throw error(403, "You don't have permission to view the Dashboard.");
	}

	const from = url.searchParams.get("from") ?? undefined;
	const to   = url.searchParams.get("to")   ?? undefined;

	try {
		const summary = await getReportsSummary(locals.session.accessToken, from, to);
		return { summary, from: from ?? null, to: to ?? null };
	} catch (err) {
		if (err instanceof BackendApiError) {
			return {
				summary: null,
				from: from ?? null,
				to: to ?? null,
				error: { message: err.message, status: err.status }
			};
		}
		throw err;
	}
};
