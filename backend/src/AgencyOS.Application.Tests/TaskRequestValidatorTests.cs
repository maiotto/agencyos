using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class TaskRequestValidatorTests
{
    [Fact]
    public void CreateTaskRequestValidator_RejectsMissingRequiredFields()
    {
        var validator = new CreateTaskRequestValidator();

        var result = validator.Validate(new CreateTaskRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateTaskRequest.MissionId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateTaskRequest.Code));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateTaskRequest.Name));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateTaskRequest.TaskTypeId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateTaskRequest.TaskStatusId));
    }

    [Fact]
    public void CreateTaskRequestValidator_AcceptsValidRequest()
    {
        var validator = new CreateTaskRequestValidator();

        var result = validator.Validate(CreateValidCreateRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateTaskRequestValidator_RejectsInvalidPriority()
    {
        var validator = new CreateTaskRequestValidator();
        var request = CreateValidCreateRequest();
        request.Priority = "Urgent";

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateTaskRequest.Priority));
    }

    [Fact]
    public void CreateTaskRequestValidator_RejectsPlannedEndBeforeStart()
    {
        var validator = new CreateTaskRequestValidator();
        var request = CreateValidCreateRequest();
        request.PlannedStartDate = new DateOnly(2026, 1, 10);
        request.PlannedEndDate = new DateOnly(2026, 1, 5);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateTaskRequest.PlannedEndDate));
    }

    [Fact]
    public void UpdateTaskRequestValidator_AcceptsValidRequest()
    {
        var validator = new UpdateTaskRequestValidator();

        var result = validator.Validate(new UpdateTaskRequest
        {
            Code = "TSK-001",
            Name = "Discovery Workshop",
            TaskTypeId = Guid.NewGuid(),
            TaskStatusId = Guid.NewGuid(),
            Priority = TaskPriority.Medium,
            EstimatedHours = 8
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateTaskRequestValidator_RejectsMissingName()
    {
        var validator = new UpdateTaskRequestValidator();

        var result = validator.Validate(new UpdateTaskRequest
        {
            Code = "TSK-001",
            TaskTypeId = Guid.NewGuid(),
            TaskStatusId = Guid.NewGuid(),
            Priority = TaskPriority.Medium,
            EstimatedHours = 8
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateTaskRequest.Name));
    }

    private static CreateTaskRequest CreateValidCreateRequest()
    {
        return new CreateTaskRequest
        {
            MissionId = Guid.NewGuid(),
            Code = "TSK-001",
            Name = "Discovery Workshop",
            TaskTypeId = Guid.NewGuid(),
            TaskStatusId = Guid.NewGuid(),
            Priority = TaskPriority.High,
            EstimatedHours = 16,
            PlannedStartDate = new DateOnly(2026, 1, 5),
            PlannedEndDate = new DateOnly(2026, 1, 9)
        };
    }
}
