using FastEndpoints;
using FastEndpoints.Swagger;
using FluentValidation;
using Web.API.Database;
using Web.API.Features.AskQuestion.Post.Data;
using Web.API.Features.IngestDocument.Post.Data;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
const string ReactCorsPolicy = "ReactLocalhostPolicy";

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy(ReactCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:51136")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddSingleton<IRagDocumentStore, SqlServerRagDocumentStore>();

// Register question answering service for FastEndpoints
builder.Services.AddSingleton<IQuestionAnsweringService,
    QuestionAnsweringService>();
builder.Services.AddSingleton<IIngestDocumentService,
    IngestDocumentService>();

// See AZURE_OPENAI_GUIDE.md for detailed setup instructions
var aiProvider = builder.Configuration["AI:Provider"] ?? "OpenAI";

switch (aiProvider.ToUpperInvariant())
{
    case "AZUREOPENAI":
        break;
    case "AZURE":
        break;
    case "OPENAI":
        break;
    case "INMEMORY":
        break;
    default:
        throw new InvalidOperationException(
            $"Unknown AI provider '{aiProvider}'. Valid options are: 'OpenAI', 'AzureOpenAI', or 'InMemory'.");
}

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();

// Add FastEndpoints with Swagger support
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(o =>
{
    o.MaxEndpointVersion = 2;
    o.DocumentSettings = s =>
    {
        s.DocumentName = "v1";
        s.Title = "RAG API with MCP Support";
        s.Version = "v1";
        s.Description = "A Retrieval-Augmented Generation (RAG) API for document ingestion and question answering without vector databases. Includes both Controllers and FastEndpoints.";
    };
});

builder.Services.SwaggerDocument(o =>
{
    o.MaxEndpointVersion = 2;
    o.DocumentSettings = s =>
    {
        s.DocumentName = "v2";
        s.Title = "RAG API with MCP Support";
        s.Version = "v2";
        s.Description = "Version 2 of the RAG API with enhanced features and improved performance. Includes both Controllers and FastEndpoints.";
    };
});

var app = builder.Build();

// Ensure the database and RagDocuments table exist on startup
var documentStore = app.Services.GetRequiredService<IRagDocumentStore>();
await documentStore.InitializeAsync();

app.UseHttpsRedirection();

app.UseCors(ReactCorsPolicy);

app.UseAuthorization();

// IMPORTANT: Configure FastEndpoints BEFORE mapping controllers
app.UseFastEndpoints(config =>
{
    config.Versioning.Prefix = "v";
    config.Versioning.PrependToRoute = true;
    config.Endpoints.RoutePrefix = "api";
});

// Add FastEndpoints Swagger generation middleware
app.UseSwaggerGen();

app.MapControllers();

app.Run();