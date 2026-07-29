namespace NtbEvent.Application.Tags.Dtos;

public sealed class SuggestedTagDto
{
    public string Tag { get; set; } = string.Empty;

    /// <summary>True when this suggestion fuzzy-matched an existing tag (via Levenshtein distance) rather than being a brand-new keyword.</summary>
    public bool IsExistingTag { get; set; }

    /// <summary>Relative confidence, 0-1, derived from the TF-IDF weight of the extracted keyword.</summary>
    public double Score { get; set; }
}
