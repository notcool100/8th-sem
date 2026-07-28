using NtbEvent.Domain.Common;

namespace NtbEvent.Domain.Entities;

/// <summary>
/// Admin-editable content for an invitation email. One row per <see cref="Type"/>
/// (e.g. "WorkshopInvite", "EventInvitation") — <see cref="BodyHtml"/> is authored via a
/// WYSIWYG editor and contains <c>{{Token}}</c> placeholders resolved at send time.
/// The plain-text fallback sent alongside the HTML body is derived from
/// <see cref="BodyHtml"/> rather than stored separately.
/// </summary>
public sealed class EmailTemplate : BaseEntity
{
    public string Type { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string BodyHtml { get; set; } = string.Empty;
}
