using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class SelectCompanyRequestValidator : AbstractValidator<SelectCompanyRequest>
{
    public SelectCompanyRequestValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("CompanyId is mandatory (BR-2003).");
    }
}
