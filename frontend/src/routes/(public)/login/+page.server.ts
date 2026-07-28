import { fail, redirect, isRedirect } from "@sveltejs/kit";
import type { Actions } from "./$types";
import { BackendApiError, loginUser } from "$lib/server/auth/api";
import {
	getClientMetadata,
	getDashboardPathForRole,
	persistAuthCookies
} from "$lib/server/auth/session";

export const actions: Actions = {
	default: async (event) => {
		const formData = await event.request.formData();
		const email = formData.get("email")?.toString().trim() ?? "";
		const password = formData.get("password")?.toString() ?? "";

		if (!email || !password) {
			return fail(400, {
				email,
				message: "Email and password are required."
			});
		}

		try {
			const session = await loginUser(email, password, getClientMetadata(event));
			persistAuthCookies(event.cookies, session);
			throw redirect(303, getDashboardPathForRole(session.user.role));
		} catch (error) {
			if (isRedirect(error)) {
				throw error;
			}

			if (error instanceof BackendApiError) {
				return fail(error.status === 403 ? 403 : 401, {
					email,
					message: error.message
				});
			}

			return fail(500, {
				email,
				message: "Unable to sign in right now."
			});
		}
	}
};
