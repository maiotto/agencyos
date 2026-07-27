using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class ResourceAvailabilityRequestValidatorTests
{
    [Fact]
    public void CreateValidator_RequiresMandatoryAssociations()
    {
        var validator = new CreateResourceAvailabilityRequestValidator();
        var result = validator.Validate(new CreateResourceAvailabilityRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateResourceAvailabilityRequest.ExecutionResourceId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateResourceAvailabilityRequest.WorkingCalendarId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateResourceAvailabilityRequest.WorkingHoursId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateResourceAvailabilityRequest.Name));
    }

    [Fact]
    public void CreateValidator_AcceptsValidRequest()
    {
        var validator = new CreateResourceAvailabilityRequestValidator();
        var result = validator.Validate(new CreateResourceAvailabilityRequest
        {
            ExecutionResourceId = Guid.NewGuid(),
            WorkingCalendarId = Guid.NewGuid(),
            WorkingHoursId = Guid.NewGuid(),
            Name = "Standard",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            WeeklyAvailability = WorkingDayNames.Ordered.Select(day => new ResourceAvailabilityWeekDayRequest
            {
                DayOfWeek = day,
                Enabled = WorkingDayNames.DefaultWeekdays.Contains(day)
            }).ToList(),
            DailyOverrides = []
        });

        Assert.True(result.IsValid);
    }
}
