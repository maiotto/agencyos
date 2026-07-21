using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class WorkloadQueryParametersValidator : AbstractValidator<WorkloadQueryParameters>
{
    public WorkloadQueryParametersValidator()
    {
        RuleFor(x => x.PeriodStartDate)
            .NotEmpty()
            .WithMessage("Planning period start date is required.");

        RuleFor(x => x.PeriodEndDate)
            .NotEmpty()
            .WithMessage("Planning period end date is required.")
            .GreaterThanOrEqualTo(x => x.PeriodStartDate)
            .WithMessage("Planning period end date must not be before the start date.");
    }
}
