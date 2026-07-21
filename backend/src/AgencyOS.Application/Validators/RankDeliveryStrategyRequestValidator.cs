using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class RankDeliveryStrategyRequestValidator : AbstractValidator<RankDeliveryStrategyRequest>
{
    public RankDeliveryStrategyRequestValidator()
    {
        RuleFor(x => x.ContractId)
            .NotEmpty()
            .WithMessage("Contract is required.");

        RuleFor(x => x.MissionId)
            .NotEmpty()
            .WithMessage("Mission is required.");

        RuleFor(x => x.PeriodStartDate)
            .NotEmpty()
            .WithMessage("Planning period start date is required.");

        RuleFor(x => x.PeriodEndDate)
            .NotEmpty()
            .WithMessage("Planning period end date is required.")
            .GreaterThanOrEqualTo(x => x.PeriodStartDate)
            .WithMessage("Planning period end date must not be before the start date.");

        RuleFor(x => x.CompanyDecisionProfileId)
            .NotEmpty()
            .WithMessage("Company Decision Profile is required.");
    }
}
