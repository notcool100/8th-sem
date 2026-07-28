import { json } from "@sveltejs/kit";
import { BackendApiError, fetchCategories } from "$lib/server/auth/api";
import type { RequestHandler } from "./$types";

export const GET: RequestHandler = async ({ locals, url }) => {
	try {
		const type = url.searchParams.get("type");
		const categories = await fetchCategories(
			locals.session?.accessToken ?? "",
			type === "event" || type === "festival" ? type : undefined
		);
		return json(categories);
	} catch (requestError) {
		if (requestError instanceof BackendApiError) {
			return json(
				{ message: requestError.message, errors: requestError.errors },
				{ status: requestError.status || 500 }
			);
		}

		return json({ message: "Unable to load categories." }, { status: 500 });
	}
};
