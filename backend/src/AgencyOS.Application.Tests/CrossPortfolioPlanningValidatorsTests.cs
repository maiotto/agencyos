using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;

namespace AgencyOS.Application.Tests;

public class CrossPortfolioPlanningQueryParametersValidatorTests
{
    private readonly CrossPortfolioPlanningQueryParametersValidator _validator = new();

    [Fact]
    public void Validate_AcceptsEmptyParameters()
    {
        var result = _validator.Validate(new CrossPortfolioPlanningQueryParameters());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsToEarlierThanFrom()
    {
        var now = DateTimeOffset.UtcNow;
        var result = _validator.Validate(new CrossPortfolioPlanningQueryParameters { From = now, To = now.AddDays(-1) });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsPeriodEndEarlierThanPeriodStart()
    {
        var date = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var result = _validator.Validate(
            new CrossPortfolioPlanningQueryParameters { PeriodStart = date, PeriodEnd = date.AddDays(-1) });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_AcceptsPeriodStartEqualToPeriodEnd()
    {
        var date = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var result = _validator.Validate(
            new CrossPortfolioPlanningQueryParameters { PeriodStart = date, PeriodEnd = date });

        Assert.True(result.IsValid);
    }
}

public class CrossPortfolioSelectionQueryParametersValidatorTests
{
    private readonly CrossPortfolioSelectionQueryParametersValidator _validator = new();

    [Fact]
    public void Validate_RejectsEmptyPortfolioIds()
    {
        var result = _validator.Validate(new CrossPortfolioSelectionQueryParameters());

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_AcceptsAtLeastOnePortfolioId()
    {
        var result = _validator.Validate(new CrossPortfolioSelectionQueryParameters
        {
            PortfolioIds = [Guid.NewGuid()]
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsPeriodEndEarlierThanPeriodStart()
    {
        var date = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var result = _validator.Validate(new CrossPortfolioSelectionQueryParameters
        {
            PortfolioIds = [Guid.NewGuid()],
            PeriodStart = date,
            PeriodEnd = date.AddDays(-1)
        });

        Assert.False(result.IsValid);
    }
}

public class SimulateCrossPortfolioPlanRequestValidatorTests
{
    private readonly SimulateCrossPortfolioPlanRequestValidator _validator = new();

    [Fact]
    public void Validate_RejectsFewerThanTwoPortfolioIds()
    {
        var result = _validator.Validate(new SimulateCrossPortfolioPlanRequest
        {
            PortfolioIds = [Guid.NewGuid()]
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsDuplicatePortfolioIds_CountingAsOne()
    {
        var portfolioId = Guid.NewGuid();
        var result = _validator.Validate(new SimulateCrossPortfolioPlanRequest
        {
            PortfolioIds = [portfolioId, portfolioId]
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_AcceptsTwoDistinctPortfolioIds()
    {
        var result = _validator.Validate(new SimulateCrossPortfolioPlanRequest
        {
            PortfolioIds = [Guid.NewGuid(), Guid.NewGuid()]
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsPeriodEndEarlierThanPeriodStart()
    {
        var date = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var result = _validator.Validate(new SimulateCrossPortfolioPlanRequest
        {
            PortfolioIds = [Guid.NewGuid(), Guid.NewGuid()],
            PeriodStart = date,
            PeriodEnd = date.AddDays(-1)
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsScenarioNameLongerThan200Characters()
    {
        var result = _validator.Validate(new SimulateCrossPortfolioPlanRequest
        {
            PortfolioIds = [Guid.NewGuid(), Guid.NewGuid()],
            ScenarioName = new string('a', 201)
        });

        Assert.False(result.IsValid);
    }
}

public class CompareCrossPortfolioScenariosRequestValidatorTests
{
    private readonly CompareCrossPortfolioScenariosRequestValidator _validator = new();

    [Fact]
    public void Validate_AcceptsDistinctScenarioIds()
    {
        var result = _validator.Validate(new CompareCrossPortfolioScenariosRequest
        {
            LeftScenarioId = Guid.NewGuid(),
            RightScenarioId = Guid.NewGuid()
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsEmptyLeftScenarioId()
    {
        var result = _validator.Validate(new CompareCrossPortfolioScenariosRequest
        {
            LeftScenarioId = Guid.Empty,
            RightScenarioId = Guid.NewGuid()
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsEmptyRightScenarioId()
    {
        var result = _validator.Validate(new CompareCrossPortfolioScenariosRequest
        {
            LeftScenarioId = Guid.NewGuid(),
            RightScenarioId = Guid.Empty
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsIdenticalScenarioIds()
    {
        var scenarioId = Guid.NewGuid();
        var result = _validator.Validate(new CompareCrossPortfolioScenariosRequest
        {
            LeftScenarioId = scenarioId,
            RightScenarioId = scenarioId
        });

        Assert.False(result.IsValid);
    }
}
