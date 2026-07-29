using NtbEvent.Application.Common;

namespace NtbEvent.Tests.Unit.AI;

/// <summary>Mid-term report §4.2.1 UT-10 (AI - BM25).</summary>
public sealed class Bm25RankerTests
{
    // UT-10: Query term found in one document only -> that document ranked first.
    [Fact]
    public void Rank_QueryTermInOneDocumentOnly_RanksThatDocumentFirst()
    {
        var documents = new List<IReadOnlyList<string>>
        {
            new List<string> { "kathmandu", "durbar", "square", "tour" },
            new List<string> { "dashain", "festival", "celebration", "family" },
            new List<string> { "pokhara", "lake", "boating", "adventure" }
        };
        var queryTerms = new List<string> { "dashain" };

        var ranked = Bm25Ranker.Rank(documents, queryTerms);

        Assert.Single(ranked);
        Assert.Equal(1, ranked[0].DocumentIndex);
        Assert.True(ranked[0].Score > 0);
    }

    [Fact]
    public void Rank_NoMatchingTerms_ReturnsEmpty()
    {
        var documents = new List<IReadOnlyList<string>>
        {
            new List<string> { "kathmandu", "durbar", "square" }
        };

        var ranked = Bm25Ranker.Rank(documents, new List<string> { "unrelated" });

        Assert.Empty(ranked);
    }
}
