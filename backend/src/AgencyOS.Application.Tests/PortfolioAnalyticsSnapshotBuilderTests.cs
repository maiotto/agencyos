using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class PortfolioAnalyticsSnapshotBuilderTests
{
    private static readonly Guid CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

    [Fact]
    public void Build_FiltersRecommendationsAndDecisions_ToPortfolioMissionsOnly()
    {
        var missionId = Guid.NewGuid();
        var otherMissionId = Guid.NewGuid();
        var portfolio = CreatePortfolio(missionId, utilization: 70m, workload: 65m);

        var ownRecommendation = CreateRecommendation(missionId, score: 80m);
        var otherRecommendation = CreateRecommendation(otherMissionId, score: 20m);

        var ownDecision = Decision.Create(ownRecommendation.Id, CompanyId, missionId, Guid.NewGuid(), "planner", DateTimeOffset.UtcNow);
        var otherDecision = Decision.Create(otherRecommendation.Id, CompanyId, otherMissionId, Guid.NewGuid(), "planner", DateTimeOffset.UtcNow);

        var snapshot = PortfolioAnalyticsSnapshotBuilder.Build(
            portfolio,
            [ownRecommendation, otherRecommendation],
            [ownDecision, otherDecision]);

        Assert.Equal(portfolio.Id, snapshot.PortfolioId);
        Assert.Equal(1, snapshot.RecommendationCount);
        Assert.Equal(1, snapshot.DecisionCount);
        Assert.Equal(80m, snapshot.AverageRecommendationScore);
        Assert.Equal(70m, snapshot.UtilizationPercentage);
        Assert.Equal(65m, snapshot.WorkloadPercentage);
        Assert.Equal($"/portfolios/{portfolio.Id}", snapshot.DrillDownPath);
    }

    [Fact]
    public void Build_CountsCompletedAndCancelledDecisions()
    {
        var missionId = Guid.NewGuid();
        var portfolio = CreatePortfolio(missionId);
        var recommendation = CreateRecommendation(missionId);

        var completed = Decision.Create(recommendation.Id, CompanyId, missionId, Guid.NewGuid(), "planner", DateTimeOffset.UtcNow);
        completed.StartImplementation("planner", null, DateTimeOffset.UtcNow);
        completed.Complete("planner", null, DateTimeOffset.UtcNow);

        var cancelled = Decision.Create(recommendation.Id, CompanyId, missionId, Guid.NewGuid(), "planner", DateTimeOffset.UtcNow);
        cancelled.Cancel("planner", null, DateTimeOffset.UtcNow);

        var pending = Decision.Create(recommendation.Id, CompanyId, missionId, Guid.NewGuid(), "planner", DateTimeOffset.UtcNow);

        var snapshot = PortfolioAnalyticsSnapshotBuilder.Build(
            portfolio,
            [recommendation],
            [completed, cancelled, pending]);

        Assert.Equal(3, snapshot.DecisionCount);
        Assert.Equal(1, snapshot.CompletedDecisionCount);
        Assert.Equal(1, snapshot.CancelledDecisionCount);
    }

    [Fact]
    public void Build_ReturnsNullUtilizationAndWorkload_WhenSnapshotJsonMissing()
    {
        var missionId = Guid.NewGuid();
        var portfolio = Portfolio.Create(
            CompanyId,
            $"Portfolio {Guid.NewGuid():N}",
            null,
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime),
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddDays(30).UtcDateTime),
            null,
            [(missionId, 1)],
            DateTimeOffset.UtcNow);

        var snapshot = PortfolioAnalyticsSnapshotBuilder.Build(portfolio, [], []);

        Assert.Null(snapshot.UtilizationPercentage);
        Assert.Null(snapshot.WorkloadPercentage);
        Assert.Equal(0, snapshot.RecommendationCount);
        Assert.Null(snapshot.AverageRecommendationScore);
    }

    [Fact]
    public void TryReadDecimal_ReturnsNull_ForMalformedJson()
    {
        var value = PortfolioAnalyticsSnapshotBuilder.TryReadDecimal("not-json", "overallUtilizationPercentage");

        Assert.Null(value);
    }

    [Fact]
    public void TryReadDecimal_ReturnsNull_WhenPropertyMissing()
    {
        var value = PortfolioAnalyticsSnapshotBuilder.TryReadDecimal("{\"other\":1}", "overallUtilizationPercentage");

        Assert.Null(value);
    }

    [Fact]
    public void ToCard_MapsSnapshotFieldsAndHealth()
    {
        var missionId = Guid.NewGuid();
        var portfolio = CreatePortfolio(missionId, utilization: 55m, workload: 60m);
        var snapshot = PortfolioAnalyticsSnapshotBuilder.Build(portfolio, [], []);

        var health = new AgencyOS.Application.DTOs.HealthIndicator { Status = PortfolioHealth.Healthy, Label = "Healthy" };
        var card = PortfolioAnalyticsSnapshotBuilder.ToCard(snapshot, health);

        Assert.Equal(snapshot.PortfolioId, card.PortfolioId);
        Assert.Equal(snapshot.Name, card.Name);
        Assert.Equal(health, card.Health);
        Assert.Equal(55m, card.UtilizationPercentage);
        Assert.Equal(60m, card.WorkloadPercentage);
        Assert.Equal(snapshot.DrillDownPath, card.DrillDownPath);
    }

    private static Portfolio CreatePortfolio(Guid missionId, decimal utilization = 50m, decimal workload = 50m)
    {
        var portfolio = Portfolio.Create(
            CompanyId,
            $"Portfolio {Guid.NewGuid():N}",
            null,
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime),
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddDays(30).UtcDateTime),
            null,
            [(missionId, 1)],
            DateTimeOffset.UtcNow);

        portfolio.CalculateCapacity(
            $"{{\"overallUtilizationPercentage\":{utilization.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}",
            DateTimeOffset.UtcNow);
        portfolio.CalculateWorkload(
            $"{{\"overallWorkloadPercentage\":{workload.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}",
            DateTimeOffset.UtcNow);
        portfolio.CalculateHealth(utilization, workload, null, "{}", DateTimeOffset.UtcNow);

        return portfolio;
    }

    private static Recommendation CreateRecommendation(Guid missionId, decimal? score = 75m) =>
        Recommendation.Create(
            CompanyId,
            missionId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            $"REC-{Guid.NewGuid():N}",
            "Portfolio Analytics Recommendation",
            "Summary",
            "Reason",
            score,
            1,
            1,
            RecommendationVersions.CurrentDecisionEngineVersion,
            null,
            "{}",
            "{}",
            "{\"ok\":true}",
            "decision-engine",
            DateTimeOffset.UtcNow);
}
