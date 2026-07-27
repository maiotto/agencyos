using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class WorkingCalendarRequestValidatorTests
{
    [Fact]
    public void CreateWorkingCalendarRequestValidator_RejectsMissingRequiredFields()
    {
        var validator = new CreateWorkingCalendarRequestValidator();

        var result = validator.Validate(new CreateWorkingCalendarRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateWorkingCalendarRequest.CompanyId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateWorkingCalendarRequest.Name));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateWorkingCalendarRequest.EffectiveFrom));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateWorkingCalendarRequest.WorkingDays));
    }

    [Fact]
    public void CreateWorkingCalendarRequestValidator_AcceptsValidRequest()
    {
        var validator = new CreateWorkingCalendarRequestValidator();

        var result = validator.Validate(CreateValidCreateRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateWorkingCalendarRequestValidator_RejectsEffectiveToEarlierThanEffectiveFrom()
    {
        var validator = new CreateWorkingCalendarRequestValidator();
        var request = CreateValidCreateRequest();
        request.EffectiveTo = request.EffectiveFrom.AddDays(-1);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateWorkingCalendarRequest.EffectiveTo));
    }

    [Fact]
    public void CreateWorkingCalendarRequestValidator_RejectsEmptyWorkingDays()
    {
        var validator = new CreateWorkingCalendarRequestValidator();
        var request = CreateValidCreateRequest();
        request.WorkingDays = [];

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateWorkingCalendarRequest.WorkingDays));
    }

    [Fact]
    public void CreateWorkingCalendarRequestValidator_RejectsDuplicateWorkingDays()
    {
        var validator = new CreateWorkingCalendarRequestValidator();
        var request = CreateValidCreateRequest();
        request.WorkingDays = ["Monday", "monday"];

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateWorkingCalendarRequest.WorkingDays));
    }

    [Fact]
    public void CreateWorkingCalendarRequestValidator_RejectsUnknownWorkingDay()
    {
        var validator = new CreateWorkingCalendarRequestValidator();
        var request = CreateValidCreateRequest();
        request.WorkingDays = ["Funday"];

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName.StartsWith(nameof(CreateWorkingCalendarRequest.WorkingDays)));
    }

    [Fact]
    public void UpdateWorkingCalendarRequestValidator_AcceptsValidRequest()
    {
        var validator = new UpdateWorkingCalendarRequestValidator();

        var result = validator.Validate(CreateValidUpdateRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateWorkingCalendarRequestValidator_RejectsMissingName()
    {
        var validator = new UpdateWorkingCalendarRequestValidator();
        var request = CreateValidUpdateRequest();
        request.Name = " ";

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateWorkingCalendarRequest.Name));
    }

    private static CreateWorkingCalendarRequest CreateValidCreateRequest()
    {
        return new CreateWorkingCalendarRequest
        {
            CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Name = "Standard Agency Week",
            EffectiveFrom = new DateOnly(2026, 8, 1),
            EffectiveTo = new DateOnly(2026, 12, 31),
            WorkingDays = WorkingDayNames.DefaultWeekdays
        };
    }

    private static UpdateWorkingCalendarRequest CreateValidUpdateRequest()
    {
        return new UpdateWorkingCalendarRequest
        {
            Name = "Standard Agency Week",
            EffectiveFrom = new DateOnly(2026, 8, 1),
            EffectiveTo = new DateOnly(2026, 12, 31),
            WorkingDays = WorkingDayNames.DefaultWeekdays
        };
    }
}
