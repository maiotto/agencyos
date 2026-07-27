using AgencyOS.Application.DTOs;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class CrossPortfolioBalancingServiceTests
{
    private readonly CrossPortfolioBalancingService _service = new();

    [Fact]
    public void BuildRecommendations_PairsOverloadedWithUnderutilized()
    {
        var overloaded = CreateParticipation("Overloaded Portfolio", PortfolioHealth.Overloaded, utilization: 130m);
        var underutilized = CreateParticipation("Underutilized Portfolio", PortfolioHealth.Underutilized, utilization: 20m);

        var recommendations = _service.BuildRecommendations([overloaded, underutilized]);

        var recommendation = Assert.Single(recommendations);
        Assert.Equal(overloaded.PortfolioId, recommendation.SourcePortfolioId);
        Assert.Equal(underutilized.PortfolioId, recommendation.TargetPortfolioId);
        Assert.Equal("CapacityBalance", recommendation.Category);
        Assert.Contains(overloaded.Name, recommendation.Recommendation);
        Assert.Contains(underutilized.Name, recommendation.Recommendation);
    }

    [Fact]
    public void BuildRecommendations_ReturnsBalancedMessage_WhenNoImbalance()
    {
        var healthyOne = CreateParticipation("Portfolio A", PortfolioHealth.Healthy, utilization: 60m);
        var healthyTwo = CreateParticipation("Portfolio B", PortfolioHealth.Healthy, utilization: 65m);

        var recommendations = _service.BuildRecommendations([healthyOne, healthyTwo]);

        var recommendation = Assert.Single(recommendations);
        Assert.Equal("Balanced", recommendation.Category);
        Assert.Null(recommendation.SourcePortfolioId);
        Assert.Null(recommendation.TargetPortfolioId);
    }

    [Fact]
    public void BuildRecommendations_ReturnsCapacityReview_WhenNoUnderutilizedCounterpart()
    {
        var overloaded = CreateParticipation("Overloaded Portfolio", PortfolioHealth.Overloaded, utilization: 140m);
        var atRisk = CreateParticipation("At Risk Portfolio", PortfolioHealth.AtRisk, utilization: 90m);

        var recommendations = _service.BuildRecommendations([overloaded, atRisk]);

        Assert.Equal(2, recommendations.Count);
        Assert.All(recommendations, recommendation => Assert.Equal("CapacityReview", recommendation.Category));
        Assert.All(recommendations, recommendation => Assert.Null(recommendation.TargetPortfolioId));
    }

    [Fact]
    public void BuildRecommendations_DoesNotReuseSameUnderutilizedTargetTwice()
    {
        var overloadedOne = CreateParticipation("Overloaded One", PortfolioHealth.Overloaded, utilization: 150m);
        var overloadedTwo = CreateParticipation("Overloaded Two", PortfolioHealth.Overloaded, utilization: 120m);
        var underutilized = CreateParticipation("Underutilized", PortfolioHealth.Underutilized, utilization: 10m);

        var recommendations = _service.BuildRecommendations([overloadedOne, overloadedTwo, underutilized]);

        Assert.Equal(2, recommendations.Count);
        var targets = recommendations.Select(r => r.TargetPortfolioId).ToList();
        Assert.Single(targets.Where(t => t == underutilized.PortfolioId));
    }

    [Fact]
    public void BuildRecommendations_ReturnsEmptyInputSafely()
    {
        var recommendations = _service.BuildRecommendations([]);

        var recommendation = Assert.Single(recommendations);
        Assert.Equal("Balanced", recommendation.Category);
    }

    private static PortfolioParticipationResponse CreateParticipation(
        string name,
        string health,
        decimal utilization) =>
        new()
        {
            PortfolioId = Guid.NewGuid(),
            Name = name,
            Status = PortfolioStatus.Active,
            Health = new HealthIndicator { Status = health, Label = health },
            MissionCount = 1,
            Missions = [new PortfolioParticipationMissionItem { MissionId = Guid.NewGuid(), Priority = 1 }],
            UtilizationPercentage = utilization,
            WorkloadPercentage = utilization,
            DrillDownPath = "/portfolios"
        };
}
