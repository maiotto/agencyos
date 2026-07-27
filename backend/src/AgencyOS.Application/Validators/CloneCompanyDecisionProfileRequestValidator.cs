using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class CloneCompanyDecisionProfileRequestValidator : AbstractValidator<CloneCompanyDecisionProfileRequest>
{
    public CloneCompanyDecisionProfileRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Cloned profile Name is mandatory.")
            .MaximumLength(200);

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Cloned profile Code is mandatory.")
            .MaximumLength(100);
    }
}
