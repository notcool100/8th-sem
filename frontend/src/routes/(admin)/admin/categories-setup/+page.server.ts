import { error, fail } from "@sveltejs/kit";
import type { PageServerLoad, Actions } from "./$types";
import { BackendApiError, fetchCategories, createCategory, updateCategory, deleteCategory } from "$lib/server/auth/api";
import { can } from "$lib/utils/permissions";

export const load: PageServerLoad = async ({ locals }) => {
	if (!can(locals.session?.user, "categories", "canView")) {
		throw error(403, "You don't have permission to view Categories.");
	}

	try {
		const categories = await fetchCategories(locals.session!.accessToken);
		return {
			categories,
			canCreate: can(locals.session?.user, "categories", "canCreate"),
			canUpdate: can(locals.session?.user, "categories", "canUpdate"),
			canDelete: can(locals.session?.user, "categories", "canDelete")
		};
	} catch (requestError) {
		if (requestError instanceof BackendApiError) {
			throw error(requestError.status || 500, requestError.message);
		}

		throw error(500, "Unable to load categories.");
	}
};

export const actions: Actions = {
	create: async ({ request, locals }) => {
		if (!locals.session) return fail(401, { message: "Unauthorized." });
		if (!can(locals.session.user, "categories", "canCreate")) {
			return fail(403, { message: "You don't have permission to create categories." });
		}

		const formData = await request.formData();
		const name = formData.get("name")?.toString().trim() ?? "";
		const description = formData.get("description")?.toString().trim() ?? "";
		const color = formData.get("color")?.toString().trim() ?? "#d90766";
		const icon = formData.get("icon")?.toString().trim() ?? "😊";
		const tagsString = formData.get("tags")?.toString().trim() ?? "";
		const isHoliday = formData.get("isHoliday") === "true";
		const typeValue = formData.get("type")?.toString().trim() ?? "event";
		const type = typeValue === "festival" ? "festival" : "event";

		if (!name || !description) {
			return fail(400, { message: "Name and Description are required." });
		}

		const tag = tagsString
			.split(",")
			.map((t) => t.trim())
			.filter(Boolean);

		try {
			await createCategory(locals.session.accessToken, {
				name,
				description,
				color,
				icon,
				tag,
				isHoliday,
				type
			});
			return { success: true };
		} catch (requestError) {
			if (requestError instanceof BackendApiError) {
				return fail(requestError.status || 400, { message: requestError.message });
			}
			return fail(500, { message: "Unable to create category." });
		}
	},

	update: async ({ request, locals }) => {
		if (!locals.session) return fail(401, { message: "Unauthorized." });
		if (!can(locals.session.user, "categories", "canUpdate")) {
			return fail(403, { message: "You don't have permission to update categories." });
		}

		const formData = await request.formData();
		const idStr = formData.get("id")?.toString() ?? "";
		const id = parseInt(idStr, 10);
		const name = formData.get("name")?.toString().trim() ?? "";
		const description = formData.get("description")?.toString().trim() ?? "";
		const color = formData.get("color")?.toString().trim() ?? "#d90766";
		const icon = formData.get("icon")?.toString().trim() ?? "😊";
		const tagsString = formData.get("tags")?.toString().trim() ?? "";
		const isHoliday = formData.get("isHoliday") === "true";
		const typeValue = formData.get("type")?.toString().trim() ?? "event";
		const type = typeValue === "festival" ? "festival" : "event";

		if (isNaN(id) || !name || !description) {
			return fail(400, { message: "ID, Name, and Description are required." });
		}

		const tag = tagsString
			.split(",")
			.map((t) => t.trim())
			.filter(Boolean);

		try {
			await updateCategory(locals.session.accessToken, id, {
				id,
				name,
				description,
				color,
				icon,
				tag,
				isHoliday,
				type
			});
			return { success: true };
		} catch (requestError) {
			if (requestError instanceof BackendApiError) {
				return fail(requestError.status || 400, { message: requestError.message });
			}
			return fail(500, { message: "Unable to update category." });
		}
	},

	delete: async ({ request, locals }) => {
		if (!locals.session) return fail(401, { message: "Unauthorized." });
		if (!can(locals.session.user, "categories", "canDelete")) {
			return fail(403, { message: "You don't have permission to delete categories." });
		}

		const formData = await request.formData();
		const idStr = formData.get("id")?.toString() ?? "";
		const id = parseInt(idStr, 10);

		if (isNaN(id)) {
			return fail(400, { message: "Valid ID is required." });
		}

		try {
			await deleteCategory(locals.session.accessToken, id);
			return { success: true };
		} catch (requestError) {
			if (requestError instanceof BackendApiError) {
				return fail(requestError.status || 400, { message: requestError.message });
			}
			return fail(500, { message: "Unable to delete category." });
		}
	}
};
