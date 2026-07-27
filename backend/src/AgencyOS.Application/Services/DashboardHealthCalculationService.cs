using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

/// <summary>
/// Deterministic dashboard health calculation (US-403 / BR-2107). Delegates the actual
/// threshold logic to <see cref="PortfolioHealth"/> — no independent health rules are introduced.
/// </summary>
public class DashboardHealthCalculationService : IDashboardHealthCalculationService
{
    /// <summary>Worst-case ordering used when rolling up multiple Portfolio health statuses.</summary>
    private static readonly IReadOnlyList<string> SeverityOrder =
    [
        PortfolioHealth.Overloaded,
        PortfolioHealth.AtRisk,
        PortfolioHealth.Underutilized,
        PortfolioHealth.Healthy,
        PortfolioHealth.Unknown
    ];

    public HealthIndicator CalculateFromUtilization(
        decimal utilizationPercentage,
        decimal workloadPercentage,
        decimal? warningPercentage = null)
    {
        var status = PortfolioHealth.Calculate(utilizationPercentage, workloadPercentage, warningPercentage);
        return BuildIndicator(status, utilizationPercentage, workloadPercentage);
    }

    public HealthIndicator CalculateOverall(IReadOnlyCollection<string> healthStatuses)
    {
        if (healthStatuses is null || healthStatuses.Count == 0)
        {
            return BuildIndicator(PortfolioHealth.Unknown, null, null);
        }

        var canonicalStatuses = healthStatuses
            .Select(PortfolioHealth.Canonicalize)
            .ToHashSet(StringComparer.Ordinal);

        var worst = SeverityOrder.First(canonicalStatuses.Contains);
        return BuildIndicator(worst, null, null);
    }

    private static HealthIndicator BuildIndicator(
        string status,
        decimal? utilizationPercentage,
        decimal? workloadPercentage)
    {
        var (label, detail) = status switch
        {
            PortfolioHealth.Overloaded => (
                "Overloaded",
                "Utilization or workload exceeds 100% of available capacity."),
            PortfolioHealth.AtRisk => (
                "At Risk",
                "Utilization or workload is approaching the configured warning threshold."),
            PortfolioHealth.Underutilized => (
                "Underutilized",
                "Utilization and workload are below 40% of available capacity."),
            PortfolioHealth.Healthy => (
                "Healthy",
                "Utilization and workload are within expected operating ranges."),
            _ => (
                "Unknown",
                "Not enough data is available to determine health for the selected period.")
        };

        var detailWithMetrics = utilizationPercentage.HasValue || workloadPercentage.HasValue
            ? $"{detail} (Utilization: {utilizationPercentage?.ToString("0.##") ?? "n/a"}%, " +
              $"Workload: {workloadPercentage?.ToString("0.##") ?? "n/a"}%)"
            : detail;

        return new HealthIndicator
        {
            Status = status,
            Label = label,
            Detail = detailWithMetrics
        };
    }
}
