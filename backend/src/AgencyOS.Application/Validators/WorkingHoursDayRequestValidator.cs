using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class WorkingHoursDayRequestValidator : AbstractValidator<WorkingHoursDayRequest>
{
    public WorkingHoursDayRequestValidator()
    {
        RuleFor(x => x.DayOfWeek)
            .NotEmpty()
            .Must(day => WorkingDayNames.All.Contains(day))
            .WithMessage("DayOfWeek must be a valid weekday name.");

        When(x => x.Enabled, () =>
        {
            RuleFor(x => x.StartTime)
                .NotNull()
                .WithMessage("Enabled weekdays require StartTime and EndTime.");

            RuleFor(x => x.EndTime)
                .NotNull()
                .WithMessage("Enabled weekdays require StartTime and EndTime.");

            RuleFor(x => x)
                .Must(day => day.StartTime is null || day.EndTime is null || day.StartTime < day.EndTime)
                .WithMessage("StartTime must be earlier than EndTime.");

            RuleFor(x => x)
                .Must(day => day.BreakStart.HasValue == day.BreakEnd.HasValue)
                .WithMessage("If BreakStart exists, BreakEnd is mandatory.");

            RuleFor(x => x)
                .Must(day =>
                    !day.BreakStart.HasValue
                    || !day.BreakEnd.HasValue
                    || day.BreakStart.Value < day.BreakEnd.Value)
                .WithMessage("BreakStart must be earlier than BreakEnd.");

            RuleFor(x => x)
                .Must(day =>
                    !day.BreakStart.HasValue
                    || !day.BreakEnd.HasValue
                    || day.StartTime is null
                    || day.EndTime is null
                    || (day.BreakStart.Value >= day.StartTime.Value && day.BreakEnd.Value <= day.EndTime.Value))
                .WithMessage("Break period must be inside working period.");
        });
    }
}
