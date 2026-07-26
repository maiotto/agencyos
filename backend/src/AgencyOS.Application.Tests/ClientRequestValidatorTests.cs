using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class ClientRequestValidatorTests
{
    [Fact]
    public void CreateClientRequestValidator_RejectsMissingRequiredFields()
    {
        var validator = new CreateClientRequestValidator();

        var result = validator.Validate(new CreateClientRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateClientRequest.LegalName));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateClientRequest.Status));
    }

    [Fact]
    public void CreateClientRequestValidator_AcceptsValidRequest()
    {
        var validator = new CreateClientRequestValidator();

        var result = validator.Validate(new CreateClientRequest
        {
            LegalName = "Acme Corporation Ltd.",
            TaxIdentifier = "12-3456789",
            Status = ClientStatus.Active
        });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Active")]
    [InlineData("active")]
    [InlineData("INACTIVE")]
    public void CreateClientRequestValidator_AcceptsCaseInsensitiveStatus(string status)
    {
        var validator = new CreateClientRequestValidator();

        var result = validator.Validate(new CreateClientRequest
        {
            LegalName = "Acme",
            Status = status
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateClientRequestValidator_RejectsInvalidStatus()
    {
        var validator = new CreateClientRequestValidator();

        var result = validator.Validate(new CreateClientRequest
        {
            LegalName = "Acme",
            Status = "Suspended"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateClientRequest.Status));
    }

    [Fact]
    public void UpdateClientRequestValidator_RejectsMissingLegalName()
    {
        var validator = new UpdateClientRequestValidator();

        var result = validator.Validate(new UpdateClientRequest
        {
            Status = ClientStatus.Active
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateClientRequest.LegalName));
    }

    [Fact]
    public void ClientStatus_Normalize_ReturnsCanonicalConstants()
    {
        Assert.Equal(ClientStatus.Active, ClientStatus.Normalize("active"));
        Assert.Equal(ClientStatus.Inactive, ClientStatus.Normalize("INACTIVE"));
    }
}
