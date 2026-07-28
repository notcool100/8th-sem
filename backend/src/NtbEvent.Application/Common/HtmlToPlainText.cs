using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace NtbEvent.Application.Common;

/// <summary>
/// Derives a plain-text email fallback from admin-authored HTML (Quill output), so the
/// WYSIWYG editor only has to maintain one body per template.
/// </summary>
public static partial class HtmlToPlainText
{
    public static string Convert(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var text = BlockBreakRegex().Replace(html, "\n");
        text = TagRegex().Replace(text, string.Empty);
        text = WebUtility.HtmlDecode(text);

        var lines = text
            .Replace("\r\n", "\n")
            .Split('\n')
            .Select(line => line.Trim());

        var builder = new StringBuilder();
        var blankRun = 0;
        foreach (var line in lines)
        {
            if (line.Length == 0)
            {
                blankRun++;
                if (blankRun > 1)
                {
                    continue;
                }
            }
            else
            {
                blankRun = 0;
            }

            builder.AppendLine(line);
        }

        return builder.ToString().Trim();
    }

    [GeneratedRegex("</(p|div|h1|h2|h3|h4|h5|h6|li|tr)>|<br\\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex BlockBreakRegex();

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex TagRegex();
}
