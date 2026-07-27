using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;

namespace AgencyOS.Application.Tests;

public class DecisionWorkspaceQueryParametersValidatorTests
{
    private readonly DecisionWorkspaceQueryParametersValidator _validator = new();

    [Fact]
    public void Validate_Succeeds_WhenEmpty()
    {
        var result = _validator.Validate(new DecisionWorkspaceQueryParameters());
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenToBeforeFrom()
    {
        var now = DateTimeOffset.UtcNow;
        var result = _validator.Validate(new DecisionWorkspaceQueryParameters
        {
            From = now,
            To = now.AddDays(-1)
        });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_Succeeds_WhenFromEqualsTo()
    {
        var now = DateTimeOffset.UtcNow;
        var result = _validator.Validate(new DecisionWorkspaceQueryParameters { From = now, To = now });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Succeeds_WhenToAfterFrom()
    {
        var now = DateTimeOffset.UtcNow;
        var result = _validator.Validate(new DecisionWorkspaceQueryParameters { From = now.AddDays(-5), To = now });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Succeeds_WhenOnlyDecisionIdSet()
    {
        var result = _validator.Validate(new DecisionWorkspaceQueryParameters { DecisionId = Guid.NewGuid() });
        Assert.True(result.IsValid);
    }
}
