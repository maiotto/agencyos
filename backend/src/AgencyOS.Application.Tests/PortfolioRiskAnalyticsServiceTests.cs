using AgencyOS.Application.DTOs;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class PortfolioRiskAnalyticsServiceTests
{
    private readonly PortfolioRiskAnalyticsService _service = new();

    [Fact]
    public void BuildRiskIndicators_ReturnsEmpty_WhenNoRiskFactorsPresent()
    {
        var snapshot = CreateSnapshot(PortfolioHealth.Healthy, averageScore: 90m, decisionCount: 4, cancelledCount: 0);

        var indicators = _service.BuildRiskIndicators([snapshot]);

        Assert.Empty(indicators);
    }

    [Fact]
    public void BuildRiskIndicators_FlagsOverloadedHealth_AsCritical()
    {
        var snapshot = CreateSnapshot(PortfolioHealth.Overloaded, averageScore: 90m, decisionCount: 0, cancelledCount: 0);

        var indicator = Assert.Single(_service.BuildRiskIndicators([snapshot]));

        Assert.Equal("Critical", indicator.RiskLevel);
        Assert.Contains("Overloaded", indicator.Reason);
    }

    [Fact]
    public void BuildRiskIndicators_FlagsAtRiskHealth_AsHigh()
    {
        var snapshot = CreateSnapshot(PortfolioHealth.AtRisk, averageScore: 90m, decisionCount: 0, cancelledCount: 0);

        var indicator = Assert.Single(_service.BuildRiskIndicators([snapshot]));

        Assert.Equal("High", indicator.RiskLevel);
    }

    [Fact]
    public void BuildRiskIndicators_FlagsLowAverageRecommendationScore_AsMedium()
    {
        var snapshot = CreateSnapshot(PortfolioHealth.Healthy, averageScore: 30m, decisionCount: 0, cancelledCount: 0);

        var indicator = Assert.Single(_service.BuildRiskIndicators([snapshot]));

        Assert.Equal("Medium", indicator.RiskLevel);
        Assert.Contains("Average Recommendation score", indicator.Reason);
    }

    [Fact]
    public void BuildRiskIndicators_FlagsHighCancellationRatio_AsMedium()
    {
        var snapshot = CreateSnapshot(PortfolioHealth.Healthy, averageScore: 90m, decisionCount: 10, cancelledCount: 4);

        var indicator = Assert.Single(_service.BuildRiskIndicators([snapshot]));

        Assert.Equal("Medium", indicator.RiskLevel);
        Assert.Contains("cancelled", indicator.Reason);
    }

    [Fact]
    public void BuildRiskIndicators_DoesNotFlagLowCancellationRatio()
    {
        var snapshot = CreateSnapshot(PortfolioHealth.Healthy, averageScore: 90m, decisionCount: 10, cancelledCount: 1);

        var indicators = _service.BuildRiskIndicators([snapshot]);

        Assert.Empty(indicators);
    }

    [Fact]
    public void BuildRiskIndicators_OrdersByRiskLevelSeverity()
    {
        var medium = CreateSnapshot(PortfolioHealth.Healthy, averageScore: 30m, decisionCount: 0, cancelledCount: 0);
        var critical = CreateSnapshot(PortfolioHealth.Overloaded, averageScore: 90m, decisionCount: 0, cancelledCount: 0);
        var high = CreateSnapshot(PortfolioHealth.AtRisk, averageScore: 90m, decisionCount: 0, cancelledCount: 0);

        var indicators = _service.BuildRiskIndicators([medium, critical, high]);

        Assert.Equal(3, indicators.Count);
        Assert.Equal("Critical", indicators[0].RiskLevel);
        Assert.Equal("High", indicators[1].RiskLevel);
        Assert.Equal("Medium", indicators[2].RiskLevel);
    }

    private static PortfolioAnalyticsSnapshot CreateSnapshot(
        string health,
        decimal? averageScore,
        int decisionCount,
        int cancelledCount) => new()
    {
        PortfolioId = Guid.NewGuid(),
        Name = $"Portfolio {Guid.NewGuid():N}",
        Status = "Active",
        PortfolioHealth = health,
        MissionCount = 1,
        MissionIds = [Guid.NewGuid()],
        AverageRecommendationScore = averageScore,
        DecisionCount = decisionCount,
        CancelledDecisionCount = cancelledCount,
        DrillDownPath = "/portfolios/x"
    };
}
