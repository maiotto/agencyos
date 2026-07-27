using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class AssignmentRequestValidatorTests
{
    [Fact]
    public void CreateAssignmentRequestValidator_RejectsMissingRequiredFields()
    {
        var validator = new CreateAssignmentRequestValidator();

        var result = validator.Validate(new CreateAssignmentRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateAssignmentRequest.TaskId));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateAssignmentRequest.ExecutionResourceId));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateAssignmentRequest.AssignmentRole));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateAssignmentRequest.Status));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateAssignmentRequest.PlannedHours));
    }

    [Fact]
    public void CreateAssignmentRequestValidator_AcceptsValidRequest()
    {
        var validator = new CreateAssignmentRequestValidator();

        var result = validator.Validate(CreateValidCreateRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateAssignmentRequestValidator_RejectsUnknownRole()
    {
        var validator = new CreateAssignmentRequestValidator();
        var request = CreateValidCreateRequest();
        request.AssignmentRole = "Owner";

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateAssignmentRequest.AssignmentRole));
    }

    [Fact]
    public void CreateAssignmentRequestValidator_RejectsUnknownStatus()
    {
        var validator = new CreateAssignmentRequestValidator();
        var request = CreateValidCreateRequest();
        request.Status = "Allocated";

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateAssignmentRequest.Status));
    }

    [Fact]
    public void CreateAssignmentRequestValidator_RejectsPlannedEndBeforeStart()
    {
        var validator = new CreateAssignmentRequestValidator();
        var request = CreateValidCreateRequest();
        request.PlannedStartDate = new DateOnly(2026, 1, 10);
        request.PlannedEndDate = new DateOnly(2026, 1, 5);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateAssignmentRequest.PlannedEndDate));
    }

    [Fact]
    public void CreateAssignmentRequestValidator_RejectsAllocationOutsideRange()
    {
        var validator = new CreateAssignmentRequestValidator();
        var request = CreateValidCreateRequest();
        request.AllocationPercentage = 0;

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateAssignmentRequest.AllocationPercentage));
    }

    [Fact]
    public void UpdateAssignmentRequestValidator_AcceptsValidRequest()
    {
        var validator = new UpdateAssignmentRequestValidator();

        var result = validator.Validate(CreateValidUpdateRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateAssignmentRequestValidator_RejectsMissingRole()
    {
        var validator = new UpdateAssignmentRequestValidator();
        var request = CreateValidUpdateRequest();
        request.AssignmentRole = string.Empty;

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateAssignmentRequest.AssignmentRole));
    }

    private static CreateAssignmentRequest CreateValidCreateRequest()
    {
        return new CreateAssignmentRequest
        {
            TaskId = Guid.NewGuid(),
            ExecutionResourceId = Guid.NewGuid(),
            AssignmentRole = AssignmentRole.Responsible,
            PlannedHours = 24,
            PlannedStartDate = new DateOnly(2026, 1, 5),
            PlannedEndDate = new DateOnly(2026, 1, 9),
            AllocationPercentage = 50,
            Status = AssignmentStatus.Planned
        };
    }

    private static UpdateAssignmentRequest CreateValidUpdateRequest()
    {
        return new UpdateAssignmentRequest
        {
            AssignmentRole = AssignmentRole.Contributor,
            PlannedHours = 16,
            PlannedStartDate = new DateOnly(2026, 1, 5),
            PlannedEndDate = new DateOnly(2026, 1, 8),
            AllocationPercentage = 25,
            Status = AssignmentStatus.Confirmed
        };
    }
}
