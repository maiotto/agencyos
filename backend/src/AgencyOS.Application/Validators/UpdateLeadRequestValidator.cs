using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class UpdateLeadRequestValidator : AbstractValidator<UpdateLeadRequest>
{
    public UpdateLeadRequestValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.LeadName)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Source)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(status => LeadStatus.All.Contains(status))
            .WithMessage("Status must be a valid Lead status.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(40)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Website)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.Website));

        RuleFor(x => x.Segment)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Segment));

        RuleFor(x => x.EstimatedContractValue)
            .GreaterThanOrEqualTo(0)
            .When(x => x.EstimatedContractValue.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
