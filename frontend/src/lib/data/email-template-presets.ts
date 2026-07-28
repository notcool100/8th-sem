import type { EmailTemplateType } from "$lib/types/email-templates";

export type EmailTemplatePreset = {
	id: string;
	name: string;
	description: string;
	subject: string;
	bodyHtml: string;
};

// Starter designs an admin can drop into the editor and then customize. Each one is the
// INNER content fragment only — the gradient header/eyebrow/title and gray footer are fixed
// shell chrome (see backend EmailShellTemplate.cs / frontend email-shell.ts) assembled around
// whatever is here, so these must only use the placeholder tokens declared for their
// EmailTemplateType in the backend's EmailTemplateTypes.Placeholders.
export const EMAIL_TEMPLATE_PRESETS: Record<EmailTemplateType, EmailTemplatePreset[]> = {
	WorkshopInvite: [
		{
			id: "workshop-classic",
			name: "Classic",
			description: "The default wording with a boxed QR panel.",
			subject: "Invitation To NTB-TikTok Content Creation Workshop",
			bodyHtml: `
<p style="font-size:15px;color:#0f172a;margin:0 0 14px;">Dear {{FullName}},</p>
<p style="font-size:14px;color:#334155;line-height:1.6;margin:0 0 14px;">
  Congratulations! You have been selected to participate in this workshop, a joint initiative of Nepal Tourism Board and TikTok.
</p>
<p style="font-size:14px;color:#334155;line-height:1.6;margin:0 0 18px;">
  You are kindly requested to attend the workshop scheduled for <strong>{{WorkshopDate}}</strong>.
</p>
<div style="text-align:center;background:#f8fafc;border:1px solid #e2e8f0;border-radius:12px;padding:22px;">
  <p style="margin:0 0 12px;font-size:13px;color:#475569;font-weight:600;">Show this QR code at the entrance</p>
  {{QrCodeImage}}
</div>
{{InviteUrlLine}}`.trim()
		},
		{
			id: "workshop-minimal",
			name: "Minimal",
			description: "Shorter copy, QR code shown without a boxed background.",
			subject: "You're invited: a workshop with Nepal Tourism Board",
			bodyHtml: `
<p style="font-size:14px;color:#334155;line-height:1.7;margin:0 0 14px;">Hi {{FullName}},</p>
<p style="font-size:14px;color:#334155;line-height:1.7;margin:0 0 14px;">
  You've been selected to join our upcoming workshop on <strong>{{WorkshopDate}}</strong>. We'd love to see you there.
</p>
<div style="text-align:center;padding:8px 0 0;">
  {{QrCodeImage}}
  <p style="margin:14px 0 0;font-size:12px;color:#94a3b8;">Bring this QR code to check in at the door.</p>
</div>
{{InviteUrlLine}}`.trim()
		},
		{
			id: "workshop-bold-cta",
			name: "Bold CTA",
			description: "Warmer wording with a red-accented QR panel.",
			subject: "You're selected — NTB Content Creation Workshop",
			bodyHtml: `
<p style="font-size:15px;color:#0f172a;margin:0 0 14px;">Dear {{FullName}},</p>
<p style="font-size:14px;color:#334155;line-height:1.6;margin:0 0 18px;">
  Congratulations on being selected! Please join us on <strong>{{WorkshopDate}}</strong> — we can't wait to see what you create.
</p>
<div style="text-align:center;background:#fff2f3;border:1px solid #fecdd3;border-radius:12px;padding:22px;">
  <p style="margin:0 0 12px;font-size:13px;color:#9f1733;font-weight:600;">Show this QR code at the entrance</p>
  {{QrCodeImage}}
</div>
{{InviteUrlLine}}`.trim()
		}
	],
	EventInvitation: [
		{
			id: "event-classic",
			name: "Classic",
			description: "The default wording with a date/location table and boxed QR panel.",
			subject: "You're invited: {{EventTitle}}",
			bodyHtml: `
<p style="font-size:15px;color:#0f172a;margin:0 0 14px;">Dear {{FullName}},</p>
<p style="font-size:14px;color:#334155;line-height:1.6;margin:0 0 18px;">
  You are cordially invited to <strong>{{EventTitle}}</strong>. {{EventSummary}}
</p>
<table style="font-size:14px;color:#334155;margin:0 0 22px;">
  <tr><td style="padding:3px 12px 3px 0;color:#64748b;">Date</td><td>{{EventDate}}</td></tr>
  <tr><td style="padding:3px 12px 3px 0;color:#64748b;">Location</td><td>{{EventLocation}}</td></tr>
</table>
<div style="text-align:center;background:#f8fafc;border:1px solid #e2e8f0;border-radius:12px;padding:22px;">
  <p style="margin:0 0 12px;font-size:13px;color:#475569;font-weight:600;">Show this QR code at the entrance</p>
  {{QrCodeImage}}
  {{ExpiryLine}}
</div>
<p style="text-align:center;margin:22px 0 6px;">
  <a href="{{InviteUrl}}" style="display:inline-block;background:#1c5c6d;color:#ffffff;text-decoration:none;font-size:14px;font-weight:600;padding:12px 26px;border-radius:10px;">View your invitation</a>
</p>
<p style="text-align:center;font-size:12px;color:#94a3b8;margin:8px 0 0;">Or open: {{InviteUrl}}</p>`.trim()
		},
		{
			id: "event-minimal",
			name: "Minimal",
			description: "Shorter copy, QR code and CTA shown without a boxed background.",
			subject: "{{EventTitle}} — you're on the list",
			bodyHtml: `
<p style="font-size:14px;color:#334155;line-height:1.7;margin:0 0 6px;">Hi {{FullName}}, {{EventSummary}}</p>
<p style="font-size:13px;color:#64748b;margin:0 0 18px;">{{EventDate}} &middot; {{EventLocation}}</p>
<div style="text-align:center;">
  {{QrCodeImage}}
  {{ExpiryLine}}
  <p style="margin:18px 0 0;">
    <a href="{{InviteUrl}}" style="display:inline-block;background:#10243e;color:#ffffff;text-decoration:none;font-size:14px;font-weight:600;padding:12px 26px;border-radius:10px;">View invitation</a>
  </p>
</div>`.trim()
		},
		{
			id: "event-bold-cta",
			name: "Bold CTA",
			description: "Date/location table with a red-accented QR panel and CTA button.",
			subject: "You're invited: {{EventTitle}}",
			bodyHtml: `
<p style="font-size:15px;color:#0f172a;margin:0 0 14px;">Dear {{FullName}},</p>
<p style="font-size:14px;color:#334155;line-height:1.6;margin:0 0 18px;">{{EventSummary}}</p>
<table style="font-size:14px;color:#334155;margin:0 0 22px;">
  <tr><td style="padding:3px 12px 3px 0;color:#64748b;">Date</td><td>{{EventDate}}</td></tr>
  <tr><td style="padding:3px 12px 3px 0;color:#64748b;">Location</td><td>{{EventLocation}}</td></tr>
</table>
<div style="text-align:center;background:#fff2f3;border:1px solid #fecdd3;border-radius:12px;padding:22px;">
  <p style="margin:0 0 12px;font-size:13px;color:#9f1733;font-weight:600;">Show this QR code at the entrance</p>
  {{QrCodeImage}}
  {{ExpiryLine}}
</div>
<p style="text-align:center;margin:22px 0 6px;">
  <a href="{{InviteUrl}}" style="display:inline-block;background:#c8102e;color:#ffffff;text-decoration:none;font-size:14px;font-weight:600;padding:12px 26px;border-radius:10px;">View your invitation</a>
</p>`.trim()
		}
	]
};
