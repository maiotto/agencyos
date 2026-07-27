using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class ExecutionResourceRequestValidatorTests
{
    [Fact]
    public void CreateExecutionResourceRequestValidator_RejectsMissingRequiredFields()
    {
        var validator = new CreateExecutionResourceRequestValidator();

        var result = validator.Validate(new CreateExecutionResourceRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateExecutionResourceRequest.Code));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateExecutionResourceRequest.Name));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateExecutionResourceRequest.ResourceType));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateExecutionResourceRequest.Status));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateExecutionResourceRequest.CapacityHoursPerWeek));
    }

    [Fact]
    public void CreateExecutionResourceRequestValidator_AcceptsValidRequest()
    {
        var validator = new CreateExecutionResourceRequestValidator();

        var result = validator.Validate(CreateValidCreateRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateExecutionResourceRequestValidator_RejectsUnknownResourceType()
    {
        var validator = new CreateExecutionResourceRequestValidator();
        var request = CreateValidCreateRequest();
        request.ResourceType = "Robot";

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateExecutionResourceRequest.ResourceType));
    }

    [Fact]
    public void CreateExecutionResourceRequestValidator_RejectsUnknownStatus()
    {
        var validator = new CreateExecutionResourceRequestValidator();
        var request = CreateValidCreateRequest();
        request.Status = "Archived";

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateExecutionResourceRequest.Status));
    }

    [Fact]
    public void CreateExecutionResourceRequestValidator_RejectsNonPositiveCapacity()
    {
        var validator = new CreateExecutionResourceRequestValidator();
        var request = CreateValidCreateRequest();
        request.CapacityHoursPerWeek = 0;

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateExecutionResourceRequest.CapacityHoursPerWeek));
    }

    [Fact]
    public void CreateExecutionResourceRequestValidator_RejectsCurrencyLongerThanThreeCharacters()
    {
        var validator = new CreateExecutionResourceRequestValidator();
        var request = CreateValidCreateRequest();
        request.Currency = "BRLX";

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateExecutionResourceRequest.Currency));
    }

    [Fact]
    public void UpdateExecutionResourceRequestValidator_AcceptsValidRequest()
    {
        var validator = new UpdateExecutionResourceRequestValidator();

        var result = validator.Validate(CreateValidUpdateRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateExecutionResourceRequestValidator_RejectsMissingName()
    {
        var validator = new UpdateExecutionResourceRequestValidator();
        var request = CreateValidUpdateRequest();
        request.Name = string.Empty;

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateExecutionResourceRequest.Name));
    }

    [Fact]
    public void UpdateExecutionResourceRequestValidator_RejectsEmptySkill()
    {
        var validator = new UpdateExecutionResourceRequestValidator();
        var request = CreateValidUpdateRequest();
        request.Skills = new[] { "Discovery", string.Empty };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
    }

    private static CreateExecutionResourceRequest CreateValidCreateRequest()
    {
        return new CreateExecutionResourceRequest
        {
            Code = "RES-001",
            Name = "Senior Delivery Consultant",
            ResourceType = ExecutionResourceType.InternalHuman,
            Status = ExecutionResourceStatus.Active,
            CapacityHoursPerWeek = 40,
            CostRate = 120,
            Currency = "BRL",
            Skills = new[] { "Discovery" },
            Availability = "Monday to Friday, business hours",
            Notes = "Allocated to strategic accounts"
        };
    }

    private static UpdateExecutionResourceRequest CreateValidUpdateRequest()
    {
        return new UpdateExecutionResourceRequest
        {
            Code = "RES-001",
            Name = "Senior Delivery Consultant",
            ResourceType = ExecutionResourceType.InternalHuman,
            Status = ExecutionResourceStatus.Active,
            CapacityHoursPerWeek = 40,
            CostRate = 120,
            Currency = "BRL",
            Skills = new[] { "Discovery" },
            Availability = "Monday to Friday, business hours",
            Notes = "Allocated to strategic accounts"
        };
    }
}
