using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class CreateCompanyDecisionProfileRequestValidator : AbstractValidator<CreateCompanyDecisionProfileRequest>
{
    public CreateCompanyDecisionProfileRequestValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("CompanyId is mandatory.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Code is mandatory.")
            .MaximumLength(100);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is mandatory (BR-1902).")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.PriorityWeights)
            .NotNull()
            .Must(weights => weights.Count > 0 && weights.Any(weight => weight.Weight > 0))
            .WithMessage("PriorityWeights must include at least one dimension with a positive weight.");

        RuleForEach(x => x.PriorityWeights)
            .SetValidator(new DecisionProfileDimensionRequestValidator());

        RuleFor(x => x.CapacityWeight).InclusiveBetween(0m, 1m);
        RuleFor(x => x.WorkloadWeight).InclusiveBetween(0m, 1m);
        RuleFor(x => x.CostWeight).InclusiveBetween(0m, 1m);
        RuleFor(x => x.RiskWeight).InclusiveBetween(0m, 1m);
        RuleFor(x => x.QualityWeight).InclusiveBetween(0m, 1m);

        RuleFor(x => x.PreferredStrategy)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.PreferredStrategy));

        RuleFor(x => x.PreferredCapacityThreshold)
            .InclusiveBetween(0m, 100m)
            .When(x => x.PreferredCapacityThreshold.HasValue);

        RuleFor(x => x.PreferredWorkloadThreshold)
            .InclusiveBetween(0m, 100m)
            .When(x => x.PreferredWorkloadThreshold.HasValue);
    }
}

public class DecisionProfileDimensionRequestValidator : AbstractValidator<DecisionProfileDimensionRequest>
{
    public DecisionProfileDimensionRequestValidator()
    {
        RuleFor(x => x.Dimension)
            .NotEmpty()
            .Must(dimension => RankingDimension.All.Contains(dimension))
            .WithMessage(x => $"Dimension must be one of: {string.Join(", ", RankingDimension.All)}.");

        RuleFor(x => x.Weight)
            .InclusiveBetween(0m, 1m);
    }
}
