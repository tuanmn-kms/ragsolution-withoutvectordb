using Web.API.Features.IngestDocument.Post.Model;

namespace Web.API.Features.IngestDocument.Post.Data
{
    public interface IIngestDocumentService
    {
        Task<Response> IngestAsync(Request request, CancellationToken cancellationToken = default);
    }
}
