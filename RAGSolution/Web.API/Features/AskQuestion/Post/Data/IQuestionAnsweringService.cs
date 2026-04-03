using Web.API.Features.AskQuestion.Post.Model;

namespace Web.API.Features.AskQuestion.Post.Data;

/// <summary>
/// Service for answering questions using RAG (Retrieval-Augmented Generation)
/// </summary>
public interface IQuestionAnsweringService
{
    /// <summary>
    /// Ask a question and get an AI-generated answer based on ingested documents
    /// </summary>
    Task<Response> AskAsync(Request request, CancellationToken cancellationToken = default);
}
