import { error, fail, redirect } from "@sveltejs/kit";
import type { Actions, PageServerLoad } from "./$types";
import {
	BackendApiError,
	approveRegistration,
	listAllRegistrations,
	listAllInvitations,
	listAllWorkshopInvites,
	rejectRegistration
} from "$lib/server/auth/api";
import { can } from "$lib/utils/permissions";

export const load: PageServerLoad = async ({ locals, url }) => {
	if (!locals.session) throw redirect(303, "/login");

	if (!can(locals.session.user, "attendees", "canView")) {
		throw error(403, "You don't have permission to view Attendees / Registrations.");
	}

	const eventIdParam = url.searchParams.get("eventId");
	const initialEventId = eventIdParam ? Number(eventIdParam) : null;
	const accessToken = locals.session.accessToken;

	const [registrationsResult, invitationsResult, workshopInvitesResult] = await Promise.allSettled([
		listAllRegistrations(accessToken),
		listAllInvitations(accessToken),
		listAllWorkshopInvites(accessToken)
	]);

	function unwrap<T>(result: PromiseSettledResult<T[]>): { items: T[]; error?: { message: string; status: number } } {
		if (result.status === "fulfilled") return { items: result.value };
		const reason = result.reason;
		if (reason instanceof BackendApiError) {
			return { items: [], error: { message: reason.message, status: reason.status } };
		}
		return { items: [], error: { message: "Failed to load data.", status: 500 } };
	}

	const registrations = unwrap(registrationsResult);
	const invitations = unwrap(invitationsResult);
	const workshopInvites = unwrap(workshopInvitesResult);

	return {
		registrations: registrations.items,
		invitations: invitations.items,
		workshopInvites: workshopInvites.items,
		initialEventId,
		canUpdate: can(locals.session.user, "attendees", "canUpdate"),
		errors: {
			registrations: registrations.error,
			invitations: invitations.error,
			workshopInvites: workshopInvites.error
		}
	};
};

export const actions: Actions = {
	approve: async ({ request, locals }) => {
		if (!locals.session) throw redirect(303, "/login");
		if (!can(locals.session.user, "attendees", "canUpdate")) {
			return fail(403, { message: "You don't have permission to approve registrations." });
		}

		const formData = await request.formData();
		const id = Number(formData.get("registrationId"));
		if (!Number.isInteger(id) || id <= 0) {
			return fail(400, { message: "Invalid registration." });
		}

		try {
			await approveRegistration(locals.session.accessToken, id);
			return { success: true, message: "Registration approved." };
		} catch (err) {
			if (err instanceof BackendApiError) {
				return fail(err.status || 400, { message: err.message });
			}
			return fail(500, { message: "Unable to approve the registration." });
		}
	},

	reject: async ({ request, locals }) => {
		if (!locals.session) throw redirect(303, "/login");
		if (!can(locals.session.user, "attendees", "canUpdate")) {
			return fail(403, { message: "You don't have permission to reject registrations." });
		}

		const formData = await request.formData();
		const id = Number(formData.get("registrationId"));
		if (!Number.isInteger(id) || id <= 0) {
			return fail(400, { message: "Invalid registration." });
		}

		try {
			await rejectRegistration(locals.session.accessToken, id);
			return { success: true, message: "Registration rejected." };
		} catch (err) {
			if (err instanceof BackendApiError) {
				return fail(err.status || 400, { message: err.message });
			}
			return fail(500, { message: "Unable to reject the registration." });
		}
	}
};

export const prerender = false;
