using NtbEvent.Application.Common;

namespace NtbEvent.Tests.Unit.AI;

/// <summary>Mid-term report §4.2.1 UT-07..UT-09 (AI - TF-IDF / Cosine Similarity).</summary>
public sealed class TfIdfVectorizerTests
{
    // UT-07: Compute TF-IDF for a single document -> correct term weights returned.
    [Fact]
    public void ComputeTfIdf_SingleDocument_TermsWeightedByFrequency()
    {
        var documents = new List<IReadOnlyList<string>>
        {
            new List<string> { "festival", "festival", "food", "kathmandu" }
        };

        var vectors = TfIdfVectorizer.ComputeTfIdf(documents);

        var vector = vectors[0];
        Assert.Equal(3, vector.Count);
        // "festival" appears twice, so its weight must be double that of a single-occurrence term.
        Assert.Equal(2 * vector["food"], vector["festival"], precision: 6);
        Assert.Equal(vector["food"], vector["kathmandu"], precision: 6);
        Assert.True(vector.Values.All(w => w > 0));
    }

    // UT-08: Identical documents -> similarity score = 1.0.
    [Fact]
    public void CosineSimilarity_IdenticalDocuments_ReturnsOne()
    {
        var documents = new List<IReadOnlyList<string>>
        {
            new List<string> { "dashain", "festival", "kathmandu", "celebration" },
            new List<string> { "dashain", "festival", "kathmandu", "celebration" },
            new List<string> { "tech", "conference", "software" }
        };

        var vectors = TfIdfVectorizer.ComputeTfIdf(documents);
        var similarity = TfIdfVectorizer.CosineSimilarity(vectors[0], vectors[1]);

        Assert.Equal(1.0, similarity, precision: 6);
    }

    // UT-09: Completely different documents -> similarity score near 0.
    [Fact]
    public void CosineSimilarity_CompletelyDifferentDocuments_ReturnsNearZero()
    {
        var documents = new List<IReadOnlyList<string>>
        {
            new List<string> { "dashain", "festival", "kathmandu", "celebration" },
            new List<string> { "software", "engineering", "conference", "technology" }
        };

        var vectors = TfIdfVectorizer.ComputeTfIdf(documents);
        var similarity = TfIdfVectorizer.CosineSimilarity(vectors[0], vectors[1]);

        Assert.Equal(0.0, similarity, precision: 6);
    }
}
