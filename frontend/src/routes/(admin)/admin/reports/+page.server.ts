import { error, redirect } from "@sveltejs/kit";
import type { PageServerLoad } from "./$types";
import { BackendApiError, getReportsSummary, listAllInvitations, listAllRegistrations, listAllWorkshopInvites } from "$lib/server/auth/api";
import { can } from "$lib/utils/permissions";

export const load: PageServerLoad = async ({ locals, url }) => {
	if (!locals.session) throw redirect(303, "/login");

	if (!can(locals.session.user, "reports", "canView")) {
		throw error(403, "You don't have permission to view Reports.");
	}

	const from = url.searchParams.get("from") ?? undefined;
	const to   = url.searchParams.get("to")   ?? undefined;
	const accessToken = locals.session.accessToken;

	const [summaryResult, invitationsResult, registrationsResult, workshopInvitesResult] = await Promise.allSettled([
		getReportsSummary(accessToken, from, to),
		listAllInvitations(accessToken),
		listAllRegistrations(accessToken),
		listAllWorkshopInvites(accessToken)
	]);

	const summary = summaryResult.status === "fulfilled" ? summaryResult.value : null;
	const invitations = invitationsResult.status === "fulfilled" ? invitationsResult.value : [];
	const registrations = registrationsResult.status === "fulfilled" ? registrationsResult.value : [];
	const workshopInvites = workshopInvitesResult.status === "fulfilled" ? workshopInvitesResult.value : [];

	const summaryError =
		summaryResult.status === "rejected" && summaryResult.reason instanceof BackendApiError
			? { message: summaryResult.reason.message, status: summaryResult.reason.status }
			: summaryResult.status === "rejected"
				? { message: "Failed to load report data.", status: 500 }
				: undefined;

	return {
		summary,
		invitations,
		registrations,
		workshopInvites,
		from: from ?? null,
		to: to ?? null,
		error: summaryError
	};
};
