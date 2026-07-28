import { env } from "$env/dynamic/private";
import type { RequestHandler } from "./$types";

const DEFAULT_API_BASE_URL = "http://localhost:5232";

function getApiBaseUrl(): string {
	const configured = env.API_BASE_URL?.trim() || DEFAULT_API_BASE_URL;
	return configured.endsWith("/") ? configured.slice(0, -1) : configured;
}

export const GET: RequestHandler = async ({ params }) => {
	const backendUrl = `${getApiBaseUrl()}/uploads/${encodeURIComponent(params.file)}`;

	let response: Response;
	try {
		response = await fetch(backendUrl);
	} catch {
		return new Response("Could not reach backend.", { status: 502 });
	}

	if (!response.ok) {
		return new Response("Not found.", { status: 404 });
	}

	const contentType = response.headers.get("content-type") ?? "application/octet-stream";
	const body = await response.arrayBuffer();

	return new Response(body, {
		status: 200,
		headers: {
			"Content-Type": contentType,
			"Cache-Control": "public, max-age=86400",
		},
	});
};
