import { json } from "@sveltejs/kit";
import { BackendApiError, verifyInvitation } from "$lib/server/auth/api";
import type { RequestHandler } from "./$types";

export const POST: RequestHandler = async ({ locals, request }) => {
	if (!locals.session) {
		return json({ message: "Not authenticated." }, { status: 401 });
	}

	let id = 0;
	try {
		const body = await request.json();
		id = Number(body?.id);
	} catch {
		return json({ message: "Invalid request body." }, { status: 400 });
	}

	if (!Number.isInteger(id) || id <= 0) {
		return json({ message: "A valid invitation id is required." }, { status: 400 });
	}

	try {
		const result = await verifyInvitation(locals.session.accessToken, id);
		return json(result);
	} catch (error) {
		if (error instanceof BackendApiError) {
			return json({ message: error.message, errors: error.errors }, { status: error.status || 500 });
		}
		return json({ message: "Unable to verify invitation." }, { status: 500 });
	}
};
