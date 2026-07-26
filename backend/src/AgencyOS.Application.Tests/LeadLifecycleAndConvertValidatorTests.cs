using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class LeadLifecycleAndConvertValidatorTests
{
    [Theory]
    [InlineData(LeadStatus.Prospect, LeadStatus.Qualified, true)]
    [InlineData(LeadStatus.Qualified, LeadStatus.Proposal, true)]
    [InlineData(LeadStatus.Proposal, LeadStatus.Negotiation, true)]
    [InlineData(LeadStatus.Negotiation, LeadStatus.Won, true)]
    [InlineData(LeadStatus.Won, LeadStatus.Lost, true)]
    [InlineData(LeadStatus.Won, LeadStatus.Archived, true)]
    [InlineData(LeadStatus.Prospect, LeadStatus.Archived, true)]
    [InlineData(LeadStatus.Prospect, LeadStatus.Won, false)]
    [InlineData(LeadStatus.Won, LeadStatus.Converted, false)]
    [InlineData(LeadStatus.Converted, LeadStatus.Archived, false)]
    [InlineData(LeadStatus.Lost, LeadStatus.Won, false)]
    [InlineData(LeadStatus.Prospect, LeadStatus.Lost, false)]
    public void CanTransition_EnforcesApprovedLifecycle(string from, string to, bool expected)
    {
        Assert.Equal(expected, LeadStatus.CanTransition(from, to));
    }

    [Fact]
    public void Convertible_ContainsOnlyWon()
    {
        Assert.Single(LeadStatus.Convertible);
        Assert.Contains(LeadStatus.Won, LeadStatus.Convertible);
    }

    [Fact]
    public void ConvertLeadRequestValidator_RequiresLegalNameAndTaxIdentifier()
    {
        var validator = new ConvertLeadRequestValidator();

        var result = validator.Validate(new ConvertLeadRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ConvertLeadRequest.LegalName));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ConvertLeadRequest.TaxIdentifier));
    }

    [Fact]
    public void ConvertLeadRequestValidator_AcceptsValidRequest()
    {
        var validator = new ConvertLeadRequestValidator();

        var result = validator.Validate(new ConvertLeadRequest
        {
            LegalName = "Acme Corporation Ltd",
            TaxIdentifier = "12.345.678/0001-90"
        });

        Assert.True(result.IsValid);
    }
}
