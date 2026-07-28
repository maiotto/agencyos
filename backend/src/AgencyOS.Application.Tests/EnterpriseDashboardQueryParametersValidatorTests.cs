using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;

namespace AgencyOS.Application.Tests;

public class EnterpriseDashboardQueryParametersValidatorTests
{
    private readonly EnterpriseDashboardQueryParametersValidator _validator = new();

    [Fact]
    public void Validate_AcceptsEmptyParameters()
    {
        var result = _validator.Validate(new EnterpriseDashboardQueryParameters());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_AcceptsFromEqualToTo()
    {
        var now = DateTimeOffset.UtcNow;
        var result = _validator.Validate(new EnterpriseDashboardQueryParameters { From = now, To = now });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsToEarlierThanFrom()
    {
        var now = DateTimeOffset.UtcNow;
        var result = _validator.Validate(
            new EnterpriseDashboardQueryParameters { From = now, To = now.AddDays(-1) });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_AcceptsPeriodStartEqualToPeriodEnd()
    {
        var date = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var result = _validator.Validate(
            new EnterpriseDashboardQueryParameters { PeriodStart = date, PeriodEnd = date });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsPeriodEndEarlierThanPeriodStart()
    {
        var date = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var result = _validator.Validate(
            new EnterpriseDashboardQueryParameters { PeriodStart = date, PeriodEnd = date.AddDays(-1) });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_AcceptsOnlyFromSet()
    {
        var result = _validator.Validate(
            new EnterpriseDashboardQueryParameters { From = DateTimeOffset.UtcNow });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_AcceptsOnlyPeriodStartSet()
    {
        var result = _validator.Validate(
            new EnterpriseDashboardQueryParameters { PeriodStart = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime) });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_AcceptsValidRangeForBothWindows()
    {
        var now = DateTimeOffset.UtcNow;
        var date = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var result = _validator.Validate(new EnterpriseDashboardQueryParameters
        {
            From = now.AddDays(-30),
            To = now,
            PeriodStart = date.AddDays(-30),
            PeriodEnd = date
        });

        Assert.True(result.IsValid);
    }
}
