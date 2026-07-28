namespace NtbEvent.Application.EmailTemplates;

/// <summary>
/// Known email template types, their available placeholder tokens, and the default
/// copy seeded the first time each type is requested (kept identical to the wording
/// that used to be hardcoded in <c>WorkshopInviteService</c>/<c>InvitationService</c>,
/// so introducing templates doesn't change anything an admin hasn't edited yet).
/// </summary>
public static class EmailTemplateTypes
{
    public const string WorkshopInvite = "WorkshopInvite";

    public const string EventInvitation = "EventInvitation";

    public static readonly IReadOnlyDictionary<string, string[]> Placeholders = new Dictionary<string, string[]>
    {
        [WorkshopInvite] = ["FullName", "WorkshopDate", "InviteUrlLine", "QrCodeImage"],
        [EventInvitation] = ["FullName", "EventTitle", "EventSummary", "EventDate", "EventLocation", "InviteUrl", "ExpiryLine", "QrCodeImage"]
    };

    public static readonly IReadOnlyDictionary<string, string> DefaultSubjects = new Dictionary<string, string>
    {
        [WorkshopInvite] = "Invitation To NTB-TikTok Content Creation Workshop",
        [EventInvitation] = "You're invited: {{EventTitle}}"
    };

    /// <summary>
    /// Title shown in the email shell's header (see <see cref="EmailShellTemplate"/>), resolved
    /// with the same token substitution as <see cref="DefaultBodies"/>. Not admin-editable —
    /// the header is fixed chrome, not part of the WYSIWYG-edited body.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> ShellTitles = new Dictionary<string, string>
    {
        [WorkshopInvite] = "NTB-TikTok Content Creation Workshop",
        [EventInvitation] = "{{EventTitle}}"
    };

    /// <summary>
    /// Text shown in the email shell's footer. Not admin-editable, same reasoning as <see cref="ShellTitles"/>.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> ShellFooters = new Dictionary<string, string>
    {
        [WorkshopInvite] = "Nepal Tourism Board",
        [EventInvitation] = "This invitation is unique to you. Please do not share it."
    };

    /// <summary>
    /// Inner content only — the gradient header/eyebrow/title and gray footer live in
    /// <see cref="EmailShellTemplate"/> and are assembled around this at render time.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> DefaultBodies = new Dictionary<string, string>
    {
        [WorkshopInvite] = """
        <p style="font-size:15px;color:#0f172a;margin:0 0 14px;">Dear {{FullName}},</p>
        <p style="font-size:14px;color:#334155;line-height:1.6;margin:0 0 14px;">
          Congratulations! You have been selected to participate in the Travel Content Creation Workshop for Tourism, a joint initiative of Nepal Tourism Board and TikTok.
        </p>
        <p style="font-size:14px;color:#334155;line-height:1.6;margin:0 0 14px;">
          You are kindly requested to attend the workshop scheduled for <strong>{{WorkshopDate}}</strong>.
        </p>
        <p style="font-size:14px;color:#334155;line-height:1.6;margin:0 0 18px;">
          We would appreciate your confirmation of participation via email at your earliest convenience.
        </p>
        <div style="text-align:center;background:#f8fafc;border:1px solid #e2e8f0;border-radius:12px;padding:22px;">
          <p style="margin:0 0 12px;font-size:13px;color:#475569;font-weight:600;">Show this QR code at the entrance</p>
          {{QrCodeImage}}
        </div>
        {{InviteUrlLine}}
        """,
        [EventInvitation] = """
        <p style="font-size:15px;color:#0f172a;margin:0 0 14px;">Dear {{FullName}},</p>
        <p style="font-size:14px;color:#334155;line-height:1.6;margin:0 0 18px;">
          You are cordially invited to <strong>{{EventTitle}}</strong>.
          {{EventSummary}}
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
        <p style="text-align:center;font-size:12px;color:#94a3b8;margin:8px 0 0;">Or open: {{InviteUrl}}</p>
        """
    };
}
