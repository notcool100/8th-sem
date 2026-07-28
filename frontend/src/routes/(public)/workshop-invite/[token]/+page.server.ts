import type { PageServerLoad } from "./$types";
import { BackendApiError, getWorkshopInviteByToken } from "$lib/server/auth/api";

export const load: PageServerLoad = async ({ params }) => {
	try {
		const invite = await getWorkshopInviteByToken(params.token);
		return { invite, notFound: false };
	} catch (error) {
		if (error instanceof BackendApiError && error.status === 404) {
			return { invite: null, notFound: true };
		}
		return { invite: null, notFound: true };
	}
};

export const prerender = false;
