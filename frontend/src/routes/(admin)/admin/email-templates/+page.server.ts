import { error, fail, redirect } from "@sveltejs/kit";
import type { Actions, PageServerLoad } from "./$types";
import { BackendApiError, getEmailTemplate, updateEmailTemplate } from "$lib/server/auth/api";
import { can } from "$lib/utils/permissions";
import type { EmailTemplateDto, EmailTemplateType } from "$lib/types/email-templates";

const TEMPLATE_TYPES: EmailTemplateType[] = ["WorkshopInvite", "EventInvitation"];

export const prerender = false;

export const load: PageServerLoad = async ({ locals }) => {
	if (!locals.session) throw redirect(303, "/login");

	if (!can(locals.session.user, "email_templates", "canView")) {
		throw error(403, "You don't have permission to view email templates.");
	}

	const templates: Record<string, EmailTemplateDto> = {};
	for (const type of TEMPLATE_TYPES) {
		templates[type] = await getEmailTemplate(locals.session.accessToken, type);
	}

	return {
		templates,
		canUpdate: can(locals.session.user, "email_templates", "canUpdate")
	};
};

export const actions: Actions = {
	save: async ({ request, locals }) => {
		if (!locals.session) {
			throw redirect(303, "/login");
		}

		if (!can(locals.session.user, "email_templates", "canUpdate")) {
			return fail(403, { message: "You don't have permission to update email templates." });
		}

		const formData = await request.formData();
		const type = formData.get("type")?.toString() as EmailTemplateType;
		const subject = formData.get("subject")?.toString().trim() ?? "";
		const bodyHtml = formData.get("bodyHtml")?.toString() ?? "";

		if (!TEMPLATE_TYPES.includes(type)) {
			return fail(400, { message: "Unknown template type." });
		}

		if (!subject || !bodyHtml) {
			return fail(400, { message: "Subject and body are both required.", type, subject, bodyHtml });
		}

		try {
			await updateEmailTemplate(locals.session.accessToken, type, { subject, bodyHtml });
			return { success: true, type, message: "Email template saved.", subject, bodyHtml };
		} catch (requestError) {
			if (requestError instanceof BackendApiError) {
				return fail(requestError.status || 400, { message: requestError.message, type, subject, bodyHtml });
			}

			return fail(500, { message: "Unable to save the email template right now.", type, subject, bodyHtml });
		}
	}
};
