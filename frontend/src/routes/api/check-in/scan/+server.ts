import { json } from "@sveltejs/kit";
import { BackendApiError, scanInvitation, scanWorkshopInvite } from "$lib/server/auth/api";
import type { RequestHandler } from "./$types";

/// Tries the QR/code against regular event invitations first, then workshop
/// invites, so the door scanner doesn't need to know which list a guest is on.
export const POST: RequestHandler = async ({ locals, request }) => {
	if (!locals.session) {
		return json({ message: "Not authenticated." }, { status: 401 });
	}

	let token = "";
	try {
		const body = await request.json();
		token = (body?.token ?? "").toString().trim();
	} catch {
		return json({ message: "Invalid request body." }, { status: 400 });
	}

	if (!token) {
		return json({ message: "A QR token is required." }, { status: 400 });
	}

	try {
		const invitationResult = await scanInvitation(locals.session.accessToken, token);
		if (invitationResult.result !== "invalid") {
			return json({ kind: "invitation", ...invitationResult });
		}

		const workshopResult = await scanWorkshopInvite(locals.session.accessToken, token);
		if (workshopResult.result !== "invalid") {
			return json({ kind: "workshop", ...workshopResult });
		}

		return json({ kind: "invitation", ...invitationResult });
	} catch (error) {
		if (error instanceof BackendApiError) {
			return json({ message: error.message, errors: error.errors }, { status: error.status || 500 });
		}
		return json({ message: "Unable to scan the code." }, { status: 500 });
	}
};
