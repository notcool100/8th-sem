namespace NtbEvent.Application.EmailTemplates.Dtos;

public sealed class EmailTemplateDto
{
    public string Type { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string BodyHtml { get; set; } = string.Empty;

    public IReadOnlyList<string> AvailablePlaceholders { get; set; } = Array.Empty<string>();

    public DateTime UpdatedAtUtc { get; set; }
}
