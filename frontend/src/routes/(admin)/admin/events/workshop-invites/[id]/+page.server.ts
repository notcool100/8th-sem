import { error, fail, redirect } from "@sveltejs/kit";
import type { Actions, PageServerLoad } from "./$types";
import {
	BackendApiError,
	getEventById,
	importWorkshopInvites,
	listWorkshopInvites,
	sendWorkshopInvite
} from "$lib/server/auth/api";
import type { ImportWorkshopInviteeRow } from "$lib/types/workshop-invites";

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
		const [event, invites] = await Promise.all([
			getEventById(session.accessToken, eventId),
			listWorkshopInvites(session.accessToken, eventId)
		]);

		return { event, invites, eventId };
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
	import: async ({ request, locals, params }) => {
		const session = requireAdmin(locals);
		const eventId = Number(params.id);

		const formData = await request.formData();
		const raw = formData.get("rows")?.toString() ?? "";

		let rows: ImportWorkshopInviteeRow[];
		try {
			rows = JSON.parse(raw);
		} catch {
			return fail(400, { message: "Could not read the spreadsheet rows." });
		}

		if (!Array.isArray(rows) || rows.length === 0) {
			return fail(400, { message: "No rows found in the spreadsheet." });
		}

		try {
			const imported = await importWorkshopInvites(session.accessToken, eventId, rows);
			return { success: true, message: `Imported ${imported} new recipient(s).` };
		} catch (error) {
			if (error instanceof BackendApiError) {
				return fail(error.status || 400, { message: error.message });
			}
			return fail(500, { message: "Unable to import the spreadsheet right now." });
		}
	},

	send: async ({ request, locals }) => {
		const session = requireAdmin(locals);
		const formData = await request.formData();
		const id = Number(formData.get("inviteId"));
		const workshopDate = (formData.get("workshopDate")?.toString() ?? "").trim();

		if (!Number.isInteger(id) || id <= 0) {
			return fail(400, { message: "Invalid recipient." });
		}
		if (!workshopDate) {
			return fail(400, { message: "Pick a workshop date first." });
		}

		try {
			await sendWorkshopInvite(session.accessToken, id, { workshopDate });
			return { success: true, message: "Invitation sent." };
		} catch (error) {
			if (error instanceof BackendApiError) {
				return fail(error.status || 400, { message: error.message });
			}
			return fail(500, { message: "Unable to send the invitation right now." });
		}
	}
};

export const prerender = false;
