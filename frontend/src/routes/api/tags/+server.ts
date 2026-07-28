import { json } from "@sveltejs/kit";
import { BackendApiError, fetchTags } from "$lib/server/auth/api";
import type { RequestHandler } from "./$types";

export const GET: RequestHandler = async () => {
	try {
		const tags = await fetchTags();
		return json(tags);
	} catch (requestError) {
		if (requestError instanceof BackendApiError) {
			return json(
				{ message: requestError.message, errors: requestError.errors },
				{ status: requestError.status || 500 }
			);
		}

		return json({ message: "Unable to load tags." }, { status: 500 });
	}
};
