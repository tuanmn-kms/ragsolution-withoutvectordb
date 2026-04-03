using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Web.API.Features.IngestDocument.Post.Model;

/// <summary>
/// Request model for ingesting a document
/// </summary>
public sealed record Request(
    [property: Required(ErrorMessage = "Document ID is required")]
    [property: SwaggerSchema("Unique identifier for the document")]
    string Id,

    [property: SwaggerSchema("Title of the document (defaults to ID if not provided)")]
    string? Title,

    [property: Required(ErrorMessage = "Document content is required")]
    [property: MinLength(10, ErrorMessage = "Content must be at least 10 characters")]
    [property: SwaggerSchema("The text content of the document to be ingested")]
    string Content,

    [property: SwaggerSchema("Additional metadata key-value pairs for the document")]
    IReadOnlyDictionary<string, string>? Metadata
);

/// <summary>
/// Response model for document ingestion
/// </summary>
public sealed record Response(
    [property: SwaggerSchema("Status message indicating the result of the ingestion")]
    string Message,

    [property: SwaggerSchema("The ID of the ingested document")]
    string DocumentId,

    [property: SwaggerSchema("Number of chunks the document was split into")]
    int ChunkCount
);
