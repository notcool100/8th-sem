namespace NtbEvent.Application.Common;

/// <summary>Okapi BM25 ranking over a small in-memory document set (used by the smart-search service).</summary>
public static class Bm25Ranker
{
    /// <summary>
    /// Scores every document in <paramref name="documents"/> against <paramref name="queryTerms"/> using
    /// BM25 (default k1=1.5, b=0.75). Returns (documentIndex, score) pairs with score &gt; 0,
    /// sorted descending by score.
    /// </summary>
    public static List<(int DocumentIndex, double Score)> Rank(
        IReadOnlyList<IReadOnlyList<string>> documents,
        IReadOnlyList<string> queryTerms,
        double k1 = 1.5,
        double b = 0.75)
    {
        var results = new List<(int DocumentIndex, double Score)>();
        if (documents.Count == 0 || queryTerms.Count == 0)
        {
            return results;
        }

        var documentCount = documents.Count;
        var docLengths = documents.Select(d => d.Count).ToArray();
        var avgDocLength = docLengths.Length == 0 ? 0.0 : docLengths.Average();

        // Inverted index: term -> (docIndex -> term frequency in that doc)
        var invertedIndex = new Dictionary<string, Dictionary<int, int>>();
        for (var docIndex = 0; docIndex < documentCount; docIndex++)
        {
            foreach (var term in documents[docIndex])
            {
                if (!invertedIndex.TryGetValue(term, out var postings))
                {
                    postings = new Dictionary<int, int>();
                    invertedIndex[term] = postings;
                }

                postings[docIndex] = postings.GetValueOrDefault(docIndex) + 1;
            }
        }

        var scores = new double[documentCount];
        foreach (var term in queryTerms.Distinct())
        {
            if (!invertedIndex.TryGetValue(term, out var postings))
            {
                continue;
            }

            var documentFrequency = postings.Count;
            // BM25 IDF with +1 smoothing to keep it non-negative even when df > N/2.
            var idf = Math.Log(((documentCount - documentFrequency + 0.5) / (documentFrequency + 0.5)) + 1.0);

            foreach (var (docIndex, termFrequency) in postings)
            {
                var docLength = docLengths[docIndex];
                var normalizedLength = avgDocLength == 0 ? 1.0 : docLength / avgDocLength;
                var numerator = termFrequency * (k1 + 1.0);
                var denominator = termFrequency + k1 * (1.0 - b + b * normalizedLength);
                scores[docIndex] += idf * (numerator / denominator);
            }
        }

        for (var docIndex = 0; docIndex < documentCount; docIndex++)
        {
            if (scores[docIndex] > 0)
            {
                results.Add((docIndex, scores[docIndex]));
            }
        }

        results.Sort((x, y) => y.Score.CompareTo(x.Score));
        return results;
    }
}
