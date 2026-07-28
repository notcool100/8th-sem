namespace NtbEvent.Application.Contracts.Services;

/// <summary>
/// Abstraction over outbound email. The infrastructure implementation sends via
/// SMTP when configured and otherwise logs the message (dev fallback).
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an HTML email with an optional inline image (referenced in the HTML
    /// via cid:<paramref name="inlineImageContentId"/>) plus the same image as a
    /// downloadable attachment.
    /// </summary>
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

public sealed class EmailMessage
{
    public string ToEmail { get; set; } = string.Empty;

    public string ToName { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string HtmlBody { get; set; } = string.Empty;

    public string? PlainTextBody { get; set; }

    /// <summary>Optional PNG bytes embedded inline (CID) and attached.</summary>
    public byte[]? InlineImagePng { get; set; }

    /// <summary>Content-ID used to reference the inline image in the HTML body.</summary>
    public string InlineImageContentId { get; set; } = "qr-code";

    /// <summary>File name used for the attached image.</summary>
    public string InlineImageFileName { get; set; } = "invitation-qr.png";
}
