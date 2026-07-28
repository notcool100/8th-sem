using System.Text.RegularExpressions;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.EmailTemplates;
using NtbEvent.Application.EmailTemplates.Dtos;
using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Services;

public sealed class EmailTemplateService : IEmailTemplateService
{
    private readonly IEmailTemplateRepository _repository;

    public EmailTemplateService(IEmailTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<EmailTemplateDto> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        var template = await GetOrSeedAsync(type, cancellationToken);
        return Map(template);
    }

    public async Task<EmailTemplateDto> UpdateAsync(string type, UpdateEmailTemplateRequest request, CancellationToken cancellationToken = default)
    {
        ValidateType(type);

        var template = await GetOrSeedAsync(type, cancellationToken);
        template.Subject = request.Subject.Trim();
        template.BodyHtml = request.BodyHtml;
        template.UpdatedAtUtc = DateTime.UtcNow;

        await _repository.UpdateAsync(template, cancellationToken);
        return Map(template);
    }

    public async Task<(string Subject, string HtmlBody)> RenderAsync(
        string type,
        IReadOnlyDictionary<string, string> values,
        string? subjectOverride = null,
        string? bodyHtmlOverride = null,
        CancellationToken cancellationToken = default)
    {
        var template = await GetOrSeedAsync(type, cancellationToken);

        var subjectSource = string.IsNullOrWhiteSpace(subjectOverride) ? template.Subject : subjectOverride;
        var bodySource = string.IsNullOrWhiteSpace(bodyHtmlOverride) ? template.BodyHtml : bodyHtmlOverride;

        var content = Substitute(bodySource, values);
        var title = Substitute(EmailTemplateTypes.ShellTitles[type], values);
        var footer = Substitute(EmailTemplateTypes.ShellFooters[type], values);

        var wrapped = EmailShellTemplate.Wrap(title, footer, content);
        var inlined = PreMailer.Net.PreMailer.MoveCssInline(
            wrapped,
            removeStyleElements: true,
            stripIdAndClassAttributes: true).Html;

        return (Substitute(subjectSource, values), inlined);
    }

    private async Task<EmailTemplate> GetOrSeedAsync(string type, CancellationToken cancellationToken)
    {
        ValidateType(type);

        var template = await _repository.GetByTypeAsync(type, cancellationToken);
        if (template is not null)
        {
            return template;
        }

        var now = DateTime.UtcNow;
        var seeded = new EmailTemplate
        {
            Type = type,
            Subject = EmailTemplateTypes.DefaultSubjects[type],
            BodyHtml = EmailTemplateTypes.DefaultBodies[type],
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        await _repository.AddAsync(seeded, cancellationToken);
        return seeded;
    }

    private static void ValidateType(string type)
    {
        if (!EmailTemplateTypes.Placeholders.ContainsKey(type))
        {
            throw new ArgumentException($"Unknown email template type '{type}'.", nameof(type));
        }
    }

    private static string Substitute(string source, IReadOnlyDictionary<string, string> values)
    {
        var result = source;
        foreach (var (key, value) in values)
        {
            result = result.Replace($"{{{{{key}}}}}", value);
        }

        // Drop any unrecognized/typo'd tokens rather than leaking raw "{{...}}" text.
        return Regex.Replace(result, "{{\\s*[A-Za-z]+\\s*}}", string.Empty);
    }

    private static EmailTemplateDto Map(EmailTemplate template) => new()
    {
        Type = template.Type,
        Subject = template.Subject,
        BodyHtml = template.BodyHtml,
        AvailablePlaceholders = EmailTemplateTypes.Placeholders[template.Type],
        UpdatedAtUtc = template.UpdatedAtUtc
    };
}
