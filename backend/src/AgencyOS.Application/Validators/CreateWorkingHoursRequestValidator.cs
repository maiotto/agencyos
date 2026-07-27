using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class CreateWorkingHoursRequestValidator : AbstractValidator<CreateWorkingHoursRequest>
{
    public CreateWorkingHoursRequestValidator()
    {
        RuleFor(x => x.WorkingCalendarId)
            .NotEmpty()
            .WithMessage("Working Calendar is mandatory.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Working Hours name is mandatory.")
            .MaximumLength(200);

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty();

        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("EffectiveTo cannot be earlier than EffectiveFrom.");

        RuleFor(x => x.Days)
            .NotNull()
            .Must(days => days.Any(day => day.Enabled))
            .WithMessage("At least one enabled weekday is required.");

        RuleFor(x => x.Days)
            .Must(days =>
                days.Select(day => day.DayOfWeek.Trim().ToLowerInvariant()).Distinct().Count() == days.Count)
            .When(x => x.Days is not null)
            .WithMessage("Weekday schedules cannot be duplicated.");

        RuleForEach(x => x.Days)
            .SetValidator(new WorkingHoursDayRequestValidator());
    }
}
