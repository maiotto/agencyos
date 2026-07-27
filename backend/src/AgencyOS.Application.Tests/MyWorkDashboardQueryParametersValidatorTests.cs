using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;

namespace AgencyOS.Application.Tests;

public class MyWorkDashboardQueryParametersValidatorTests
{
    private readonly MyWorkDashboardQueryParametersValidator _validator = new();

    [Fact]
    public void Validate_Succeeds_WhenNoFiltersSet()
    {
        var result = _validator.Validate(new MyWorkDashboardQueryParameters());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenToBeforeFrom()
    {
        var result = _validator.Validate(new MyWorkDashboardQueryParameters
        {
            From = new DateTimeOffset(2026, 6, 30, 0, 0, 0, TimeSpan.Zero),
            To = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero)
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_Succeeds_WhenToEqualsFrom()
    {
        var moment = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

        var result = _validator.Validate(new MyWorkDashboardQueryParameters { From = moment, To = moment });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenPeriodEndBeforePeriodStart()
    {
        var result = _validator.Validate(new MyWorkDashboardQueryParameters
        {
            PeriodStart = new DateOnly(2026, 6, 30),
            PeriodEnd = new DateOnly(2026, 6, 1)
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_Succeeds_WhenPeriodEndEqualsPeriodStart()
    {
        var day = new DateOnly(2026, 6, 1);

        var result = _validator.Validate(new MyWorkDashboardQueryParameters { PeriodStart = day, PeriodEnd = day });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Succeeds_WhenValidRangesProvided()
    {
        var result = _validator.Validate(new MyWorkDashboardQueryParameters
        {
            From = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
            To = new DateTimeOffset(2026, 6, 30, 0, 0, 0, TimeSpan.Zero),
            PeriodStart = new DateOnly(2026, 6, 1),
            PeriodEnd = new DateOnly(2026, 6, 30)
        });

        Assert.True(result.IsValid);
    }
}
