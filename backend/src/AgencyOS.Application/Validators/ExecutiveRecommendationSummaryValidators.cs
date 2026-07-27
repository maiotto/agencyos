using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class ExecutiveRecommendationSummaryQueryParametersValidator
    : AbstractValidator<ExecutiveRecommendationSummaryQueryParameters>
{
    public ExecutiveRecommendationSummaryQueryParametersValidator()
    {
        RuleFor(parameters => parameters.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) ||
                            ExecutiveRecommendationSummaryStatus.IsKnown(status))
            .WithMessage(
                $"Status must be one of: {string.Join(", ", ExecutiveRecommendationSummaryStatus.All)}.");

        RuleFor(parameters => parameters.MinConfidenceLevel)
            .InclusiveBetween(0, 100)
            .When(parameters => parameters.MinConfidenceLevel.HasValue);
    }
}

public class GenerateExecutiveRecommendationSummaryRequestValidator
    : AbstractValidator<GenerateExecutiveRecommendationSummaryRequest>
{
    public GenerateExecutiveRecommendationSummaryRequestValidator()
    {
        RuleFor(request => request.RecommendationId).NotEmpty();
        RuleFor(request => request.GeneratedBy).NotEmpty().MaximumLength(200);
    }
}

public class CreateExecutiveRecommendationSummaryVersionRequestValidator
    : AbstractValidator<CreateExecutiveRecommendationSummaryVersionRequest>
{
    public CreateExecutiveRecommendationSummaryVersionRequestValidator()
    {
        RuleFor(request => request.GeneratedBy).NotEmpty().MaximumLength(200);
    }
}
