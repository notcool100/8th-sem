using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Infrastructure.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NtbEvent.Infrastructure.Services;

/// <summary>
/// Sends email via the SendGrid HTTP API when configured; otherwise logs the
/// message so the flow is testable without real credentials. Used instead of
/// raw SMTP because the host's network blocks outbound SMTP ports.
/// </summary>
public sealed class SendGridEmailService : IEmailService
{
    private readonly SendGridOptions _options;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(IOptions<SendGridOptions> options, ILogger<SendGridEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
        {
            _logger.LogWarning(
                "SendGrid is not configured (SendGrid:ApiKey is empty). Email to {Email} with subject '{Subject}' was NOT sent.",
                message.ToEmail, message.Subject);
            return;
        }

        var client = new SendGridClient(_options.ApiKey);
        var from = new EmailAddress(_options.FromAddress, _options.FromName);
        var to = new EmailAddress(message.ToEmail, message.ToName);
        var plainText = message.PlainTextBody ?? StripHtml(message.HtmlBody);

        var mail = MailHelper.CreateSingleEmail(from, to, message.Subject, plainText, message.HtmlBody);

        if (message.InlineImagePng is { Length: > 0 } png)
        {
            mail.AddAttachment(new Attachment
            {
                Content = Convert.ToBase64String(png),
                Type = "image/png",
                Filename = message.InlineImageFileName,
                Disposition = "inline",
                ContentId = message.InlineImageContentId
            });
        }

        var response = await client.SendEmailAsync(mail, cancellationToken);
        if ((int)response.StatusCode >= 400)
        {
            var body = await response.Body.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "SendGrid request failed ({Status}) sending to {Email}: {Body}",
                response.StatusCode, message.ToEmail, body);
            throw new InvalidOperationException($"SendGrid request failed with status {response.StatusCode}: {body}");
        }

        _logger.LogInformation("Email sent to {Email} via SendGrid.", message.ToEmail);
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
