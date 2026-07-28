namespace NtbEvent.Infrastructure.Configuration;

/// <summary>
/// SendGrid HTTP API settings for sending email. Used instead of raw SMTP because
/// the production host's network blocks outbound SMTP ports (25/465/587) — the
/// SendGrid API is reached over HTTPS, which is unaffected.
/// </summary>
public sealed class SendGridOptions
{
    public const string SectionName = "SendGrid";

    public string ApiKey { get; init; } = string.Empty;

    public string FromAddress { get; init; } = "no-reply@ntb.gov.np";

    public string FromName { get; init; } = "Nepal Tourism Board";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}
