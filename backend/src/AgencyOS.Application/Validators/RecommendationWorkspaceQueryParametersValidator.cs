using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class RecommendationWorkspaceQueryParametersValidator : AbstractValidator<RecommendationWorkspaceQueryParameters>
{
    public RecommendationWorkspaceQueryParametersValidator()
    {
        RuleFor(parameters => parameters)
            .Must(parameters => !parameters.From.HasValue || !parameters.To.HasValue || parameters.To >= parameters.From)
            .WithMessage("To cannot be earlier than From.");

        RuleFor(parameters => parameters)
            .Must(parameters =>
                (parameters.LeftRecommendationId.HasValue && parameters.RightRecommendationId.HasValue)
                || (!parameters.LeftRecommendationId.HasValue && !parameters.RightRecommendationId.HasValue))
            .WithMessage("LeftRecommendationId and RightRecommendationId must both be provided together.");

        RuleFor(parameters => parameters)
            .Must(parameters =>
                !parameters.LeftRecommendationId.HasValue
                || !parameters.RightRecommendationId.HasValue
                || parameters.LeftRecommendationId != parameters.RightRecommendationId)
            .WithMessage("LeftRecommendationId and RightRecommendationId must be different.");
    }
}
