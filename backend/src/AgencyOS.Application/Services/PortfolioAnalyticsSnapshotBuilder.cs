using System.Text.Json;
using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

/// <summary>
/// Shared, read-only projection logic for building a <see cref="PortfolioAnalyticsSnapshot"/> from
/// a Portfolio and its mission-scoped Recommendations/Decisions (US-404 / BR-2201..BR-2210).
/// Never mutates the Portfolio and never re-derives Capacity/Workload — utilization/workload are
/// parsed from the already-persisted <c>CapacitySummary</c>/<c>WorkloadSummary</c> JSON snapshots
/// (BR-2204), mirroring <c>DashboardAggregationService</c>'s tolerant JSON reads.
/// </summary>
public static class PortfolioAnalyticsSnapshotBuilder
{
    /// <summary>Filters a company-wide Recommendation/Decision set down to a single Portfolio's Missions.</summary>
    public static PortfolioAnalyticsSnapshot Build(
        Portfolio portfolio,
        IReadOnlyCollection<Recommendation> companyRecommendations,
        IReadOnlyCollection<Decision> companyDecisions)
    {
        var missionIds = portfolio.Missions.Select(mission => mission.MissionId).ToHashSet();
        var portfolioRecommendations = companyRecommendations
            .Where(recommendation => missionIds.Contains(recommendation.MissionId))
            .ToList();
        var portfolioDecisions = companyDecisions
            .Where(decision => missionIds.Contains(decision.MissionId))
            .ToList();

        return BuildFrom(portfolio, missionIds, portfolioRecommendations, portfolioDecisions);
    }

    /// <summary>Builds from Recommendations/Decisions already scoped to this Portfolio's Missions.</summary>
    public static PortfolioAnalyticsSnapshot BuildFrom(
        Portfolio portfolio,
        IReadOnlyCollection<Guid> missionIds,
        IReadOnlyCollection<Recommendation> portfolioRecommendations,
        IReadOnlyCollection<Decision> portfolioDecisions)
    {
        var scored = portfolioRecommendations.Where(recommendation => recommendation.Score.HasValue).ToList();

        return new PortfolioAnalyticsSnapshot
        {
            PortfolioId = portfolio.Id,
            Name = portfolio.Name,
            Status = portfolio.Status,
            PortfolioHealth = portfolio.PortfolioHealth,
            MissionCount = portfolio.Missions.Count,
            MissionIds = missionIds.ToList(),
            UtilizationPercentage = TryReadDecimal(portfolio.CapacitySummary, "overallUtilizationPercentage"),
            WorkloadPercentage = TryReadDecimal(portfolio.WorkloadSummary, "overallWorkloadPercentage"),
            RecommendationCount = portfolioRecommendations.Count,
            AverageRecommendationScore = scored.Count > 0
                ? decimal.Round(scored.Average(recommendation => recommendation.Score!.Value), 2, MidpointRounding.AwayFromZero)
                : null,
            DecisionCount = portfolioDecisions.Count,
            CompletedDecisionCount = portfolioDecisions.Count(decision => DecisionStatus.IsCompleted(decision.DecisionStatus)),
            CancelledDecisionCount = portfolioDecisions.Count(decision => DecisionStatus.IsCancelled(decision.DecisionStatus)),
            DrillDownPath = $"/portfolios/{portfolio.Id}"
        };
    }

    public static PortfolioAnalyticsCardResponse ToCard(PortfolioAnalyticsSnapshot snapshot, HealthIndicator health) =>
        new()
        {
            PortfolioId = snapshot.PortfolioId,
            Name = snapshot.Name,
            Status = snapshot.Status,
            Health = health,
            MissionCount = snapshot.MissionCount,
            UtilizationPercentage = snapshot.UtilizationPercentage,
            WorkloadPercentage = snapshot.WorkloadPercentage,
            RecommendationCount = snapshot.RecommendationCount,
            DecisionCount = snapshot.DecisionCount,
            DrillDownPath = snapshot.DrillDownPath
        };

    public static decimal? TryReadDecimal(string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty(propertyName, out var property)
                && property.TryGetDecimal(out var value))
            {
                return value;
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }
}
