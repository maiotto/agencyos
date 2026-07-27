using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class CreateRecommendationRequestValidator : AbstractValidator<CreateRecommendationRequest>
{
    public CreateRecommendationRequestValidator()
    {
        RuleFor(request => request.CompanyId).NotEmpty();
        RuleFor(request => request.MissionId).NotEmpty();
        RuleFor(request => request.ContractId).NotEmpty();
        RuleFor(request => request.DeliveryStrategyId).NotEmpty();
        RuleFor(request => request.Title).NotEmpty().MaximumLength(300);
        RuleFor(request => request.Summary).MaximumLength(2000).When(request => request.Summary is not null);
        RuleFor(request => request.Reason).MaximumLength(4000).When(request => request.Reason is not null);
        RuleFor(request => request.RecommendationPayload).NotEmpty();
        RuleFor(request => request.CapacitySnapshot).NotEmpty();
        RuleFor(request => request.WorkloadSnapshot).NotEmpty();
        RuleFor(request => request.GeneratedBy).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Version)
            .GreaterThanOrEqualTo(1)
            .When(request => request.Version.HasValue);
    }
}

public class CreateRecommendationVersionRequestValidator : AbstractValidator<CreateRecommendationVersionRequest>
{
    public CreateRecommendationVersionRequestValidator()
    {
        RuleFor(request => request.RecommendationPayload).NotEmpty();
        RuleFor(request => request.CapacitySnapshot).NotEmpty();
        RuleFor(request => request.WorkloadSnapshot).NotEmpty();
        RuleFor(request => request.GeneratedBy).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Title).MaximumLength(300).When(request => request.Title is not null);
        RuleFor(request => request.Summary).MaximumLength(2000).When(request => request.Summary is not null);
        RuleFor(request => request.Reason).MaximumLength(4000).When(request => request.Reason is not null);
    }
}

public class RecommendationQueryParametersValidator : AbstractValidator<RecommendationQueryParameters>
{
    public RecommendationQueryParametersValidator()
    {
        RuleFor(request => request.OrderDirection)
            .Must(value => value is null
                || string.Equals(value, "asc", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("OrderDirection must be 'asc' or 'desc'.");
    }
}
