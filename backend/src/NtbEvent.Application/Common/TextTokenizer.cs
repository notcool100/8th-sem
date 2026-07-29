using System.Text.RegularExpressions;

namespace NtbEvent.Application.Common;

/// <summary>Shared lowercase/word-splitting/stop-word tokenizer used by the TF-IDF, BM25, and tag-suggestion services.</summary>
public static class TextTokenizer
{
    private static readonly Regex WordPattern = new(@"[a-zA-Z0-9]+", RegexOptions.Compiled);

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "and", "are", "as", "at", "be", "been", "being", "but", "by",
        "can", "could", "did", "do", "does", "doing", "down", "during",
        "each", "few", "for", "from", "further",
        "had", "has", "have", "having", "he", "her", "here", "hers", "herself", "him", "himself", "his", "how",
        "i", "if", "in", "into", "is", "it", "its", "itself",
        "just", "me", "more", "most", "my", "myself",
        "no", "nor", "not", "now", "of", "off", "on", "once", "only", "or", "other", "our", "ours", "ourselves", "out", "over", "own",
        "s", "same", "she", "should", "so", "some", "such",
        "t", "than", "that", "the", "their", "theirs", "them", "themselves", "then", "there", "these", "they", "this", "those", "through", "to", "too",
        "under", "until", "up",
        "very", "was", "we", "were", "what", "when", "where", "which", "while", "who", "whom", "why", "will", "with", "would",
        "you", "your", "yours", "yourself", "yourselves"
    };

    /// <summary>Lowercases, splits on non-alphanumeric boundaries, and strips stop words.</summary>
    public static List<string> Tokenize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return WordPattern.Matches(text.ToLowerInvariant())
            .Select(m => m.Value)
            .Where(token => token.Length > 1 && !StopWords.Contains(token))
            .ToList();
    }
}
