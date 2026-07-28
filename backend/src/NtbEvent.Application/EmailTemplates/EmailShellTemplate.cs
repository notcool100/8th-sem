namespace NtbEvent.Application.EmailTemplates;

/// <summary>
/// Fixed chrome (gradient header, eyebrow, white title, white card body, gray footer)
/// wrapped around every outgoing invitation email. Deliberately kept out of
/// <see cref="EmailTemplateTypes.DefaultBodies"/> / the admin's WYSIWYG editor: Quill's
/// Delta document model has no Blot for table/div layout or gradients, so any HTML it
/// touches gets normalized down to plain paragraphs on the next load/save. Only the inner
/// content fragment is admin-editable; this shell is assembled at render time instead.
/// Table-based layout (rather than nested divs) is used for the card so Outlook's Word
/// rendering engine displays it correctly.
/// </summary>
public static class EmailShellTemplate
{
    /// <summary>
    /// Token-based template consumed by <see cref="Application.Services.EmailTemplateService"/>,
    /// which substitutes {{ShellTitle}}/{{ShellFooter}}/{{ShellContent}} after the inner content
    /// has already had its own placeholders resolved. The &lt;style&gt; block here is for
    /// authoring convenience only — PreMailer.Net inlines it into style="" attributes before
    /// the message is sent, so no literal &lt;style&gt; tag reaches the recipient's inbox.
    /// </summary>
    public const string Html = """
    <!doctype html>
    <html>
      <head>
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <style>
          .ntb-shell-body { margin: 0; padding: 24px 12px; background: #eef2f7; }
          .ntb-shell-card { max-width: 560px; margin: 0 auto; background: #ffffff; border: 1px solid #e2e8f0; border-radius: 16px; border-collapse: separate; overflow: hidden; }
          .ntb-shell-header { background: linear-gradient(135deg, #1c5c6d, #2a7f96); padding: 28px 28px 22px; }
          .ntb-shell-eyebrow { margin: 0; font-size: 12px; letter-spacing: 1.5px; text-transform: uppercase; color: #ffffff; opacity: .85; }
          .ntb-shell-title { margin: 10px 0 0; font-size: 22px; color: #ffffff; font-family: Inter, Arial, sans-serif; }
          .ntb-shell-content { padding: 28px; background: #ffffff; font-family: Inter, Arial, sans-serif; }
          .ntb-shell-footer { padding: 16px 28px; background: #f1f5f9; border-top: 1px solid #e2e8f0; text-align: center; }
          .ntb-shell-footer p { margin: 0; font-size: 12px; color: #94a3b8; font-family: Inter, Arial, sans-serif; }
        </style>
      </head>
      <body class="ntb-shell-body">
        <table role="presentation" width="100%" cellpadding="0" cellspacing="0" class="ntb-shell-card">
          <tr>
            <td class="ntb-shell-header">
              <p class="ntb-shell-eyebrow">Nepal Tourism Board &middot; Invitation</p>
              <h1 class="ntb-shell-title">{{ShellTitle}}</h1>
            </td>
          </tr>
          <tr>
            <td class="ntb-shell-content">
              {{ShellContent}}
            </td>
          </tr>
          <tr>
            <td class="ntb-shell-footer">
              <p>{{ShellFooter}}</p>
            </td>
          </tr>
        </table>
      </body>
    </html>
    """;

    public static string Wrap(string title, string footer, string contentHtml) =>
        Html.Replace("{{ShellTitle}}", title)
            .Replace("{{ShellFooter}}", footer)
            .Replace("{{ShellContent}}", contentHtml);
}
