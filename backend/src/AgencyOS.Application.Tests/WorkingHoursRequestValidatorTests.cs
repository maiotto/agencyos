using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class WorkingHoursRequestValidatorTests
{
    [Fact]
    public void CreateWorkingHoursRequestValidator_RejectsMissingCalendarAndEnabledDays()
    {
        var validator = new CreateWorkingHoursRequestValidator();

        var result = validator.Validate(new CreateWorkingHoursRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateWorkingHoursRequest.WorkingCalendarId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateWorkingHoursRequest.Name));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateWorkingHoursRequest.Days));
    }

    [Fact]
    public void CreateWorkingHoursRequestValidator_AcceptsValidRequest()
    {
        var validator = new CreateWorkingHoursRequestValidator();

        var result = validator.Validate(CreateValidCreateRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateWorkingHoursRequestValidator_RejectsBreakWithoutEnd()
    {
        var validator = new CreateWorkingHoursRequestValidator();
        var request = CreateValidCreateRequest();
        request.Days = new[]
        {
            new WorkingHoursDayRequest
            {
                DayOfWeek = WorkingDayNames.Monday,
                Enabled = true,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(18, 0),
                BreakStart = new TimeOnly(12, 0)
            }
        };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateWorkingHoursRequestValidator_AcceptsValidRequest()
    {
        var validator = new UpdateWorkingHoursRequestValidator();

        var result = validator.Validate(new UpdateWorkingHoursRequest
        {
            Name = "Updated Hours",
            EffectiveFrom = new DateOnly(2099, 1, 1),
            Days = CreateValidCreateRequest().Days
        });

        Assert.True(result.IsValid);
    }

    private static CreateWorkingHoursRequest CreateValidCreateRequest() =>
        new()
        {
            WorkingCalendarId = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
            Name = "Standard Office Hours",
            EffectiveFrom = new DateOnly(2099, 1, 1),
            EffectiveTo = new DateOnly(2099, 12, 31),
            Days = WorkingDayNames.DefaultWeekdays
                .Select(day => new WorkingHoursDayRequest
                {
                    DayOfWeek = day,
                    Enabled = true,
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(18, 0),
                    BreakStart = new TimeOnly(12, 0),
                    BreakEnd = new TimeOnly(13, 0)
                })
                .ToList()
        };
}
