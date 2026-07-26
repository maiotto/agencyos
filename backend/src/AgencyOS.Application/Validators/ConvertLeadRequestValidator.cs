using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class ConvertLeadRequestValidator : AbstractValidator<ConvertLeadRequest>
{
    public ConvertLeadRequestValidator()
    {
        RuleFor(x => x.LegalName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.TaxIdentifier)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.TradeName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.TradeName));

        RuleFor(x => x.Website)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.Website));
    }
}
