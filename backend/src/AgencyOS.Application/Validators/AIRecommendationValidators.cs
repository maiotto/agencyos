using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class AIRecommendationQueryParametersValidator : AbstractValidator<AIRecommendationQueryParameters>
{
    public AIRecommendationQueryParametersValidator()
    {
        RuleFor(parameters => parameters.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) || AIRecommendationStatus.IsKnown(status))
            .WithMessage("Status filter is not a known AI recommendation status.");

        RuleFor(parameters => parameters.Search).MaximumLength(300);

        RuleFor(parameters => parameters.MinConfidenceScore)
            .InclusiveBetween(0, 100)
            .When(parameters => parameters.MinConfidenceScore.HasValue);
    }
}

public class GenerateAIRecommendationRequestValidator : AbstractValidator<GenerateAIRecommendationRequest>
{
    public GenerateAIRecommendationRequestValidator()
    {
        RuleFor(request => request.RecommendationId).NotEmpty();
        RuleFor(request => request.GeneratedBy).NotEmpty().MaximumLength(200);
    }
}
