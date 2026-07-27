using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class RecommendationHistoryQueryParametersValidator
    : AbstractValidator<RecommendationHistoryQueryParameters>
{
    public RecommendationHistoryQueryParametersValidator()
    {
        RuleFor(parameters => parameters.Search).MaximumLength(300);
        RuleFor(parameters => parameters.OrderDirection)
            .Must(value => value is null
                || string.Equals(value, "asc", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("OrderDirection must be 'asc' or 'desc'.");
        RuleFor(parameters => parameters.Version)
            .GreaterThanOrEqualTo(1)
            .When(parameters => parameters.Version.HasValue);
    }
}
