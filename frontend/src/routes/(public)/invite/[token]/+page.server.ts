import type { PageServerLoad } from "./$types";
import { BackendApiError, getInvitationByToken } from "$lib/server/auth/api";

export const load: PageServerLoad = async ({ params }) => {
	try {
		const invitation = await getInvitationByToken(params.token);
		return { invitation, notFound: false };
	} catch (error) {
		if (error instanceof BackendApiError && error.status === 404) {
			return { invitation: null, notFound: true };
		}
		return { invitation: null, notFound: true };
	}
};

export const prerender = false;
