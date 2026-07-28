import { error } from "@sveltejs/kit";
import { env } from "$env/dynamic/private";

const DEFAULT_API_BASE_URL = "http://localhost:5232";

function getApiBaseUrl(): string {
	const configuredBaseUrl = env.API_BASE_URL?.trim() || DEFAULT_API_BASE_URL;
	return configuredBaseUrl.endsWith("/") ? configuredBaseUrl.slice(0, -1) : configuredBaseUrl;
}

/** Proxies a binary (e.g. .xlsx) export from the backend, forwarding the caller's Bearer token. */
export async function proxyExport(backendPath: string, accessToken: string | undefined): Promise<Response> {
	if (!accessToken) {
		throw error(401, "Unauthorized");
	}

	const response = await fetch(`${getApiBaseUrl()}${backendPath}`, {
		headers: { Authorization: `Bearer ${accessToken}` }
	});

	if (!response.ok) {
		throw error(response.status, "Failed to generate export.");
	}

	const body = await response.arrayBuffer();
	return new Response(body, {
		status: 200,
		headers: {
			"Content-Type":
				response.headers.get("content-type") ??
				"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
			"Content-Disposition": response.headers.get("content-disposition") ?? "attachment; filename=export.xlsx"
		}
	});
}
