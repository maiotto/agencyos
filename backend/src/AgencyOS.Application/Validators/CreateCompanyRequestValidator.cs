using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class CreateCompanyRequestValidator : AbstractValidator<CreateCompanyRequest>
{
    public CreateCompanyRequestValidator()
    {
        RuleFor(x => x.CompanyCode)
            .NotEmpty()
            .WithMessage("CompanyCode is mandatory (BR-2002).")
            .MaximumLength(50);

        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .WithMessage("CompanyName is mandatory (BR-2001).")
            .MaximumLength(200);

        RuleFor(x => x.LegalName)
            .MaximumLength(300)
            .When(x => !string.IsNullOrWhiteSpace(x.LegalName));

        RuleFor(x => x.Timezone)
            .NotEmpty()
            .WithMessage("Timezone is mandatory.")
            .MaximumLength(100);

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Country));

        RuleFor(x => x.Language)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.Language));

        RuleFor(x => x.Currency)
            .MaximumLength(10)
            .When(x => !string.IsNullOrWhiteSpace(x.Currency));
    }
}
