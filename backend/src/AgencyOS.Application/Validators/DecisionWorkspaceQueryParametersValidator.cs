using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class DecisionWorkspaceQueryParametersValidator : AbstractValidator<DecisionWorkspaceQueryParameters>
{
    public DecisionWorkspaceQueryParametersValidator()
    {
        RuleFor(parameters => parameters)
            .Must(parameters => !parameters.From.HasValue || !parameters.To.HasValue || parameters.To >= parameters.From)
            .WithMessage("To cannot be earlier than From.");
    }
}
