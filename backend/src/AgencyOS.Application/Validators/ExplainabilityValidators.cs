using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class ExplainabilityQueryParametersValidator : AbstractValidator<ExplainabilityQueryParameters>
{
    public ExplainabilityQueryParametersValidator()
    {
        RuleFor(parameters => parameters.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) || ExplainabilityStatus.IsKnown(status))
            .WithMessage($"Status must be one of: {string.Join(", ", ExplainabilityStatus.All)}.");

        RuleFor(parameters => parameters.ExplanationType)
            .Must(type => string.IsNullOrWhiteSpace(type) || ExplainabilityTypes.IsKnown(type))
            .WithMessage($"ExplanationType must be one of: {string.Join(", ", ExplainabilityTypes.All)}.");
    }
}

public class GenerateExplainabilityRequestValidator : AbstractValidator<GenerateExplainabilityRequest>
{
    public GenerateExplainabilityRequestValidator()
    {
        RuleFor(request => request.RecommendationId)
            .NotEmpty()
            .WithMessage("RecommendationId is required.");

        RuleFor(request => request.GeneratedBy)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.ExplanationType)
            .Must(type => string.IsNullOrWhiteSpace(type) || ExplainabilityTypes.IsKnown(type))
            .WithMessage($"ExplanationType must be one of: {string.Join(", ", ExplainabilityTypes.All)}.");

        RuleFor(request => request)
            .Must(request =>
            {
                var type = ResolveType(request);
                if (ExplainabilityTypes.IsAIRecommendation(type))
                {
                    return request.AIRecommendationId.HasValue && request.AIRecommendationId.Value != Guid.Empty;
                }

                return !request.AIRecommendationId.HasValue;
            })
            .WithMessage(
                "AIRecommendationId is required for AIRecommendation explanations and must be omitted for Recommendation explanations.");
    }

    private static string ResolveType(GenerateExplainabilityRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.ExplanationType))
        {
            return request.ExplanationType.Trim();
        }

        return request.AIRecommendationId.HasValue
            ? ExplainabilityTypes.AIRecommendation
            : ExplainabilityTypes.Recommendation;
    }
}
