using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;

namespace AgencyOS.Application.Tests;

public class MissionRequestValidatorTests
{
    [Fact]
    public void CreateMissionRequestValidator_RejectsMissingRequiredFields()
    {
        var validator = new CreateMissionRequestValidator();

        var result = validator.Validate(new CreateMissionRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateMissionRequest.Code));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateMissionRequest.Name));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateMissionRequest.ClientContractId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateMissionRequest.MissionTypeId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateMissionRequest.MissionStatusId));
    }

    [Fact]
    public void CreateMissionRequestValidator_AcceptsValidRequest()
    {
        var validator = new CreateMissionRequestValidator();

        var result = validator.Validate(CreateValidCreateRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateMissionRequestValidator_RejectsEndDateBeforeStartDate()
    {
        var validator = new CreateMissionRequestValidator();
        var request = CreateValidCreateRequest();
        request.StartDate = new DateOnly(2026, 6, 30);
        request.EndDate = new DateOnly(2026, 1, 1);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateMissionRequest.EndDate));
    }

    [Fact]
    public void UpdateMissionRequestValidator_AcceptsValidRequest()
    {
        var validator = new UpdateMissionRequestValidator();

        var result = validator.Validate(new UpdateMissionRequest
        {
            ClientContractId = Guid.NewGuid(),
            Code = "MSN-2026-001",
            Name = "Acme Delivery Mission",
            MissionTypeId = Guid.NewGuid(),
            MissionStatusId = Guid.NewGuid(),
            Priority = "NORMAL"
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateMissionRequestValidator_RejectsMissingName()
    {
        var validator = new UpdateMissionRequestValidator();

        var result = validator.Validate(new UpdateMissionRequest
        {
            ClientContractId = Guid.NewGuid(),
            Code = "MSN-2026-001",
            MissionTypeId = Guid.NewGuid(),
            MissionStatusId = Guid.NewGuid()
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateMissionRequest.Name));
    }

    private static CreateMissionRequest CreateValidCreateRequest()
    {
        return new CreateMissionRequest
        {
            ClientContractId = Guid.NewGuid(),
            Code = "MSN-2026-001",
            Name = "Acme Delivery Mission",
            MissionTypeId = Guid.NewGuid(),
            MissionStatusId = Guid.NewGuid(),
            Priority = "NORMAL",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 6, 30)
        };
    }
}
