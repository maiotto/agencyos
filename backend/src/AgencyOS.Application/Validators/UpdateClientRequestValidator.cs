using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class UpdateClientRequestValidator : AbstractValidator<UpdateClientRequest>
{
    public UpdateClientRequestValidator()
    {
        RuleFor(x => x.LegalName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(status => ClientStatus.All.Contains(status))
            .WithMessage("Status must be a valid Client status.");

        RuleFor(x => x.TradeName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.TradeName));

        RuleFor(x => x.TaxIdentifier)
            .MaximumLength(30)
            .When(x => !string.IsNullOrWhiteSpace(x.TaxIdentifier));

        RuleFor(x => x.Website)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.Website));

        RuleFor(x => x.Industry)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Industry));
    }
}
