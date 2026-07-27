using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;

namespace AgencyOS.Application.Tests;

public class CompanyRequestValidatorTests
{
    private readonly CreateCompanyRequestValidator _createValidator = new();
    private readonly UpdateCompanyRequestValidator _updateValidator = new();
    private readonly SelectCompanyRequestValidator _selectValidator = new();

    [Fact]
    public void Create_ValidRequest_PassesValidation()
    {
        var result = _createValidator.Validate(CreateValidRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Create_MissingCompanyCode_FailsValidation_BR2002()
    {
        var request = CreateValidRequest();
        request.CompanyCode = string.Empty;

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateCompanyRequest.CompanyCode));
    }

    [Fact]
    public void Create_MissingCompanyName_FailsValidation_BR2001()
    {
        var request = CreateValidRequest();
        request.CompanyName = string.Empty;

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateCompanyRequest.CompanyName));
    }

    [Fact]
    public void Create_MissingTimezone_FailsValidation()
    {
        var request = CreateValidRequest();
        request.Timezone = string.Empty;

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateCompanyRequest.Timezone));
    }

    [Fact]
    public void Create_CompanyCodeTooLong_FailsValidation()
    {
        var request = CreateValidRequest();
        request.CompanyCode = new string('A', 51);

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Update_ValidRequest_PassesValidation()
    {
        var result = _updateValidator.Validate(new UpdateCompanyRequest
        {
            CompanyName = "Updated Name",
            Timezone = "UTC"
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Update_MissingCompanyName_FailsValidation_BR2001()
    {
        var result = _updateValidator.Validate(new UpdateCompanyRequest
        {
            CompanyName = string.Empty,
            Timezone = "UTC"
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Update_MissingTimezone_FailsValidation()
    {
        var result = _updateValidator.Validate(new UpdateCompanyRequest
        {
            CompanyName = "Name",
            Timezone = string.Empty
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Select_EmptyCompanyId_FailsValidation_BR2003()
    {
        var result = _selectValidator.Validate(new SelectCompanyRequest { CompanyId = Guid.Empty });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Select_ValidCompanyId_PassesValidation()
    {
        var result = _selectValidator.Validate(new SelectCompanyRequest { CompanyId = Guid.NewGuid() });

        Assert.True(result.IsValid);
    }

    private static CreateCompanyRequest CreateValidRequest() =>
        new()
        {
            CompanyCode = "ACME",
            CompanyName = "Acme Agency",
            Timezone = "UTC"
        };
}
