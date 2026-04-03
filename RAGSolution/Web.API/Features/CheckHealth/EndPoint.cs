using FastEndpoints;
using Web.API.Database;

namespace Web.API.Features.CheckHealth;

/// <summary>
/// Health check endpoint for monitoring API status
/// </summary>
public class EndPoint : EndpointWithoutRequest<HealthResponse>
{
    private readonly IRagDocumentStore? _documentStore;

    public EndPoint(IRagDocumentStore? documentStore = null)
    {
        _documentStore = documentStore;
    }

    public override void Configure()
    {
        Get("/health");
        AllowAnonymous();
        Version(1);
        Description(b => b
            .Produces<HealthResponse>(200, "application/json")
            .WithTags("Health", "Monitoring")
            .WithSummary("Check API health status")
            .WithDescription("Returns the health status of the API and its dependencies including database connectivity"));
    }

    public override async Task HandleAsync(CancellationToken c)
    {
        var response = new HealthResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0",
            Service = "RAG API"
        };

        // Check document store health if available
        if (_documentStore != null)
        {
            try
            {
                var dbHealth = await _documentStore.CheckHealthAsync(c);
                response.DatabaseStatus = dbHealth.IsHealthy ? "Healthy" : "Unhealthy";
                response.DatabaseMessage = dbHealth.Message;
            }
            catch (Exception ex)
            {
                response.DatabaseStatus = "Unhealthy";
                response.DatabaseMessage = ex.Message;
                response.Status = "Degraded";
            }
        }
        else
        {
            response.DatabaseStatus = "Not Configured";
        }

        if (response.Status == "Healthy")
        {
            await Send.OkAsync(response, c);
        }
    }
}

/// <summary>
/// Health check response model
/// </summary>
public sealed class HealthResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string? DatabaseStatus { get; set; }
    public string? DatabaseMessage { get; set; }
}
