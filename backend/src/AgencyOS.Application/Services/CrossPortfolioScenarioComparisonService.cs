using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;

namespace AgencyOS.Application.Services;

/// <summary>
/// Compares two in-memory Cross-Portfolio Plan scenarios field-by-field (US-405). Read-only —
/// operates entirely on the already-computed <see cref="CrossPortfolioScenarioResponse"/> held by
/// each <see cref="CrossPortfolioScenarioRecord"/>.
/// </summary>
public class CrossPortfolioScenarioComparisonService : ICrossPortfolioScenarioComparisonService
{
    public ScenarioComparisonResponse Compare(
        CrossPortfolioScenarioRecord left,
        CrossPortfolioScenarioRecord right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var leftScenario = left.Scenario;
        var rightScenario = right.Scenario;

        var fieldDiffs = new List<ScenarioComparisonFieldDiff>
        {
            BuildPercentageDiff(
                "Average Utilization %",
                leftScenario.Capacity.AverageUtilizationPercentage,
                rightScenario.Capacity.AverageUtilizationPercentage),
            BuildPercentageDiff(
                "Average Workload %",
                leftScenario.Workload.AverageWorkloadPercentage,
                rightScenario.Workload.AverageWorkloadPercentage),
            BuildCountDiff(
                "Portfolio Conflict Count",
                leftScenario.Conflicts.PortfolioConflictCount,
                rightScenario.Conflicts.PortfolioConflictCount),
            BuildCountDiff(
                "Resource Conflict Count",
                leftScenario.Conflicts.ResourceConflictCount,
                rightScenario.Conflicts.ResourceConflictCount),
            BuildCountDiff(
                "Participation Count",
                leftScenario.Portfolios.Count,
                rightScenario.Portfolios.Count),
            BuildCountDiff(
                "Balancing Recommendation Count",
                leftScenario.Recommendations.Count,
                rightScenario.Recommendations.Count)
        };

        return new ScenarioComparisonResponse
        {
            CompanyId = leftScenario.CompanyId,
            GeneratedAt = DateTimeOffset.UtcNow,
            Left = leftScenario,
            Right = rightScenario,
            FieldDiffs = fieldDiffs,
            RequiresHumanApproval = true,
            AdvisoryDisclaimer = CrossPortfolioPlanningConstants.AdvisoryDisclaimer
        };
    }

    private static ScenarioComparisonFieldDiff BuildPercentageDiff(string field, decimal? left, decimal? right) =>
        new()
        {
            Field = field,
            LeftValue = left?.ToString("0.##"),
            RightValue = right?.ToString("0.##"),
            Delta = left.HasValue && right.HasValue
                ? decimal.Round(right.Value - left.Value, 2, MidpointRounding.AwayFromZero)
                : null
        };

    private static ScenarioComparisonFieldDiff BuildCountDiff(string field, int left, int right) =>
        new()
        {
            Field = field,
            LeftValue = left.ToString(),
            RightValue = right.ToString(),
            Delta = right - left
        };
}
