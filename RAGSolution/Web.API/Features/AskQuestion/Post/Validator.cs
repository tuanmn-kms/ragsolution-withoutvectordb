using FastEndpoints;
using FluentValidation;
using Web.API.Features.AskQuestion.Post.Model;

namespace Web.API.Features.AskQuestion.Post
{
    public class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Question)
                .NotEmpty()
                .WithMessage("Question is required")
                .MinimumLength(3)
                .WithMessage("Question must be at least 3 characters");

            RuleFor(x => x.TopK)
                .InclusiveBetween(1, 10)
                .WithMessage("TopK must be between 1 and 10");

            RuleFor(x => x.MaxAnswerSentences)
                .InclusiveBetween(1, 10)
                .WithMessage("MaxAnswerSentences must be between 1 and 10");
        }
    }
}
