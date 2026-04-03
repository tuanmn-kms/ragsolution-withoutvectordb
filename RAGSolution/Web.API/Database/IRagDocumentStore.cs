namespace Web.API.Database;

public interface IRagDocumentStore
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task UpsertDocumentAsync(StoredRagDocument document, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoredRagDocument>> GetAllDocumentsAsync(CancellationToken cancellationToken = default);
    Task<RagDatabaseHealth> CheckHealthAsync(CancellationToken cancellationToken = default);
}

public sealed record StoredRagDocument(
    string Id,
    string Title,
    string Content,
    IReadOnlyDictionary<string, string> Metadata
);

public sealed record RagDatabaseHealth(
    bool IsHealthy,
    string Server,
    string Database,
    string Message
);
