import { json } from "@sveltejs/kit";
import { BackendApiError, suggestTags } from "$lib/server/auth/api";
import type { RequestHandler } from "./$types";

export const POST: RequestHandler = async ({ request, locals }) => {
	if (!locals.session?.accessToken) {
		return json({ message: "Unauthorized." }, { status: 401 });
	}

	try {
		const { title, description } = await request.json();
		const suggestions = await suggestTags(locals.session.accessToken, title ?? "", description ?? "");
		return json(suggestions);
	} catch (requestError) {
		if (requestError instanceof BackendApiError) {
			return json(
				{ message: requestError.message, errors: requestError.errors },
				{ status: requestError.status || 500 }
			);
		}

		return json({ message: "Unable to suggest tags." }, { status: 500 });
	}
};
