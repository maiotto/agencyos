using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;

namespace AgencyOS.Application.Tests;

public class PortfolioAnalyticsQueryParametersValidatorTests
{
    private readonly PortfolioAnalyticsQueryParametersValidator _validator = new();

    [Fact]
    public void Validate_AcceptsEmptyParameters()
    {
        var result = _validator.Validate(new PortfolioAnalyticsQueryParameters());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsToEarlierThanFrom()
    {
        var now = DateTimeOffset.UtcNow;
        var result = _validator.Validate(new PortfolioAnalyticsQueryParameters { From = now, To = now.AddDays(-1) });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_AcceptsFromEqualToTo()
    {
        var now = DateTimeOffset.UtcNow;
        var result = _validator.Validate(new PortfolioAnalyticsQueryParameters { From = now, To = now });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsPeriodEndEarlierThanPeriodStart()
    {
        var date = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var result = _validator.Validate(
            new PortfolioAnalyticsQueryParameters { PeriodStart = date, PeriodEnd = date.AddDays(-1) });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_AcceptsPeriodStartEqualToPeriodEnd()
    {
        var date = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var result = _validator.Validate(
            new PortfolioAnalyticsQueryParameters { PeriodStart = date, PeriodEnd = date });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_AcceptsPortfolioIdSet()
    {
        var result = _validator.Validate(new PortfolioAnalyticsQueryParameters { PortfolioId = Guid.NewGuid() });

        Assert.True(result.IsValid);
    }
}

public class PortfolioCompareQueryParametersValidatorTests
{
    private readonly PortfolioCompareQueryParametersValidator _validator = new();

    [Fact]
    public void Validate_AcceptsDistinctPortfolioIds()
    {
        var result = _validator.Validate(new PortfolioCompareQueryParameters
        {
            LeftPortfolioId = Guid.NewGuid(),
            RightPortfolioId = Guid.NewGuid()
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsEmptyLeftPortfolioId()
    {
        var result = _validator.Validate(new PortfolioCompareQueryParameters
        {
            LeftPortfolioId = Guid.Empty,
            RightPortfolioId = Guid.NewGuid()
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsEmptyRightPortfolioId()
    {
        var result = _validator.Validate(new PortfolioCompareQueryParameters
        {
            LeftPortfolioId = Guid.NewGuid(),
            RightPortfolioId = Guid.Empty
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsIdenticalPortfolioIds()
    {
        var portfolioId = Guid.NewGuid();
        var result = _validator.Validate(new PortfolioCompareQueryParameters
        {
            LeftPortfolioId = portfolioId,
            RightPortfolioId = portfolioId
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsToEarlierThanFrom()
    {
        var now = DateTimeOffset.UtcNow;
        var result = _validator.Validate(new PortfolioCompareQueryParameters
        {
            LeftPortfolioId = Guid.NewGuid(),
            RightPortfolioId = Guid.NewGuid(),
            From = now,
            To = now.AddDays(-1)
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsPeriodEndEarlierThanPeriodStart()
    {
        var date = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var result = _validator.Validate(new PortfolioCompareQueryParameters
        {
            LeftPortfolioId = Guid.NewGuid(),
            RightPortfolioId = Guid.NewGuid(),
            PeriodStart = date,
            PeriodEnd = date.AddDays(-1)
        });

        Assert.False(result.IsValid);
    }
}
