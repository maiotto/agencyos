using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class CreateExecutionResourceRequestValidator : AbstractValidator<CreateExecutionResourceRequest>
{
    public CreateExecutionResourceRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ResourceType)
            .NotEmpty()
            .Must(resourceType => ExecutionResourceType.All.Contains(resourceType))
            .WithMessage("Resource Type must be a valid Execution Resource type.");

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(status => ExecutionResourceStatus.All.Contains(status))
            .WithMessage("Status must be a valid Execution Resource status.");

        RuleFor(x => x.CapacityHoursPerWeek)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .MaximumLength(3)
            .When(x => !string.IsNullOrWhiteSpace(x.Currency));

        RuleFor(x => x.Availability)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrWhiteSpace(x.Availability));

        RuleFor(x => x.Notes)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));

        RuleForEach(x => x.Skills)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.Skills is not null);
    }
}
