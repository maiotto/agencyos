using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;

namespace AgencyOS.Application.Tests;

public class PlanningWorkspaceQueryParametersValidatorTests
{
    private readonly PlanningWorkspaceQueryParametersValidator _validator = new();

    [Fact]
    public void Validate_Succeeds_WhenEmpty()
    {
        var result = _validator.Validate(new PlanningWorkspaceQueryParameters());
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenToBeforeFrom()
    {
        var now = DateTimeOffset.UtcNow;
        var result = _validator.Validate(new PlanningWorkspaceQueryParameters
        {
            From = now,
            To = now.AddDays(-1)
        });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenPeriodEndBeforePeriodStart()
    {
        var result = _validator.Validate(new PlanningWorkspaceQueryParameters
        {
            PeriodStart = new DateOnly(2026, 6, 30),
            PeriodEnd = new DateOnly(2026, 6, 1)
        });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_Succeeds_WhenPeriodEqual()
    {
        var date = new DateOnly(2026, 6, 15);
        var result = _validator.Validate(new PlanningWorkspaceQueryParameters
        {
            PeriodStart = date,
            PeriodEnd = date
        });
        Assert.True(result.IsValid);
    }
}
