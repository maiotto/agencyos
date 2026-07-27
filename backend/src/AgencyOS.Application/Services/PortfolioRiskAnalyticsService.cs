using System.Text;
using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

/// <summary>
/// Flags at-risk Portfolios from already-built <see cref="PortfolioAnalyticsSnapshot"/>
/// projections (US-404 / BR-2210): Overloaded/AtRisk stored health, low average Recommendation
/// score, and a high proportion of cancelled Decisions. Pure calculation — no repository access.
/// </summary>
public class PortfolioRiskAnalyticsService : IPortfolioRiskAnalyticsService
{
    private const decimal LowRecommendationScoreThreshold = 50m;
    private const decimal HighCancellationRatioThreshold = 0.3m;

    public IReadOnlyList<PortfolioRiskIndicatorResponse> BuildRiskIndicators(
        IReadOnlyList<PortfolioAnalyticsSnapshot> snapshots)
    {
        var indicators = new List<PortfolioRiskIndicatorResponse>();

        foreach (var snapshot in snapshots)
        {
            var status = PortfolioHealth.Canonicalize(snapshot.PortfolioHealth);
            var reasons = new List<string>();

            var isOverloaded = status == PortfolioHealth.Overloaded;
            var isAtRisk = status == PortfolioHealth.AtRisk;

            if (isOverloaded)
            {
                reasons.Add("Portfolio health is Overloaded.");
            }
            else if (isAtRisk)
            {
                reasons.Add("Portfolio health is At Risk.");
            }

            var hasLowScore = snapshot.AverageRecommendationScore.HasValue
                && snapshot.AverageRecommendationScore.Value < LowRecommendationScoreThreshold;
            if (hasLowScore)
            {
                reasons.Add(
                    $"Average Recommendation score ({snapshot.AverageRecommendationScore!.Value:0.##}) is below the {LowRecommendationScoreThreshold:0.##} threshold.");
            }

            var cancellationRatio = snapshot.DecisionCount > 0
                ? (decimal)snapshot.CancelledDecisionCount / snapshot.DecisionCount
                : 0m;
            var hasHighCancellation = snapshot.DecisionCount > 0 && cancellationRatio >= HighCancellationRatioThreshold;
            if (hasHighCancellation)
            {
                reasons.Add(
                    $"{cancellationRatio:P0} of Decisions ({snapshot.CancelledDecisionCount}/{snapshot.DecisionCount}) were cancelled.");
            }

            if (reasons.Count == 0)
            {
                continue;
            }

            var riskLevel = isOverloaded
                ? "Critical"
                : isAtRisk
                    ? "High"
                    : "Medium";

            indicators.Add(new PortfolioRiskIndicatorResponse
            {
                PortfolioId = snapshot.PortfolioId,
                Name = snapshot.Name,
                RiskLevel = riskLevel,
                Reason = JoinReasons(reasons),
                Health = new HealthIndicator
                {
                    Status = status,
                    Label = status,
                    Detail = null
                },
                DrillDownPath = snapshot.DrillDownPath
            });
        }

        return indicators
            .OrderByDescending(indicator => RiskLevelWeight(indicator.RiskLevel))
            .ThenBy(indicator => indicator.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string JoinReasons(IReadOnlyList<string> reasons)
    {
        var builder = new StringBuilder();
        for (var index = 0; index < reasons.Count; index++)
        {
            if (index > 0)
            {
                builder.Append(' ');
            }

            builder.Append(reasons[index]);
        }

        return builder.ToString();
    }

    private static int RiskLevelWeight(string riskLevel) => riskLevel switch
    {
        "Critical" => 3,
        "High" => 2,
        "Medium" => 1,
        _ => 0
    };
}
