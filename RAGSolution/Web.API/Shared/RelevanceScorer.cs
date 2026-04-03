namespace Web.API.Shared;

/// <summary>
/// Calculates relevance scores between query terms and document chunks
/// </summary>
internal static class RelevanceScorer
{
    /// <summary>
    /// Computes relevance score based on term overlap and density
    /// </summary>
    /// <param name="questionTerms">Tokenized question terms</param>
    /// <param name="chunkTerms">Tokenized chunk terms</param>
    /// <returns>Relevance score (higher is better)</returns>
    public static double ComputeRelevance(
        IReadOnlyCollection<string> questionTerms,
        IReadOnlyCollection<string> chunkTerms)
    {
        if (questionTerms.Count == 0 || chunkTerms.Count == 0)
        {
            return 0;
        }

        var chunkTermSet = new HashSet<string>(chunkTerms, StringComparer.OrdinalIgnoreCase);
        var overlap = questionTerms.Count(chunkTermSet.Contains);

        if (overlap == 0)
        {
            return 0;
        }

        // Lexical score: proportion of question terms found in chunk
        var lexicalScore = overlap / (double)questionTerms.Count;

        // Density bonus: encourages selecting chunks that are both relevant and concise
        var densityBonus = overlap / (double)Math.Max(1, chunkTerms.Count);

        return lexicalScore + densityBonus;
    }
}
