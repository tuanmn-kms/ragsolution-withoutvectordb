namespace Web.API.Shared;

/// <summary>
/// Represents a document with chunked content for RAG processing
/// </summary>
internal sealed record RagDocument(
    string Id,
    string Title,
    string Content,
    IReadOnlyList<RagChunk> Chunks,
    IReadOnlyDictionary<string, string> Metadata
);

/// <summary>
/// Represents a chunk of text with pre-computed tokens
/// </summary>
internal sealed record RagChunk(
    string Text,
    IReadOnlyCollection<string> Tokens
);