using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class CreateContactRequestValidator : AbstractValidator<CreateContactRequest>
{
    public CreateContactRequestValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(status => ContactStatus.All.Contains(status))
            .WithMessage("Status must be a valid Contact status.");

        RuleFor(x => x.LastName)
            .MaximumLength(120)
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.JobTitle)
            .MaximumLength(120)
            .When(x => !string.IsNullOrWhiteSpace(x.JobTitle));

        RuleFor(x => x.Phone)
            .MaximumLength(40)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Mobile)
            .MaximumLength(40)
            .When(x => !string.IsNullOrWhiteSpace(x.Mobile));
    }
}
