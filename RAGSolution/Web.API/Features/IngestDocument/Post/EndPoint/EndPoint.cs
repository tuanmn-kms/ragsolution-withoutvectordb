using FastEndpoints;
using Web.API.Features.IngestDocument.Post.Data;
using Web.API.Features.IngestDocument.Post.Model;

namespace Web.API.Features.IngestDocument.Post.EndPoint
{
    /// <summary>
    /// FastEndpoint for ingesting documents - V1
    /// </summary>
    public class EndPoint : Endpoint<Request, Response>
    {
        private readonly IIngestDocumentService _ingestDocumentService;

        public EndPoint(IIngestDocumentService ingestDocumentService) 
        {
            _ingestDocumentService = ingestDocumentService;
        }

        public sealed override void Configure()
        {
            Post("/ingest");
            AllowAnonymous();
            Version(1);
            Description(b => b
                .Produces<Response>(200, "application/json")
                .ProducesProblem(400)
                .ProducesProblem(500)
                .WithTags("RAG", "Document Ingestion")
                .WithSummary("Ingest a document into the RAG system (V1)")
                .WithDescription("Submit a document with title, content, and metadata to be ingested into the RAG system for later retrieval"));
        }

        public sealed override async Task HandleAsync(Request r, CancellationToken c)
        {
            var result = await _ingestDocumentService.IngestAsync(r, c);
            await Send.OkAsync(result, cancellation: c);
        }
    }
}
