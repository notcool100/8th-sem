namespace NtbEvent.Application.EmailTemplates.Dtos;

public sealed class UpdateEmailTemplateRequest
{
    public string Subject { get; set; } = string.Empty;

    public string BodyHtml { get; set; } = string.Empty;
}
