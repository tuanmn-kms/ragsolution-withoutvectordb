using OpenAI.Chat;
using System.Collections.Concurrent;
using Web.API.Database;
using Web.API.Features.AskQuestion.Post.Model;
using Web.API.Shared;

namespace Web.API.Features.AskQuestion.Post.Data;

/// <summary>
/// Internal implementation of question answering service using RAG pattern
/// </summary>
internal sealed class QuestionAnsweringService : IQuestionAnsweringService
{
    private const int MinTopK = 1;
    private const int MaxTopK = 10;

    private readonly IRagDocumentStore _documentStore;
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, RagDocument> _documents = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _loadLock = new(1, 1);
    private volatile bool _isLoaded;

    public QuestionAnsweringService(
        IRagDocumentStore documentStore,
        IConfiguration configuration)
    {
        _documentStore = documentStore;
        _configuration = configuration;
    }

    public async Task<Response> AskAsync(Request request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await EnsureDocumentsLoadedAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(request.Question))
        {
            throw new ArgumentException("Question is required.", nameof(request));
        }

        if (_documents.IsEmpty)
        {
            return CreateResponse(
                request.Question,
                "No documents available. Ingest documents before asking questions.");
        }

        var questionTerms = TextProcessor.Tokenize(request.Question).ToArray();

        if (questionTerms.Length == 0)
        {
            return CreateResponse(
                request.Question,
                "Could not process the question. Please use descriptive words.");
        }

        var rankedChunks = FindRelevantChunks(questionTerms, request.TopK);

        if (rankedChunks.Length == 0)
        {
            return CreateResponse(
                request.Question,
                "I could not find relevant context in the ingested documents.");
        }

        var sources = BuildSources(rankedChunks);
        var context = string.Join("\n\n", rankedChunks.Select(x => x.Chunk.Text));
        var answer = await GenerateAnswerAsync(request.Question, context, request.Model, request.MaxAnswerSentences, cancellationToken);

        return new Response(request.Question, answer, sources);
    }

    private RankedChunk[] FindRelevantChunks(string[] questionTerms, int topK)
    {
        var clampedTopK = Math.Clamp(topK, MinTopK, MaxTopK);

        return _documents.Values
            .SelectMany(doc => doc.Chunks.Select(chunk => new RankedChunk(
                doc.Id,
                doc.Title,
                chunk,
                RelevanceScorer.ComputeRelevance(questionTerms, chunk.Tokens))))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.DocumentId, StringComparer.OrdinalIgnoreCase)
            .Take(clampedTopK)
            .ToArray();
    }

    private static Source[] BuildSources(RankedChunk[] rankedChunks)
    {
        return rankedChunks
            .Select(x => new Source(
                x.DocumentId,
                x.Title,
                TextProcessor.BuildSnippet(x.Chunk.Text),
                Math.Round(x.Score, 4)))
            .ToArray();
    }

    private async Task<string> GenerateAnswerAsync(
        string question,
        string context,
        string model,
       int maxAnswerSentences,
        CancellationToken cancellationToken = default)
    {
        var prompt = $"""
            Answer the question using ONLY the information provided in the context below.
            Use the context to answer the question.
            Limit all my generated answers to a maximum of {maxAnswerSentences} sentences.
            Be concise, accurate. 

            Context:
            {context}

            Question:
            {question}
            """;

        //var prompt = $"""
        //    Answer the question using ONLY the information provided in the context below.
        //    If the context does not contain enough information to answer the question, respond with "I don't have enough information to answer this question."
        //    Be concise, accurate, and do not add information beyond what is provided.

        //    Context:
        //    {context}

        //    Question:
        //    {question}
        //    """;

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful assistant that answers questions based on provided context."),
            new UserChatMessage(prompt)
        };

        var chatClient = CreateChatClient(_configuration, model);
        var response = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);

        return response.Value.Content[0].Text;
    }

    private async Task EnsureDocumentsLoadedAsync(CancellationToken cancellationToken)
    {
        if (_isLoaded) return;

        await _loadLock.WaitAsync(cancellationToken);
        try
        {
            if (_isLoaded) return;

            var storedDocuments = await _documentStore.GetAllDocumentsAsync(cancellationToken);

            foreach (var doc in storedDocuments)
            {
                var chunks = TextProcessor.ChunkContent(doc.Content);
                _documents[doc.Id] = new RagDocument(
                    doc.Id,
                    doc.Title,
                    doc.Content,
                    chunks,
                    doc.Metadata);
            }

            _isLoaded = true;
        }
        finally
        {
            _loadLock.Release();
        }
    }

    private static ChatClient CreateChatClient(IConfiguration configuration, string model)
    {
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI API key is not configured.");
        if(string.IsNullOrWhiteSpace(model))
        {
            model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";
        }

        return new ChatClient(model, apiKey);
    }

    private static Response CreateResponse(string question, string answer)
        => new(question, answer, Array.Empty<Source>());

    /// <summary>
    /// Internal record for ranked chunk results
    /// </summary>
    private readonly record struct RankedChunk(
        string DocumentId,
        string Title,
        RagChunk Chunk,
        double Score);
}
