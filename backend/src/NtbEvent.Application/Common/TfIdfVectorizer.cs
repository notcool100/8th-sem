namespace NtbEvent.Application.Common;

/// <summary>Bag-of-words TF-IDF vectorizer with cosine similarity, shared by the recommendation and tag-suggestion services.</summary>
public static class TfIdfVectorizer
{
    /// <summary>
    /// Computes a TF-IDF vector (term -> weight) for every document in <paramref name="documents"/>.
    /// TF is raw term frequency within the document; IDF is the standard smoothed
    /// log((1 + N) / (1 + df)) + 1, so terms present in every document don't collapse to zero.
    /// </summary>
    public static List<Dictionary<string, double>> ComputeTfIdf(IReadOnlyList<IReadOnlyList<string>> documents)
    {
        var documentCount = documents.Count;
        var documentFrequency = new Dictionary<string, int>();

        foreach (var document in documents)
        {
            foreach (var term in document.Distinct())
            {
                documentFrequency[term] = documentFrequency.GetValueOrDefault(term) + 1;
            }
        }

        var idf = documentFrequency.ToDictionary(
            kvp => kvp.Key,
            kvp => Math.Log((1.0 + documentCount) / (1.0 + kvp.Value)) + 1.0);

        var vectors = new List<Dictionary<string, double>>(documentCount);
        foreach (var document in documents)
        {
            var termFrequency = new Dictionary<string, int>();
            foreach (var term in document)
            {
                termFrequency[term] = termFrequency.GetValueOrDefault(term) + 1;
            }

            var vector = termFrequency.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value * idf[kvp.Key]);

            vectors.Add(vector);
        }

        return vectors;
    }

    public static double CosineSimilarity(IReadOnlyDictionary<string, double> a, IReadOnlyDictionary<string, double> b)
    {
        if (a.Count == 0 || b.Count == 0)
        {
            return 0.0;
        }

        // Iterate the smaller map for the dot product.
        var (smaller, larger) = a.Count <= b.Count ? (a, b) : (b, a);

        var dotProduct = 0.0;
        foreach (var (term, weight) in smaller)
        {
            if (larger.TryGetValue(term, out var otherWeight))
            {
                dotProduct += weight * otherWeight;
            }
        }

        var magnitudeA = Math.Sqrt(a.Values.Sum(w => w * w));
        var magnitudeB = Math.Sqrt(b.Values.Sum(w => w * w));

        if (magnitudeA == 0.0 || magnitudeB == 0.0)
        {
            return 0.0;
        }

        return dotProduct / (magnitudeA * magnitudeB);
    }
}
