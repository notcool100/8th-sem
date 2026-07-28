using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Infrastructure.Configuration;

namespace NtbEvent.Infrastructure.Services;

/// <summary>
/// Sends email via SMTP when configured; otherwise logs the message so the flow
/// is testable in development without real credentials. Uses MailKit instead of
/// System.Net.Mail because the latter doesn't support implicit TLS (port 465),
/// which is what most cPanel/Exim hosts expose.
/// </summary>
public sealed class SmtpEmailService : IEmailService
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<SmtpOptions> options, ILogger<SmtpEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
        {
            _logger.LogWarning(
                "SMTP is not configured (Smtp:Host is empty). Email to {Email} with subject '{Subject}' was NOT sent. " +
                "Configure the Smtp section to enable real delivery.",
                message.ToEmail, message.Subject);
            return;
        }

        var mail = new MimeMessage();
        mail.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        mail.To.Add(new MailboxAddress(message.ToName, message.ToEmail));
        mail.Subject = message.Subject;

        var bodyBuilder = new BodyBuilder
        {
            TextBody = message.PlainTextBody ?? StripHtml(message.HtmlBody),
            HtmlBody = message.HtmlBody
        };

        if (message.InlineImagePng is { Length: > 0 } png)
        {
            var inline = bodyBuilder.LinkedResources.Add(message.InlineImageFileName, png, new ContentType("image", "png"));
            inline.ContentId = message.InlineImageContentId;
            bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace(
                $"cid:{message.InlineImageContentId}", $"cid:{inline.ContentId}");

            // Also attach the QR so the guest can save it.
            bodyBuilder.Attachments.Add(message.InlineImageFileName, png, new ContentType("image", "png"));
        }

        mail.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            var socketOptions = _options.Port == 465
                ? SecureSocketOptions.SslOnConnect
                : _options.EnableSsl
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.None;

            await client.ConnectAsync(_options.Host, _options.Port, socketOptions, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
            }

            await client.SendAsync(mail, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
            _logger.LogInformation("Invitation email sent to {Email}.", message.ToEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send invitation email to {Email}.", message.ToEmail);
            throw;
        }

        // Best-effort only: plain SMTP submission never writes to the mailbox's Sent folder
        // (that's normally done by the IMAP client you compose mail in), so webmail shows
        // nothing even though delivery succeeded. Append a copy ourselves so it does. Failure
        // here must never fail the request — the email has already been delivered.
        await TryAppendToSentFolderAsync(mail, cancellationToken);
    }

    private async Task TryAppendToSentFolderAsync(MimeMessage mail, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Username))
        {
            return;
        }

        try
        {
            using var imap = new ImapClient();
            var imapHost = string.IsNullOrWhiteSpace(_options.ImapHost) ? _options.Host : _options.ImapHost;
            var socketOptions = _options.ImapEnableSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;

            _logger.LogInformation("Connecting to IMAP {Host}:{Port} as {Username} to sync Sent folder.", imapHost, _options.ImapPort, _options.Username);
            await imap.ConnectAsync(imapHost, _options.ImapPort, socketOptions, cancellationToken);
            await imap.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);

            var (sentFolder, availableFolders) = await FindSentFolderAsync(imap, cancellationToken);
            if (sentFolder is null)
            {
                _logger.LogWarning(
                    "Could not locate a Sent folder via IMAP for {Username}; skipping Sent-folder sync. Folders seen on the server: {Folders}",
                    _options.Username, string.Join(", ", availableFolders));
            }
            else
            {
                await sentFolder.OpenAsync(FolderAccess.ReadWrite, cancellationToken);
                await sentFolder.AppendAsync(mail, MessageFlags.Seen, cancellationToken);
                _logger.LogInformation("Appended sent invitation email to IMAP folder '{Folder}' for {Username}.", sentFolder.FullName, _options.Username);
            }

            await imap.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to append sent invitation email to the IMAP Sent folder for {Email}. The email was still delivered via SMTP.", mail.To);
        }
    }

    private static async Task<(IMailFolder? Folder, IReadOnlyList<string> AvailableFolders)> FindSentFolderAsync(ImapClient imap, CancellationToken cancellationToken)
    {
        try
        {
            var special = imap.GetFolder(SpecialFolder.Sent);
            if (special is not null)
            {
                return (special, Array.Empty<string>());
            }
        }
        catch (NotSupportedException)
        {
            // Server doesn't advertise the SPECIAL-USE extension; fall back to name matching below.
        }

        var candidateNames = new[] { "Sent", "Sent Items", "Sent Messages", "INBOX.Sent" };
        var personal = imap.GetFolder(imap.PersonalNamespaces[0]);
        var subfolders = (await personal.GetSubfoldersAsync(false, cancellationToken: cancellationToken)).ToList();
        var availableFolders = subfolders.Select(f => f.FullName).ToList();

        foreach (var name in candidateNames)
        {
            var match = subfolders.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                return (match, availableFolders);
            }
        }

        return (null, availableFolders);
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
        {
            return string.Empty;
        }

        var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ");
        text = System.Net.WebUtility.HtmlDecode(text);
        return System.Text.RegularExpressions.Regex.Replace(text, "\\s+", " ").Trim();
    }
}
