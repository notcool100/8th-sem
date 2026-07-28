using NtbEvent.Application.EmailTemplates.Dtos;

namespace NtbEvent.Application.Contracts.Services;

/// <summary>
/// Admin-editable invitation email content. Each <c>type</c> (see
/// <see cref="EmailTemplateTypes"/>) has exactly one template, auto-seeded with the
/// product's default copy the first time it's requested.
/// </summary>
public interface IEmailTemplateService
{
    Task<EmailTemplateDto> GetByTypeAsync(string type, CancellationToken cancellationToken = default);

    Task<EmailTemplateDto> UpdateAsync(string type, UpdateEmailTemplateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Token-substitutes the stored Subject/BodyHtml for <paramref name="type"/> using
    /// <paramref name="values"/> (keys without the surrounding <c>{{ }}</c>). Missing
    /// placeholders resolve to an empty string.
    /// </summary>
    /// <param name="subjectOverride">
    /// When set (non-blank), replaces the stored template's subject before substitution —
    /// e.g. a per-event custom invitation subject.
    /// </param>
    /// <param name="bodyHtmlOverride">
    /// When set (non-blank), replaces the stored template's body before substitution —
    /// e.g. a per-event custom invitation body.
    /// </param>
    Task<(string Subject, string HtmlBody)> RenderAsync(
        string type,
        IReadOnlyDictionary<string, string> values,
        string? subjectOverride = null,
        string? bodyHtmlOverride = null,
        CancellationToken cancellationToken = default);
}
