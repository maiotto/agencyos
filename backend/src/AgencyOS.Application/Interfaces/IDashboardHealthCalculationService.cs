namespace AgencyOS.Application.Interfaces;

using AgencyOS.Application.DTOs;

/// <summary>
/// Computes deterministic dashboard health indicators (US-403 / BR-2107) by reusing
/// <c>PortfolioHealth</c>'s utilization/workload threshold rules. Never introduces new health logic.
/// </summary>
public interface IDashboardHealthCalculationService
{
    /// <summary>Derives a health indicator from utilization/workload percentages (BR-2107).</summary>
    HealthIndicator CalculateFromUtilization(
        decimal utilizationPercentage,
        decimal workloadPercentage,
        decimal? warningPercentage = null);

    /// <summary>Rolls a set of Portfolio health statuses up to a single worst-case indicator.</summary>
    HealthIndicator CalculateOverall(IReadOnlyCollection<string> healthStatuses);
}
