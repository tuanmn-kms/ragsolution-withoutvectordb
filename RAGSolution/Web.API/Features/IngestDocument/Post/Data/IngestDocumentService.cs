using System.Collections.Concurrent;
using Web.API.Database;
using Web.API.Features.IngestDocument.Post.Model;
using Web.API.Shared;

namespace Web.API.Features.IngestDocument.Post.Data
{
    internal sealed class IngestDocumentService(
        IRagDocumentStore documentStore) : IIngestDocumentService
    {

        private readonly ConcurrentDictionary<string, RagDocument> _documents = new(StringComparer.OrdinalIgnoreCase);

        public async Task<Response> IngestAsync(Request request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(request.Id))
            {
                throw new ArgumentException("Document id is required.", nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                throw new ArgumentException("Document content is required.", nameof(request));
            }

            var title = string.IsNullOrWhiteSpace(request.Title) ? request.Id : request.Title;
            var chunks = TextProcessor.ChunkContent(request.Content);

            var metadata = request.Metadata is null
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(request.Metadata, StringComparer.OrdinalIgnoreCase);

            _documents[request.Id] = new RagDocument(
                request.Id,
                title,
                request.Content,
                chunks,
                metadata);

            await documentStore.UpsertDocumentAsync(new StoredRagDocument(
                request.Id,
                title,
                request.Content,
                metadata), cancellationToken);

            return new Response(
                Message: $"Document '{request.Id}' ingested successfully.",
                DocumentId: request.Id,
                ChunkCount: chunks.Count
            );
        }
    }
}
