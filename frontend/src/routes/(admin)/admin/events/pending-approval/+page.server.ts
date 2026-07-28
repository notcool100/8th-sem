import { error, fail, redirect } from "@sveltejs/kit";
import type { Actions, PageServerLoad } from "./$types";
import { BackendApiError, approveEvent, listApprovals, rejectEvent } from "$lib/server/auth/api";
import { can } from "$lib/utils/permissions";

export const load: PageServerLoad = async ({ locals }) => {
	if (!locals.session) throw redirect(303, "/login");

	if (!can(locals.session.user, "events", "canView")) {
		throw error(403, "You don't have permission to view Event Approvals.");
	}

	try {
		const approvals = await listApprovals(locals.session.accessToken);
		return {
			approvals,
			canUpdate: can(locals.session.user, "events", "canUpdate")
		};
	} catch (requestError) {
		if (requestError instanceof BackendApiError) {
			throw error(requestError.status || 500, requestError.message);
		}
		throw error(500, "Unable to load approvals.");
	}
};

export const actions: Actions = {
	approve: async ({ request, locals }) => {
		if (!locals.session) return fail(401, { message: "Unauthorized." });
		if (!can(locals.session.user, "events", "canUpdate")) {
			return fail(403, { message: "You don't have permission to approve events." });
		}

		const formData = await request.formData();
		const approvalId = parseInt(formData.get("approvalId")?.toString() ?? "0", 10);
		if (!approvalId) return fail(400, { message: "Invalid approval ID." });

		try {
			await approveEvent(locals.session.accessToken, approvalId);
			return { success: true, message: "Event approved successfully." };
		} catch (requestError) {
			if (requestError instanceof BackendApiError) {
				return fail(requestError.status || 400, { message: requestError.message });
			}
			return fail(500, { message: "Unable to approve the event." });
		}
	},

	reject: async ({ request, locals }) => {
		if (!locals.session) return fail(401, { message: "Unauthorized." });
		if (!can(locals.session.user, "events", "canUpdate")) {
			return fail(403, { message: "You don't have permission to reject events." });
		}

		const formData = await request.formData();
		const approvalId = parseInt(formData.get("approvalId")?.toString() ?? "0", 10);
		const reason = formData.get("reason")?.toString().trim() ?? "";

		if (!approvalId) return fail(400, { message: "Invalid approval ID." });
		if (!reason) return fail(400, { message: "Rejection reason is required." });

		try {
			await rejectEvent(locals.session.accessToken, approvalId, reason);
			return { success: true, message: "Event rejected." };
		} catch (requestError) {
			if (requestError instanceof BackendApiError) {
				return fail(requestError.status || 400, { message: requestError.message });
			}
			return fail(500, { message: "Unable to reject the event." });
		}
	}
};
