using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Web.API.Features.AskQuestion.Post.Model;

/// <summary>
/// Request model for asking questions to the RAG system
/// </summary>
public sealed record Request
{
    [Required(ErrorMessage = "Question is required")]
    [MinLength(3, ErrorMessage = "Question must be at least 3 characters")]
    [SwaggerSchema("The question to ask the RAG system", Nullable = false)]
    public required string Question { get; init; }

    [Range(1, 10, ErrorMessage = "TopK must be between 1 and 10")]
    [SwaggerSchema("Number of top relevant documents to retrieve (default: 3)", Nullable = false)]
    public int TopK { get; init; } = 3;

    [Range(1, 10, ErrorMessage = "MaxAnswerSentences must be between 1 and 10")]
    [SwaggerSchema("Maximum number of sentences in the generated answer (default: 3)", Nullable = false)]
    public int MaxAnswerSentences { get; init; } = 3;

    [Required(ErrorMessage = "Model is required")]
    public required string Model { get; init; } = "gpt-4o-mini";
}

/// <summary>
/// Response model containing the answer and source documents
/// </summary>
public sealed record Response(
    [property: SwaggerSchema("The original question that was asked")]
    string Question,

    [property: SwaggerSchema("The AI-generated answer based on the context")]
    string Answer,

    [property: SwaggerSchema("List of source documents used to generate the answer")]
    IReadOnlyList<Source> Sources
);

/// <summary>
/// Source document information with relevance score
/// </summary>
public sealed record Source(
    [property: SwaggerSchema("Unique identifier of the source document")]
    string DocumentId,

    [property: SwaggerSchema("Title of the source document")]
    string Title,

    [property: SwaggerSchema("Relevant snippet from the source document")]
    string Snippet,

    [property: SwaggerSchema("Relevance score of this source document")]
    double Score
);
