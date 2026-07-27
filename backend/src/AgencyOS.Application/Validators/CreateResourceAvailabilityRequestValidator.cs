using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class CreateResourceAvailabilityRequestValidator : AbstractValidator<CreateResourceAvailabilityRequest>
{
    public CreateResourceAvailabilityRequestValidator()
    {
        RuleFor(x => x.ExecutionResourceId)
            .NotEmpty()
            .WithMessage("Execution Resource is mandatory.");

        RuleFor(x => x.WorkingCalendarId)
            .NotEmpty()
            .WithMessage("Working Calendar is mandatory.");

        RuleFor(x => x.WorkingHoursId)
            .NotEmpty()
            .WithMessage("Working Hours configuration is mandatory.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Resource Availability name is mandatory.")
            .MaximumLength(200);

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty()
            .WithMessage("EffectiveFrom is mandatory.");

        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("EffectiveTo cannot be earlier than EffectiveFrom.");

        RuleFor(x => x.WeeklyAvailability)
            .NotNull()
            .Must(days => days.Any(day => day.Enabled))
            .WithMessage("At least one weekday must be enabled in weekly availability.");

        RuleFor(x => x.WeeklyAvailability)
            .Must(days =>
                days.Select(day => day.DayOfWeek.Trim().ToLowerInvariant()).Distinct().Count() == days.Count)
            .When(x => x.WeeklyAvailability is not null)
            .WithMessage("Weekly availability weekdays cannot be duplicated.");

        RuleForEach(x => x.WeeklyAvailability)
            .SetValidator(new ResourceAvailabilityWeekDayRequestValidator());

        RuleFor(x => x.DailyOverrides)
            .Must(overrides =>
                overrides.Select(day => day.OverrideDate).Distinct().Count() == overrides.Count)
            .When(x => x.DailyOverrides is not null)
            .WithMessage("Daily overrides cannot duplicate the same date.");

        RuleForEach(x => x.DailyOverrides)
            .SetValidator(new ResourceAvailabilityDayOverrideRequestValidator());

        RuleForEach(x => x.DailyOverrides)
            .Must((request, overrideDay) =>
                ResourceAvailabilityRules.CoversDate(
                    request.EffectiveFrom,
                    request.EffectiveTo,
                    overrideDay.OverrideDate))
            .When(x => x.DailyOverrides is not null)
            .WithMessage("Daily overrides must fall within the Resource Availability effective period.");
    }
}

public class UpdateResourceAvailabilityRequestValidator : AbstractValidator<UpdateResourceAvailabilityRequest>
{
    public UpdateResourceAvailabilityRequestValidator()
    {
        RuleFor(x => x.WorkingCalendarId)
            .NotEmpty()
            .WithMessage("Working Calendar is mandatory.");

        RuleFor(x => x.WorkingHoursId)
            .NotEmpty()
            .WithMessage("Working Hours configuration is mandatory.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Resource Availability name is mandatory.")
            .MaximumLength(200);

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty()
            .WithMessage("EffectiveFrom is mandatory.");

        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("EffectiveTo cannot be earlier than EffectiveFrom.");

        RuleFor(x => x.WeeklyAvailability)
            .NotNull()
            .Must(days => days.Any(day => day.Enabled))
            .WithMessage("At least one weekday must be enabled in weekly availability.");

        RuleFor(x => x.WeeklyAvailability)
            .Must(days =>
                days.Select(day => day.DayOfWeek.Trim().ToLowerInvariant()).Distinct().Count() == days.Count)
            .When(x => x.WeeklyAvailability is not null)
            .WithMessage("Weekly availability weekdays cannot be duplicated.");

        RuleForEach(x => x.WeeklyAvailability)
            .SetValidator(new ResourceAvailabilityWeekDayRequestValidator());

        RuleFor(x => x.DailyOverrides)
            .Must(overrides =>
                overrides.Select(day => day.OverrideDate).Distinct().Count() == overrides.Count)
            .When(x => x.DailyOverrides is not null)
            .WithMessage("Daily overrides cannot duplicate the same date.");

        RuleForEach(x => x.DailyOverrides)
            .SetValidator(new ResourceAvailabilityDayOverrideRequestValidator());

        RuleForEach(x => x.DailyOverrides)
            .Must((request, overrideDay) =>
                ResourceAvailabilityRules.CoversDate(
                    request.EffectiveFrom,
                    request.EffectiveTo,
                    overrideDay.OverrideDate))
            .When(x => x.DailyOverrides is not null)
            .WithMessage("Daily overrides must fall within the Resource Availability effective period.");
    }
}

public class ResourceAvailabilityWeekDayRequestValidator : AbstractValidator<ResourceAvailabilityWeekDayRequest>
{
    public ResourceAvailabilityWeekDayRequestValidator()
    {
        RuleFor(x => x.DayOfWeek)
            .NotEmpty()
            .Must(day => WorkingDayNames.All.Contains(WorkingDayNames.Canonicalize(day)))
            .WithMessage("DayOfWeek must be a valid weekday name.");
    }
}

public class ResourceAvailabilityDayOverrideRequestValidator : AbstractValidator<ResourceAvailabilityDayOverrideRequest>
{
    public ResourceAvailabilityDayOverrideRequestValidator()
    {
        RuleFor(x => x.OverrideDate)
            .NotEmpty();

        RuleFor(x => x)
            .Must(x =>
            {
                try
                {
                    ResourceAvailabilityRules.ValidateOverrideTimes(x.Available, x.StartTime, x.EndTime);
                    return true;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            })
            .WithMessage("Override StartTime and EndTime must both be provided or both omitted, and StartTime must be earlier than EndTime when available.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => x.Notes is not null);
    }
}
