using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class PlanningWorkspaceQueryParametersValidator : AbstractValidator<PlanningWorkspaceQueryParameters>
{
    public PlanningWorkspaceQueryParametersValidator()
    {
        RuleFor(parameters => parameters)
            .Must(parameters => !parameters.From.HasValue || !parameters.To.HasValue || parameters.To >= parameters.From)
            .WithMessage("To cannot be earlier than From.");

        RuleFor(parameters => parameters)
            .Must(parameters =>
                !parameters.PeriodStart.HasValue
                || !parameters.PeriodEnd.HasValue
                || parameters.PeriodEnd >= parameters.PeriodStart)
            .WithMessage("PeriodEnd cannot be earlier than PeriodStart.");
    }
}
