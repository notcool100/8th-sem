import { json } from "@sveltejs/kit";
import { BackendApiError, getRecommendations } from "$lib/server/auth/api";
import type { RequestHandler } from "./$types";

export const GET: RequestHandler = async ({ params }) => {
	const eventId = Number(params.id);
	if (!Number.isFinite(eventId)) {
		return json({ message: "Invalid event id." }, { status: 400 });
	}

	try {
		const recommendations = await getRecommendations(eventId);
		return json(recommendations);
	} catch (requestError) {
		if (requestError instanceof BackendApiError) {
			return json(
				{ message: requestError.message, errors: requestError.errors },
				{ status: requestError.status || 500 }
			);
		}

		return json({ message: "Unable to load recommendations." }, { status: 500 });
	}
};
