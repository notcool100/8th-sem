import { error, fail, redirect } from "@sveltejs/kit";
import type { Actions, PageServerLoad } from "./$types";
import {
	BackendApiError,
	createUser,
	deleteUser,
	fetchNavItems,
	listUsers,
	updateUser,
	updateUserPermissions
} from "$lib/server/auth/api";

export const load: PageServerLoad = async ({ locals }) => {
	if (locals.session?.user.role !== "superadmin") {
		throw redirect(303, "/admin/dashboard");
	}

	try {
		const [users, navItems] = await Promise.all([
			listUsers(locals.session.accessToken),
			fetchNavItems(locals.session.accessToken)
		]);
		return { users, navItems };
	} catch (requestError) {
		if (requestError instanceof BackendApiError) {
			throw error(requestError.status || 500, requestError.message);
		}
		throw error(500, "Unable to load users.");
	}
};

function parsePermissions(formData: FormData) {
	const map = new Map<number, { navItemId: number; canView: boolean; canCreate: boolean; canUpdate: boolean; canDelete: boolean; needsApproval: boolean }>();
	for (const [key, value] of formData.entries()) {
		const match = key.match(/^perm_(\d+)_(canView|canCreate|canUpdate|canDelete|needsApproval)$/);
		if (match) {
			const navItemId = parseInt(match[1], 10);
			const field = match[2] as "canView" | "canCreate" | "canUpdate" | "canDelete" | "needsApproval";
			if (!map.has(navItemId)) {
				map.set(navItemId, { navItemId, canView: false, canCreate: false, canUpdate: false, canDelete: false, needsApproval: false });
			}
			map.get(navItemId)![field] = value === "on" || value === "true" || value === "1";
		}
	}
	return Array.from(map.values());
}

export const actions: Actions = {
	create: async ({ request, locals }) => {
		if (locals.session?.user.role !== "superadmin") {
			throw redirect(303, "/admin/dashboard");
		}

		const formData = await request.formData();
		const values = {
			fullName: formData.get("fullName")?.toString().trim() ?? "",
			email: formData.get("email")?.toString().trim() ?? "",
			department: formData.get("department")?.toString().trim() ?? "",
			password: formData.get("password")?.toString() ?? ""
		};

		if (!values.fullName || !values.email || !values.password) {
			return fail(400, { values, message: "Full name, email, and password are required." });
		}

		const permissions = parsePermissions(formData);

		try {
			const user = await createUser(locals.session.accessToken, {
				fullName: values.fullName,
				email: values.email,
				department: values.department,
				password: values.password,
				permissions
			});

			return {
				values: { fullName: "", email: "", department: values.department, password: "" },
				success: true,
				message: `Created ${user.fullName} successfully.`
			};
		} catch (requestError) {
			if (requestError instanceof BackendApiError) {
				return fail(requestError.status || 400, { values, message: requestError.message });
			}
			return fail(500, { values, message: "Unable to create the user right now." });
		}
	},

	update: async ({ request, locals }) => {
		if (locals.session?.user.role !== "superadmin") {
			throw redirect(303, "/admin/dashboard");
		}

		const formData = await request.formData();
		const userId = parseInt(formData.get("userId")?.toString() ?? "0", 10);
		const fullName = formData.get("fullName")?.toString().trim() ?? "";
		const department = formData.get("department")?.toString().trim() ?? "";
		const isActive = formData.get("isActive") === "true";

		if (!userId || !fullName) {
			return fail(400, { message: "Invalid request." });
		}

		const permissions = parsePermissions(formData);

		try {
			await updateUser(locals.session.accessToken, userId, { fullName, department, isActive });
			await updateUserPermissions(locals.session.accessToken, userId, permissions);
			return { success: true, message: "User updated successfully." };
		} catch (requestError) {
			if (requestError instanceof BackendApiError) {
				return fail(requestError.status || 400, { message: requestError.message });
			}
			return fail(500, { message: "Unable to update the user right now." });
		}
	},

	delete: async ({ request, locals }) => {
		if (locals.session?.user.role !== "superadmin") {
			throw redirect(303, "/admin/dashboard");
		}

		const formData = await request.formData();
		const userId = parseInt(formData.get("userId")?.toString() ?? "0", 10);

		if (!userId) {
			return fail(400, { message: "Invalid user ID." });
		}

		try {
			await deleteUser(locals.session.accessToken, userId);
			return { success: true, message: "User deleted successfully." };
		} catch (requestError) {
			if (requestError instanceof BackendApiError) {
				return fail(requestError.status || 400, { message: requestError.message });
			}
			return fail(500, { message: "Unable to delete the user right now." });
		}
	}
};
