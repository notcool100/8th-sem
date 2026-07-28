import { fail } from "@sveltejs/kit";
import type { Actions, PageServerLoad } from "./$types";
import { BackendApiError, getPublicEventBySlug, registerForEvent } from "$lib/server/auth/api";
import type { RegisterGuestPayload } from "$lib/types/registrations";
import { ALL_SELF_REGISTRATION_FIELDS } from "$lib/config/selfRegistrationFields";

export const load: PageServerLoad = async ({ params }) => {
	try {
		const event = await getPublicEventBySlug(params.slug);
		return { event, notFound: false };
	} catch (error) {
		if (error instanceof BackendApiError && error.status === 404) {
			return { event: null, notFound: true };
		}
		return { event: null, notFound: true };
	}
};

/** Fields captured into dedicated Guest columns rather than the additionalFields bag. */
const DEDICATED_FIELD_KEYS = new Set(["fullName", "email", "mobile", "orgName"]);

function collectFormValues(formData: FormData): Record<string, string | string[] | boolean> {
	const values: Record<string, string | string[] | boolean> = {
		fullName: (formData.get("fullName")?.toString() ?? "").trim(),
		email: (formData.get("email")?.toString() ?? "").trim()
	};

	for (const field of ALL_SELF_REGISTRATION_FIELDS) {
		if (field.locked) continue;
		if (field.type === "checkboxGroup") {
			values[field.key] = formData.getAll(field.key).map((v) => v.toString());
		} else if (field.type === "checkbox") {
			values[field.key] = formData.get(field.key) === "on";
		} else {
			values[field.key] = (formData.get(field.key)?.toString() ?? "").trim();
		}
	}

	return values;
}

export const actions: Actions = {
	register: async ({ request, params }) => {
		const formData = await request.formData();
		const fullName = (formData.get("fullName")?.toString() ?? "").trim();
		const email = (formData.get("email")?.toString() ?? "").trim();

		if (!fullName || !email) {
			return fail(400, { message: "Full name and email are required.", values: collectFormValues(formData) });
		}

		try {
			const event = await getPublicEventBySlug(params.slug);
			const enabledKeys = new Set(event.selfRegistrationFields ?? []);

			const phone = (formData.get("mobile")?.toString() ?? "").trim();
			const organization = (formData.get("orgName")?.toString() ?? "").trim();

			const additionalFields: Record<string, string> = {};
			for (const field of ALL_SELF_REGISTRATION_FIELDS) {
				if (field.locked || DEDICATED_FIELD_KEYS.has(field.key) || !enabledKeys.has(field.key)) {
					continue;
				}

				if (field.type === "checkboxGroup") {
					const selected = formData.getAll(field.key).map((v) => v.toString());
					if (selected.length) additionalFields[field.key] = selected.join(", ");
				} else if (field.type === "checkbox") {
					additionalFields[field.key] = formData.get(field.key) === "on" ? "Yes" : "No";
				} else {
					const value = (formData.get(field.key)?.toString() ?? "").trim();
					if (value) additionalFields[field.key] = value;
				}
			}

			const payload: RegisterGuestPayload = {
				fullName,
				email,
				phone: phone || undefined,
				organization: organization || undefined,
				additionalFields
			};

			await registerForEvent(event.id, payload);
			return { success: true, message: "Your registration has been submitted and is awaiting admin approval." };
		} catch (error) {
			if (error instanceof BackendApiError) {
				return fail(error.status || 400, {
					message: error.message,
					values: collectFormValues(formData)
				});
			}
			return fail(500, { message: "Unable to submit your registration right now.", values: collectFormValues(formData) });
		}
	}
};

export const prerender = false;
