import { json } from "@sveltejs/kit";
import { BackendApiError, searchEvents } from "$lib/server/auth/api";
import type { RequestHandler } from "./$types";

export const GET: RequestHandler = async ({ url }) => {
	try {
		const query = url.searchParams.get("q") ?? "";
		const results = await searchEvents(query);
		return json(results);
	} catch (requestError) {
		if (requestError instanceof BackendApiError) {
			return json(
				{ message: requestError.message, errors: requestError.errors },
				{ status: requestError.status || 500 }
			);
		}

		return json({ message: "Unable to search events." }, { status: 500 });
	}
};
