using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;

namespace AgencyOS.Application.Tests;

public class ExecutiveWorkspaceQueryParametersValidatorTests
{
    private readonly ExecutiveWorkspaceQueryParametersValidator _validator = new();

    [Fact]
    public void Validate_Succeeds_WhenEmpty()
    {
        var result = _validator.Validate(new ExecutiveWorkspaceQueryParameters());
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenToBeforeFrom()
    {
        var now = DateTimeOffset.UtcNow;
        var result = _validator.Validate(new ExecutiveWorkspaceQueryParameters
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
        var result = _validator.Validate(new ExecutiveWorkspaceQueryParameters { From = now, To = now });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Succeeds_WhenToAfterFrom()
    {
        var now = DateTimeOffset.UtcNow;
        var result = _validator.Validate(new ExecutiveWorkspaceQueryParameters { From = now.AddDays(-5), To = now });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenPeriodEndBeforePeriodStart()
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var result = _validator.Validate(new ExecutiveWorkspaceQueryParameters
        {
            PeriodStart = today,
            PeriodEnd = today.AddDays(-1)
        });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_Succeeds_WhenPeriodStartEqualsPeriodEnd()
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var result = _validator.Validate(new ExecutiveWorkspaceQueryParameters { PeriodStart = today, PeriodEnd = today });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Succeeds_WhenPeriodEndAfterPeriodStart()
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var result = _validator.Validate(new ExecutiveWorkspaceQueryParameters
        {
            PeriodStart = today.AddDays(-10),
            PeriodEnd = today
        });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Succeeds_WhenOnlyCompanyIdSet()
    {
        var result = _validator.Validate(new ExecutiveWorkspaceQueryParameters { CompanyId = Guid.NewGuid() });
        Assert.True(result.IsValid);
    }
}
