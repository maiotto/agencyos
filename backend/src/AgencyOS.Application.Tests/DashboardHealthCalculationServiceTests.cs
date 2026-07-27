using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class DashboardHealthCalculationServiceTests
{
    private readonly DashboardHealthCalculationService _service = new();

    [Fact]
    public void CalculateFromUtilization_ReturnsHealthy_ForNormalRange()
    {
        var indicator = _service.CalculateFromUtilization(60m, 55m);

        Assert.Equal(PortfolioHealth.Healthy, indicator.Status);
        Assert.Equal("Healthy", indicator.Label);
        Assert.Contains("60", indicator.Detail);
    }

    [Fact]
    public void CalculateFromUtilization_ReturnsOverloaded_WhenAboveHundred()
    {
        var indicator = _service.CalculateFromUtilization(120m, 0m);

        Assert.Equal(PortfolioHealth.Overloaded, indicator.Status);
        Assert.Equal("Overloaded", indicator.Label);
    }

    [Fact]
    public void CalculateFromUtilization_ReturnsAtRisk_AboveWarningThreshold()
    {
        var indicator = _service.CalculateFromUtilization(90m, 0m);

        Assert.Equal(PortfolioHealth.AtRisk, indicator.Status);
        Assert.Equal("At Risk", indicator.Label);
    }

    [Fact]
    public void CalculateFromUtilization_ReturnsUnderutilized_BelowFortyPercent()
    {
        var indicator = _service.CalculateFromUtilization(10m, 5m);

        Assert.Equal(PortfolioHealth.Underutilized, indicator.Status);
        Assert.Equal("Underutilized", indicator.Label);
    }

    [Fact]
    public void CalculateFromUtilization_RespectsCustomWarningThreshold()
    {
        var indicator = _service.CalculateFromUtilization(70m, 0m, 65m);

        Assert.Equal(PortfolioHealth.AtRisk, indicator.Status);
    }

    [Fact]
    public void CalculateOverall_ReturnsUnknown_WhenNoStatuses()
    {
        var indicator = _service.CalculateOverall([]);

        Assert.Equal(PortfolioHealth.Unknown, indicator.Status);
    }

    [Fact]
    public void CalculateOverall_ReturnsWorstStatus_AmongMultiple()
    {
        var indicator = _service.CalculateOverall(
            [PortfolioHealth.Healthy, PortfolioHealth.AtRisk, PortfolioHealth.Underutilized]);

        Assert.Equal(PortfolioHealth.AtRisk, indicator.Status);
    }

    [Fact]
    public void CalculateOverall_ReturnsOverloaded_WhenAnyOverloaded()
    {
        var indicator = _service.CalculateOverall(
            [PortfolioHealth.Healthy, PortfolioHealth.Overloaded]);

        Assert.Equal(PortfolioHealth.Overloaded, indicator.Status);
    }

    [Fact]
    public void CalculateOverall_ReturnsHealthy_WhenAllHealthy()
    {
        var indicator = _service.CalculateOverall(
            [PortfolioHealth.Healthy, PortfolioHealth.Healthy]);

        Assert.Equal(PortfolioHealth.Healthy, indicator.Status);
    }
}
