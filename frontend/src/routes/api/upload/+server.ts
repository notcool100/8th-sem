import { error, json } from "@sveltejs/kit";
import { env } from "$env/dynamic/private";
import type { RequestHandler } from "./$types";

const DEFAULT_API_BASE_URL = "http://localhost:5232";

function getApiBaseUrl(): string {
	const configuredBaseUrl = env.API_BASE_URL?.trim() || DEFAULT_API_BASE_URL;
	return configuredBaseUrl.endsWith("/") ? configuredBaseUrl.slice(0, -1) : configuredBaseUrl;
}

export const POST: RequestHandler = async ({ request, locals }) => {
	if (!locals.session?.accessToken) {
		throw error(401, "Unauthorized");
	}

	try {
		const formData = await request.formData();
		const file = formData.get("file");
		if (!file || !(file instanceof File)) {
			throw error(400, "No file uploaded or file is invalid.");
		}

		const backendFormData = new FormData();
		backendFormData.append("file", file);

		const backendUrl = `${getApiBaseUrl()}/api/upload`;
		const response = await fetch(backendUrl, {
			method: "POST",
			body: backendFormData,
			headers: {
				"Authorization": `Bearer ${locals.session.accessToken}`
			}
		});

		if (!response.ok) {
			const errorData = await response.json().catch(() => ({}));
			return json(
				{ message: errorData.message || "Failed to upload file to backend." },
				{ status: response.status }
			);
		}

		const result = await response.json();
		return json(result);
	} catch (err: any) {
		return json(
			{ message: err.message || "An error occurred during file upload." },
			{ status: err.status || 500 }
		);
	}
};
