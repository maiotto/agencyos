using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class CreateWorkingCalendarRequestValidator : AbstractValidator<CreateWorkingCalendarRequest>
{
    public CreateWorkingCalendarRequestValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("CompanyId is mandatory.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Calendar Name is mandatory.")
            .MaximumLength(200);

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty()
            .WithMessage("EffectiveFrom is mandatory.");

        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("EffectiveTo cannot be earlier than EffectiveFrom.");

        RuleFor(x => x.WorkingDays)
            .NotNull()
            .Must(WorkingCalendarRules.HasAtLeastOneWorkingDay)
            .WithMessage("Calendar must contain at least one working day.");

        RuleFor(x => x.WorkingDays)
            .Must(days => !WorkingCalendarRules.HasDuplicateWorkingDays(days))
            .When(x => x.WorkingDays is not null)
            .WithMessage("Working days cannot be duplicated.");

        RuleForEach(x => x.WorkingDays)
            .NotEmpty()
            .Must(day => WorkingDayNames.All.Contains(day))
            .WithMessage("Working day must be a valid weekday name.");
    }
}
