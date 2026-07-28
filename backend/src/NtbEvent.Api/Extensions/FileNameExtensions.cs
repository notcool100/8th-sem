using System.Text;

namespace NtbEvent.Api.Extensions;

public static class FileNameExtensions
{
    /// <summary>Converts arbitrary text (e.g. an event title) into a safe download filename segment.</summary>
    public static string ToSafeFileNameSegment(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "event";
        }

        var builder = new StringBuilder();
        var lastWasDash = false;
        foreach (var c in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(c);
                lastWasDash = false;
            }
            else if (!lastWasDash)
            {
                builder.Append('-');
                lastWasDash = true;
            }
        }

        var result = builder.ToString().Trim('-');
        return result.Length == 0 ? "event" : result;
    }
}
