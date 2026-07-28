import { json } from "@sveltejs/kit";
import { BackendApiError, verifyInvitation, verifyWorkshopInvite } from "$lib/server/auth/api";
import type { RequestHandler } from "./$types";

export const POST: RequestHandler = async ({ locals, request }) => {
	if (!locals.session) {
		return json({ message: "Not authenticated." }, { status: 401 });
	}

	let id = 0;
	let kind = "invitation";
	try {
		const body = await request.json();
		id = Number(body?.id);
		kind = body?.kind === "workshop" ? "workshop" : "invitation";
	} catch {
		return json({ message: "Invalid request body." }, { status: 400 });
	}

	if (!Number.isInteger(id) || id <= 0) {
		return json({ message: "A valid id is required." }, { status: 400 });
	}

	try {
		if (kind === "workshop") {
			const result = await verifyWorkshopInvite(locals.session.accessToken, id);
			return json({ kind: "workshop", ...result });
		}

		const result = await verifyInvitation(locals.session.accessToken, id);
		return json({ kind: "invitation", ...result });
	} catch (error) {
		if (error instanceof BackendApiError) {
			return json({ message: error.message, errors: error.errors }, { status: error.status || 500 });
		}
		return json({ message: "Unable to verify." }, { status: 500 });
	}
};
