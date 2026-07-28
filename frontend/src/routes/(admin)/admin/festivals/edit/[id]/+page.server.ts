import { redirect } from "@sveltejs/kit";
import type { PageServerLoad } from "./$types";

export const load: PageServerLoad = async ({ params }) => {
	throw redirect(303, `/admin/festivals/create?edit=${params.id}`);
};
