using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class ContractRequestValidatorTests
{
    [Fact]
    public void CreateContractRequestValidator_RejectsMissingRequiredFields()
    {
        var validator = new CreateContractRequestValidator();

        var result = validator.Validate(new CreateContractRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateContractRequest.ClientId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateContractRequest.ContractCode));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateContractRequest.ContractName));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateContractRequest.ContractType));
    }

    [Fact]
    public void CreateContractRequestValidator_AcceptsValidDraftRequest()
    {
        var validator = new CreateContractRequestValidator();

        var result = validator.Validate(CreateValidCreateRequest(ContractStatus.Draft));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("draft")]
    [InlineData("DRAFT")]
    public void CreateContractRequestValidator_AcceptsCaseInsensitiveDraft(string status)
    {
        var validator = new CreateContractRequestValidator();

        var result = validator.Validate(CreateValidCreateRequest(status));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateContractRequestValidator_RejectsNonDraftStatus()
    {
        var validator = new CreateContractRequestValidator();

        var result = validator.Validate(CreateValidCreateRequest(ContractStatus.Active));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateContractRequest.Status));
    }

    [Fact]
    public void CreateContractRequestValidator_RejectsInvalidContractType()
    {
        var validator = new CreateContractRequestValidator();
        var request = CreateValidCreateRequest(ContractStatus.Draft);
        request.ContractType = "Unknown";

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateContractRequest.ContractType));
    }

    [Fact]
    public void UpdateContractRequestValidator_AcceptsValidRequest()
    {
        var validator = new UpdateContractRequestValidator();

        var result = validator.Validate(new UpdateContractRequest
        {
            ContractCode = "CTR-2026-001",
            ContractName = "Acme Retainer 2026",
            ContractType = ContractType.MonthlyRetainer,
            Status = ContractStatus.Draft,
            EstimatedValue = 120000,
            StartDate = new DateOnly(2026, 1, 1)
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateContractRequestValidator_RejectsMissingContractName()
    {
        var validator = new UpdateContractRequestValidator();

        var result = validator.Validate(new UpdateContractRequest
        {
            ContractCode = "CTR-2026-001",
            ContractType = ContractType.MonthlyRetainer,
            Status = ContractStatus.Draft,
            EstimatedValue = 120000,
            StartDate = new DateOnly(2026, 1, 1)
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateContractRequest.ContractName));
    }

    private static CreateContractRequest CreateValidCreateRequest(string status)
    {
        return new CreateContractRequest
        {
            ClientId = Guid.NewGuid(),
            ContractCode = "CTR-2026-001",
            ContractName = "Acme Retainer 2026",
            ContractType = ContractType.MonthlyRetainer,
            Status = status,
            EstimatedValue = 120000,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31)
        };
    }
}
