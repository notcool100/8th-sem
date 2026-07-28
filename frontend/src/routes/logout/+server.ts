import { json } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { logoutUser } from "$lib/server/auth/api";
import {
	REFRESH_TOKEN_COOKIE,
	clearAuthCookies,
	getClientMetadata
} from "$lib/server/auth/session";

export const POST: RequestHandler = async (event) => {
	const refreshToken = event.cookies.get(REFRESH_TOKEN_COOKIE);

	if (refreshToken) {
		try {
			await logoutUser(refreshToken, getClientMetadata(event));
		} catch {
			// Clear local auth state even if backend logout fails.
		}
	}

	clearAuthCookies(event.cookies);
	return json({ ok: true });
};
