using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.TaskTypeId)
            .NotEmpty();

        RuleFor(x => x.TaskStatusId)
            .NotEmpty();

        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(priority => TaskPriority.All.Contains(priority))
            .WithMessage("Priority must be a valid Task priority.");

        RuleFor(x => x.EstimatedHours)
            .GreaterThan(0);

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => x.Description is not null);

        RuleFor(x => x.PlannedEndDate)
            .Must((request, plannedEndDate) =>
                !request.PlannedStartDate.HasValue
                || !plannedEndDate.HasValue
                || plannedEndDate >= request.PlannedStartDate)
            .WithMessage("Planned End Date must be greater than or equal to Planned Start Date when both dates are provided.");
    }
}
