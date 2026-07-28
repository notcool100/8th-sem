import type { EmailTemplateType } from "$lib/types/email-templates";

// Mirrors backend/src/NtbEvent.Application/EmailTemplates/EmailShellTemplate.cs.
// Preview-only: the real email is assembled and CSS-inlined server-side at send time
// (EmailTemplateService.RenderAsync), this just keeps what the admin sees in sync with that.
const SHELL_TITLES: Record<EmailTemplateType, string> = {
	WorkshopInvite: "NTB-TikTok Content Creation Workshop",
	EventInvitation: "{{EventTitle}}"
};

const SHELL_FOOTERS: Record<EmailTemplateType, string> = {
	WorkshopInvite: "Nepal Tourism Board",
	EventInvitation: "This invitation is unique to you. Please do not share it."
};

export function shellTitleTemplate(type: EmailTemplateType): string {
	return SHELL_TITLES[type];
}

export function wrapEmailShell(type: EmailTemplateType, title: string, contentHtml: string): string {
	const footer = SHELL_FOOTERS[type];
	return `
<div style="font-family:Inter,Arial,sans-serif;max-width:560px;margin:0 auto;background:#ffffff;border:1px solid #e2e8f0;border-radius:16px;overflow:hidden;">
  <div style="background:linear-gradient(135deg,#1c5c6d,#2a7f96);padding:28px 28px 22px;color:#ffffff;">
    <p style="margin:0;font-size:12px;letter-spacing:1.5px;text-transform:uppercase;opacity:.85;">Nepal Tourism Board &middot; Invitation</p>
    <h1 style="margin:10px 0 0;font-size:22px;">${title}</h1>
  </div>
  <div style="padding:28px;">${contentHtml}</div>
  <div style="padding:16px 28px;background:#f1f5f9;border-top:1px solid #e2e8f0;text-align:center;">
    <p style="margin:0;font-size:12px;color:#94a3b8;">${footer}</p>
  </div>
</div>`.trim();
}
