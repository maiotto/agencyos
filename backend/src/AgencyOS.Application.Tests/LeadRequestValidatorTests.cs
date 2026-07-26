using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class LeadRequestValidatorTests
{
    [Fact]
    public void CreateLeadRequestValidator_RejectsLeadNameLongerThanContactFirstName()
    {
        var validator = new CreateLeadRequestValidator();
        var request = new CreateLeadRequest
        {
            CompanyName = "Acme",
            LeadName = new string('A', 121),
            Source = "Referral",
            Status = LeadStatus.Prospect
        };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateLeadRequest.LeadName));
    }

    [Fact]
    public void CreateLeadRequestValidator_AcceptsLeadNameAtContactFirstNameLimit()
    {
        var validator = new CreateLeadRequestValidator();
        var request = new CreateLeadRequest
        {
            CompanyName = "Acme",
            LeadName = new string('A', 120),
            Source = "Referral",
            Status = LeadStatus.Prospect
        };

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateLeadRequestValidator_RejectsLeadNameLongerThanContactFirstName()
    {
        var validator = new UpdateLeadRequestValidator();
        var request = new UpdateLeadRequest
        {
            CompanyName = "Acme",
            LeadName = new string('B', 121),
            Source = "Referral",
            Status = LeadStatus.Qualified
        };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateLeadRequest.LeadName));
    }

    [Fact]
    public void CreateLeadRequestValidator_RejectsMissingRequiredFields()
    {
        var validator = new CreateLeadRequestValidator();

        var result = validator.Validate(new CreateLeadRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateLeadRequest.CompanyName));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateLeadRequest.LeadName));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateLeadRequest.Source));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateLeadRequest.Status));
    }
}
