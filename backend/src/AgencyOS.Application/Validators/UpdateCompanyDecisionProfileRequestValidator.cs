using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class UpdateCompanyDecisionProfileRequestValidator : AbstractValidator<UpdateCompanyDecisionProfileRequest>
{
    public UpdateCompanyDecisionProfileRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

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
