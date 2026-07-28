import { proxyExport } from "$lib/server/exportProxy";
import type { RequestHandler } from "./$types";

export const GET: RequestHandler = async ({ params, locals }) => {
	return proxyExport(`/api/events/${encodeURIComponent(params.eventId)}/invitations/export`, locals.session?.accessToken);
};
