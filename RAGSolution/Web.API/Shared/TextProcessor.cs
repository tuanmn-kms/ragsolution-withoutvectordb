using System.Text.RegularExpressions;
using Web.API.Shared;

namespace Web.API.Shared;

/// <summary>
/// Utility class for text processing operations (tokenization, chunking, etc.)
/// </summary>
internal static partial class TextProcessor
{
    [GeneratedRegex(@"[a-zA-Z0-9]+", RegexOptions.Compiled)]
    private static partial Regex TokenRegex();

    [GeneratedRegex(@"(?<=[.!?])\s+", RegexOptions.Compiled)]
    private static partial Regex SentenceSplitRegex();

    private const int SentencesPerChunk = 10;
    private const int MinTokenLength = 2;

    /// <summary>
    /// Tokenizes input text into lowercase alphanumeric terms
    /// </summary>
    public static IEnumerable<string> Tokenize(string input)
    {
        return TokenRegex()
            .Matches(input.ToLowerInvariant())
            .Select(match => match.Value)
            .Where(token => token.Length >= MinTokenLength);
    }

    /// <summary>
    /// Splits content into chunks of sentences with pre-computed tokens
    /// </summary>
    public static IReadOnlyList<RagChunk> ChunkContent(string content)
    {
        var sentences = SentenceSplitRegex()
            .Split(content.Trim())
            .Where(sentence => !string.IsNullOrWhiteSpace(sentence))
            .Select(s => s.Trim())
            .ToArray();

        if (sentences.Length == 0)
        {
            return Array.Empty<RagChunk>();
        }

        var chunks = new List<RagChunk>();
        var currentSentences = new List<string>(capacity: SentencesPerChunk);

        foreach (var sentence in sentences)
        {
            currentSentences.Add(sentence);

            if (currentSentences.Count == SentencesPerChunk)
            {
                chunks.Add(CreateChunk(currentSentences));
                currentSentences.Clear();
            }
        }

        // Add remaining sentences as final chunk
        if (currentSentences.Count > 0)
        {
            chunks.Add(CreateChunk(currentSentences));
        }

        return chunks;
    }

    /// <summary>
    /// Builds a snippet from text, truncating if necessary
    /// </summary>
    public static string BuildSnippet(string text, int maxLength = 200)
    {
        if (text.Length <= maxLength)
        {
            return text;
        }

        return string.Concat(text.AsSpan(0, maxLength).TrimEnd(), "...");
    }

    private static RagChunk CreateChunk(List<string> sentences)
    {
        var text = string.Join(" ", sentences);
        var tokens = Tokenize(text).ToArray();
        return new RagChunk(text, tokens);
    }
}
