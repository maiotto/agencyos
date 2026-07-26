using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class CreateContractRequestValidator : AbstractValidator<CreateContractRequest>
{
    public CreateContractRequestValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty();

        RuleFor(x => x.ContractCode)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.ContractName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ContractType)
            .NotEmpty()
            .Must(type => ContractType.All.Contains(type))
            .WithMessage("Contract Type must be a valid contract type.");

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(status => string.Equals(status, ContractStatus.Draft, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Status must be Draft when creating a Contract.");

        RuleFor(x => x.EstimatedValue)
            .GreaterThan(0);

        RuleFor(x => x.StartDate)
            .NotEmpty();

        RuleFor(x => x.EndDate)
            .Must((request, endDate) => !endDate.HasValue || endDate.Value >= request.StartDate)
            .WithMessage("End Date must not be before Start Date.")
            .When(x => x.EndDate.HasValue);
    }
}
