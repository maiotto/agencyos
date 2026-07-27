using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class RecommendationComparisonQueryParametersValidator
    : AbstractValidator<RecommendationComparisonQueryParameters>
{
    public RecommendationComparisonQueryParametersValidator()
    {
        RuleFor(parameters => parameters.LeftId)
            .NotEmpty()
            .WithMessage("LeftId is required.");

        RuleFor(parameters => parameters.RightId)
            .NotEmpty()
            .WithMessage("RightId is required.");

        RuleFor(parameters => parameters)
            .Must(parameters => parameters.LeftId != parameters.RightId)
            .WithMessage("LeftId and RightId must be different.");
    }
}

public class RecommendationVersionComparisonQueryParametersValidator
    : AbstractValidator<RecommendationVersionComparisonQueryParameters>
{
    public RecommendationVersionComparisonQueryParametersValidator()
    {
        RuleFor(parameters => parameters.LeftVersion)
            .GreaterThanOrEqualTo(1)
            .When(parameters => parameters.LeftVersion.HasValue);

        RuleFor(parameters => parameters.RightVersion)
            .GreaterThanOrEqualTo(1)
            .When(parameters => parameters.RightVersion.HasValue);

        RuleFor(parameters => parameters)
            .Must(parameters =>
                parameters.LeftVersion.HasValue == parameters.RightVersion.HasValue)
            .WithMessage("LeftVersion and RightVersion must both be provided or both omitted.");

        RuleFor(parameters => parameters)
            .Must(parameters =>
                !parameters.LeftVersion.HasValue
                || !parameters.RightVersion.HasValue
                || parameters.LeftVersion != parameters.RightVersion)
            .WithMessage("LeftVersion and RightVersion must be different.");
    }
}
