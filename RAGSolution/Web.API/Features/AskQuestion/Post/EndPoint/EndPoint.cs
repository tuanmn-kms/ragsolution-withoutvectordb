using FastEndpoints;
using Web.API.Features.AskQuestion.Post.Data;
using Web.API.Features.AskQuestion.Post.Model;

namespace Web.API.Features.AskQuestion.Post.EndPoint
{
    /// <summary>
    /// FastEndpoint for asking questions - V1
    /// </summary>
    public class EndPoint : Endpoint<Request, Response>
    {
        private readonly IQuestionAnsweringService _questionAnsweringService;

        public EndPoint(IQuestionAnsweringService questionAnsweringService)
        {
            _questionAnsweringService = questionAnsweringService;
        }

        /// <summary>
        /// https://fast-endpoints.com/docs/get-started
        /// https://dev.to/djnitehawk/building-rest-apis-in-net-6-the-easy-way-3h0d
        /// </summary>
        public override void Configure()
        {
            Post("/ask");
            AllowAnonymous();
            Version(1);
            Description(b => b
                .Produces<Response>(200, "application/json")
                .ProducesProblem(400)
                .ProducesProblem(500)
                .WithTags("RAG", "Question Answering")
                .WithSummary("Ask a question to the RAG system (V1)")
                .WithDescription("Submit a question and get an AI-generated answer based on ingested documents"));
        }

        public override async Task HandleAsync(Request r, CancellationToken c)
        {
            await Send.OkAsync(await _questionAnsweringService.AskAsync(r, c));
        }
    }
}
