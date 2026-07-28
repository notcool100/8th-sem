import { json } from "@sveltejs/kit";
import { BackendApiError, scanInvitation } from "$lib/server/auth/api";
import type { RequestHandler } from "./$types";

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
		const result = await scanInvitation(locals.session.accessToken, token);
		return json(result);
	} catch (error) {
		if (error instanceof BackendApiError) {
			return json({ message: error.message, errors: error.errors }, { status: error.status || 500 });
		}
		return json({ message: "Unable to scan invitation." }, { status: 500 });
	}
};
