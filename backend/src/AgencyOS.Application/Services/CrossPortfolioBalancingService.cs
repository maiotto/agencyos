using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

/// <summary>
/// Produces advisory capacity/workload balancing recommendations across a set of Portfolio
/// participations (US-405 / BR-2306). Purely informational — never executes reallocation, AI
/// optimization, financial optimization, or hiring changes. Mission priorities supplied on each
/// <see cref="PortfolioParticipationResponse"/> are read-only inputs and are never reordered here.
/// </summary>
public class CrossPortfolioBalancingService : ICrossPortfolioBalancingService
{
    public IReadOnlyList<BalancingRecommendationResponse> BuildRecommendations(
        IReadOnlyList<PortfolioParticipationResponse> participations)
    {
        var recommendations = new List<BalancingRecommendationResponse>();

        var strained = participations
            .Where(participation => IsOverloadedOrAtRisk(participation.Health.Status))
            .OrderByDescending(participation => participation.UtilizationPercentage ?? participation.WorkloadPercentage ?? 0m)
            .ToList();

        var underutilized = participations
            .Where(participation => string.Equals(
                participation.Health.Status,
                PortfolioHealth.Underutilized,
                StringComparison.OrdinalIgnoreCase))
            .OrderBy(participation => participation.UtilizationPercentage ?? participation.WorkloadPercentage ?? 0m)
            .ToList();

        var usedTargets = new HashSet<Guid>();

        foreach (var source in strained)
        {
            var target = underutilized.FirstOrDefault(candidate =>
                candidate.PortfolioId != source.PortfolioId && !usedTargets.Contains(candidate.PortfolioId));

            if (target is not null)
            {
                usedTargets.Add(target.PortfolioId);
                recommendations.Add(new BalancingRecommendationResponse
                {
                    SourcePortfolioId = source.PortfolioId,
                    SourcePortfolioName = source.Name,
                    TargetPortfolioId = target.PortfolioId,
                    TargetPortfolioName = target.Name,
                    Category = "CapacityBalance",
                    Recommendation =
                        $"Consider reviewing Portfolio '{source.Name}' ({source.Health.Status}) against "
                        + $"'{target.Name}' (Underutilized) for potential rebalancing.",
                    Rationale =
                        $"'{source.Name}' utilization is {FormatPercentage(source.UtilizationPercentage)} vs "
                        + $"'{target.Name}' utilization at {FormatPercentage(target.UtilizationPercentage)}. "
                        + "Mission priorities are preserved as-is; no automatic reallocation is performed."
                });
            }
            else
            {
                recommendations.Add(new BalancingRecommendationResponse
                {
                    SourcePortfolioId = source.PortfolioId,
                    SourcePortfolioName = source.Name,
                    Category = "CapacityReview",
                    Recommendation =
                        $"Portfolio '{source.Name}' is {source.Health.Status} — review resourcing options with "
                        + "the Portfolio owner; no Underutilized Portfolio is available for comparison in this selection.",
                    Rationale = "No offsetting Underutilized Portfolio was found among the selected Portfolios."
                });
            }
        }

        if (recommendations.Count == 0)
        {
            recommendations.Add(new BalancingRecommendationResponse
            {
                Category = "Balanced",
                Recommendation = "Selected Portfolios show no significant capacity or workload imbalance requiring review.",
                Rationale = "No Portfolio in this selection is Overloaded, AtRisk, or Underutilized."
            });
        }

        return recommendations;
    }

    private static bool IsOverloadedOrAtRisk(string status) =>
        string.Equals(status, PortfolioHealth.Overloaded, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, PortfolioHealth.AtRisk, StringComparison.OrdinalIgnoreCase);

    private static string FormatPercentage(decimal? value) =>
        value.HasValue ? $"{value.Value:0.##}%" : "an unknown value";
}
