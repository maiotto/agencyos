using AgencyOS.Application.DTOs;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class PortfolioHealthAnalyticsServiceTests
{
    private static readonly Guid CompanyId = Guid.NewGuid();

    private readonly PortfolioHealthAnalyticsService _service = new(new DashboardHealthCalculationService());

    [Fact]
    public void BuildHealthAnalytics_ReturnsZeroedResponse_WhenNoSnapshots()
    {
        var result = _service.BuildHealthAnalytics(CompanyId, []);

        Assert.Equal(0, result.PortfolioCount);
        Assert.Empty(result.Distribution);
        Assert.Equal(PortfolioHealth.Unknown, result.OverallHealth.Status);
        Assert.Empty(result.RiskIndicators);
    }

    [Fact]
    public void BuildHealthAnalytics_CountsEachHealthStatus()
    {
        var snapshots = new[]
        {
            CreateSnapshot(PortfolioHealth.Healthy),
            CreateSnapshot(PortfolioHealth.Healthy),
            CreateSnapshot(PortfolioHealth.AtRisk),
            CreateSnapshot(PortfolioHealth.Overloaded),
            CreateSnapshot(PortfolioHealth.Underutilized),
            CreateSnapshot(PortfolioHealth.Unknown)
        };

        var result = _service.BuildHealthAnalytics(CompanyId, snapshots);

        Assert.Equal(6, result.PortfolioCount);
        Assert.Equal(2, result.HealthyCount);
        Assert.Equal(1, result.AtRiskCount);
        Assert.Equal(1, result.OverloadedCount);
        Assert.Equal(1, result.UnderutilizedCount);
        Assert.Equal(1, result.UnknownCount);
        Assert.Equal(PortfolioHealth.Overloaded, result.OverallHealth.Status);
    }

    [Fact]
    public void BuildHealthAnalytics_OnlyFlagsOverloadedAndAtRisk_AsRiskIndicators()
    {
        var healthy = CreateSnapshot(PortfolioHealth.Healthy);
        var atRisk = CreateSnapshot(PortfolioHealth.AtRisk);
        var overloaded = CreateSnapshot(PortfolioHealth.Overloaded);
        var underutilized = CreateSnapshot(PortfolioHealth.Underutilized);

        var result = _service.BuildHealthAnalytics(CompanyId, [healthy, atRisk, overloaded, underutilized]);

        Assert.Equal(2, result.RiskIndicators.Count);
        Assert.Contains(result.RiskIndicators, indicator => indicator.PortfolioId == atRisk.PortfolioId);
        Assert.Contains(result.RiskIndicators, indicator => indicator.PortfolioId == overloaded.PortfolioId);
    }

    [Fact]
    public void BuildHealthAnalytics_SetsDrillDownPath_ScopedToCompany()
    {
        var result = _service.BuildHealthAnalytics(CompanyId, []);

        Assert.Equal($"/portfolios?companyId={CompanyId}", result.DrillDownPath);
    }

    private static PortfolioAnalyticsSnapshot CreateSnapshot(string health) => new()
    {
        PortfolioId = Guid.NewGuid(),
        Name = $"Portfolio {Guid.NewGuid():N}",
        Status = "Active",
        PortfolioHealth = health,
        MissionCount = 1,
        MissionIds = [Guid.NewGuid()],
        DrillDownPath = "/portfolios/x"
    };
}
