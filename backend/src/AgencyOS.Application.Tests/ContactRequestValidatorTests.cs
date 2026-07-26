using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class ContactRequestValidatorTests
{
    [Fact]
    public void CreateContactRequestValidator_RejectsMissingRequiredFields()
    {
        var validator = new CreateContactRequestValidator();

        var result = validator.Validate(new CreateContactRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateContactRequest.ClientId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateContactRequest.FirstName));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateContactRequest.Email));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateContactRequest.Status));
    }

    [Fact]
    public void CreateContactRequestValidator_AcceptsValidRequest()
    {
        var validator = new CreateContactRequestValidator();

        var result = validator.Validate(new CreateContactRequest
        {
            ClientId = Guid.NewGuid(),
            FirstName = "Alex",
            Email = "alex@acme.example",
            Status = ContactStatus.Active
        });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Active")]
    [InlineData("active")]
    [InlineData("INACTIVE")]
    public void CreateContactRequestValidator_AcceptsCaseInsensitiveStatus(string status)
    {
        var validator = new CreateContactRequestValidator();

        var result = validator.Validate(new CreateContactRequest
        {
            ClientId = Guid.NewGuid(),
            FirstName = "Alex",
            Email = "alex@acme.example",
            Status = status
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateContactRequestValidator_RejectsInvalidStatus()
    {
        var validator = new CreateContactRequestValidator();

        var result = validator.Validate(new CreateContactRequest
        {
            ClientId = Guid.NewGuid(),
            FirstName = "Alex",
            Email = "alex@acme.example",
            Status = "Archived"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateContactRequest.Status));
    }

    [Fact]
    public void CreateContactRequestValidator_RejectsInvalidEmail()
    {
        var validator = new CreateContactRequestValidator();

        var result = validator.Validate(new CreateContactRequest
        {
            ClientId = Guid.NewGuid(),
            FirstName = "Alex",
            Email = "not-an-email",
            Status = ContactStatus.Active
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateContactRequest.Email));
    }

    [Fact]
    public void UpdateContactRequestValidator_RejectsMissingFirstName()
    {
        var validator = new UpdateContactRequestValidator();

        var result = validator.Validate(new UpdateContactRequest
        {
            Email = "alex@acme.example",
            Status = ContactStatus.Active
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateContactRequest.FirstName));
    }

    [Fact]
    public void UpdateContactRequestValidator_AcceptsValidRequest()
    {
        var validator = new UpdateContactRequestValidator();

        var result = validator.Validate(new UpdateContactRequest
        {
            FirstName = "Alex",
            Email = "alex@acme.example",
            Mobile = "+1-555-0199",
            Status = ContactStatus.Active
        });

        Assert.True(result.IsValid);
    }
}
