using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class UpdateAssignmentRequestValidator : AbstractValidator<UpdateAssignmentRequest>
{
    public UpdateAssignmentRequestValidator()
    {
        RuleFor(x => x.AssignmentRole)
            .NotEmpty()
            .Must(role => AssignmentRole.All.Contains(role))
            .WithMessage("Assignment Role must be a valid role.");

        RuleFor(x => x.PlannedHours)
            .GreaterThan(0);

        RuleFor(x => x.PlannedStartDate)
            .NotEmpty();

        RuleFor(x => x.PlannedEndDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(x => x.PlannedStartDate)
            .WithMessage("Planned End Date must not be before Planned Start Date.");

        RuleFor(x => x.AllocationPercentage)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(status => AssignmentStatus.All.Contains(status))
            .WithMessage("Status must be a valid Assignment status.");

        RuleFor(x => x.Notes)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
