namespace NtbEvent.Infrastructure.Configuration;

/// <summary>
/// SMTP settings for sending invitation emails. When <see cref="Host"/> is empty
/// the email service falls back to logging the message instead of sending.
/// For Gmail use Host=smtp.gmail.com, Port=587, EnableSsl=true and an App Password.
/// </summary>
public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; init; } = string.Empty;

    public int Port { get; init; } = 587;

    public bool EnableSsl { get; init; } = true;

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string FromAddress { get; init; } = "no-reply@ntb.gov.np";

    public string FromName { get; init; } = "Nepal Tourism Board";

    /// <summary>
    /// Host for appending a copy of each sent email to the mailbox's Sent folder via IMAP,
    /// so it shows up in webmail (plain SMTP submission never writes there). Defaults to
    /// <see cref="Host"/> since most providers, including cPanel/Dovecot, serve SMTP and IMAP
    /// from the same hostname.
    /// </summary>
    public string ImapHost { get; init; } = string.Empty;

    public int ImapPort { get; init; } = 993;

    public bool ImapEnableSsl { get; init; } = true;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);
}
