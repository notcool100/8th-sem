// Type strings must match NtbEvent.Application.EmailTemplates.EmailTemplateTypes exactly
// (they're used as-is in the API route, e.g. /api/email-templates/WorkshopInvite).
export type EmailTemplateType = "WorkshopInvite" | "EventInvitation";

export type EmailTemplateDto = {
	type: EmailTemplateType | string;
	subject: string;
	bodyHtml: string;
	availablePlaceholders: string[];
	updatedAtUtc: string;
};

export type UpdateEmailTemplatePayload = {
	subject: string;
	bodyHtml: string;
};
