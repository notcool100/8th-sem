import { error, fail, redirect } from "@sveltejs/kit";
import type { Actions, PageServerLoad } from "./$types";
import {
	BackendApiError,
	cancelInvitation,
	createInvitation,
	getEventById,
	listInvitations,
	resendInvitation
} from "$lib/server/auth/api";
import type { InviteGuestPayload } from "$lib/types/invitations";

function requireAdmin(locals: App.Locals) {
	if (!locals.session) {
		throw redirect(303, "/login");
	}
	if (locals.session.user.role === "client") {
		throw redirect(303, "/client/dashboard");
	}
	return locals.session;
}

export const load: PageServerLoad = async ({ locals, params }) => {
	const session = requireAdmin(locals);

	const eventId = Number(params.id);
	if (!Number.isInteger(eventId) || eventId <= 0) {
		throw redirect(303, "/admin/events");
	}

	try {
		const [event, invitations] = await Promise.all([
			getEventById(session.accessToken, eventId),
			listInvitations(session.accessToken, eventId)
		]);

		return { event, invitations, eventId };
	} catch (err) {
		if (err instanceof BackendApiError && err.status === 404) {
			throw redirect(303, "/admin/events");
		}
		if (err instanceof BackendApiError && err.status === 403) {
			throw error(403, err.message);
		}
		throw err;
	}
};

export const actions: Actions = {
	invite: async ({ request, locals, params }) => {
		const session = requireAdmin(locals);
		const eventId = Number(params.id);

		const formData = await request.formData();
		const fullName = (formData.get("fullName")?.toString() ?? "").trim();
		const email = (formData.get("email")?.toString() ?? "").trim();
		const phone = (formData.get("phone")?.toString() ?? "").trim();
		const organization = (formData.get("organization")?.toString() ?? "").trim();
		const expiresAt = (formData.get("expiresAt")?.toString() ?? "").trim();

		if (!fullName || !email) {
			return fail(400, { message: "Full name and email are required.", values: { fullName, email, phone, organization, expiresAt } });
		}

		const payload: InviteGuestPayload = {
			fullName,
			email,
			phone: phone || undefined,
			organization: organization || undefined,
			expiresAtUtc: expiresAt ? new Date(expiresAt).toISOString() : undefined
		};

		try {
			await createInvitation(session.accessToken, eventId, payload);
			return { success: true, message: `Invitation sent to ${email}.` };
		} catch (error) {
			if (error instanceof BackendApiError) {
				return fail(error.status || 400, {
					message: error.message,
					values: { fullName, email, phone, organization, expiresAt }
				});
			}
			return fail(500, { message: "Unable to send the invitation right now." });
		}
	},

	resend: async ({ request, locals }) => {
		requireAdmin(locals);
		const formData = await request.formData();
		const id = Number(formData.get("invitationId"));
		if (!Number.isInteger(id) || id <= 0) {
			return fail(400, { message: "Invalid invitation." });
		}

		try {
			await resendInvitation(locals.session!.accessToken, id);
			return { success: true, message: "Invitation re-sent." };
		} catch (error) {
			if (error instanceof BackendApiError) {
				return fail(error.status || 400, { message: error.message });
			}
			return fail(500, { message: "Unable to resend the invitation." });
		}
	},

	cancel: async ({ request, locals }) => {
		requireAdmin(locals);
		const formData = await request.formData();
		const id = Number(formData.get("invitationId"));
		if (!Number.isInteger(id) || id <= 0) {
			return fail(400, { message: "Invalid invitation." });
		}

		try {
			await cancelInvitation(locals.session!.accessToken, id);
			return { success: true, message: "Invitation cancelled." };
		} catch (error) {
			if (error instanceof BackendApiError) {
				return fail(error.status || 400, { message: error.message });
			}
			return fail(500, { message: "Unable to cancel the invitation." });
		}
	}
};

export const prerender = false;
