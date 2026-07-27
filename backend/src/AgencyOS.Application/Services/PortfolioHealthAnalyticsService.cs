using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

/// <summary>
/// Deterministic Portfolio health distribution and risk rollup (US-404 / BR-2203). Delegates every
/// health determination to <see cref="IDashboardHealthCalculationService"/>, which itself defers to
/// <see cref="PortfolioHealth"/> — no independent health thresholds are introduced.
/// </summary>
public class PortfolioHealthAnalyticsService : IPortfolioHealthAnalyticsService
{
    private readonly IDashboardHealthCalculationService _healthCalculationService;

    public PortfolioHealthAnalyticsService(IDashboardHealthCalculationService healthCalculationService)
    {
        _healthCalculationService = healthCalculationService;
    }

    public PortfolioHealthAnalyticsResponse BuildHealthAnalytics(
        Guid companyId,
        IReadOnlyList<PortfolioAnalyticsSnapshot> snapshots)
    {
        var statuses = snapshots.Select(snapshot => PortfolioHealth.Canonicalize(snapshot.PortfolioHealth)).ToList();

        var distribution = statuses
            .GroupBy(status => status, StringComparer.OrdinalIgnoreCase)
            .Select(group => new StatusCountItem { Status = group.Key, Count = group.Count() })
            .OrderBy(item => item.Status, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var riskIndicators = snapshots
            .Where(snapshot =>
            {
                var status = PortfolioHealth.Canonicalize(snapshot.PortfolioHealth);
                return status == PortfolioHealth.Overloaded || status == PortfolioHealth.AtRisk;
            })
            .Select(BuildRiskIndicator)
            .ToList();

        return new PortfolioHealthAnalyticsResponse
        {
            CompanyId = companyId,
            GeneratedAt = DateTimeOffset.UtcNow,
            PortfolioCount = snapshots.Count,
            Distribution = distribution,
            OverallHealth = _healthCalculationService.CalculateOverall(statuses),
            HealthyCount = statuses.Count(status => status == PortfolioHealth.Healthy),
            AtRiskCount = statuses.Count(status => status == PortfolioHealth.AtRisk),
            OverloadedCount = statuses.Count(status => status == PortfolioHealth.Overloaded),
            UnderutilizedCount = statuses.Count(status => status == PortfolioHealth.Underutilized),
            UnknownCount = statuses.Count(status => status == PortfolioHealth.Unknown),
            RiskIndicators = riskIndicators,
            DrillDownPath = $"/portfolios?companyId={companyId}"
        };
    }

    private PortfolioRiskIndicatorResponse BuildRiskIndicator(PortfolioAnalyticsSnapshot snapshot)
    {
        var status = PortfolioHealth.Canonicalize(snapshot.PortfolioHealth);
        var indicator = _healthCalculationService.CalculateOverall([status]);

        var reason = status == PortfolioHealth.Overloaded
            ? "Utilization or workload exceeds 100% of available capacity."
            : "Utilization or workload is approaching the configured warning threshold.";

        return new PortfolioRiskIndicatorResponse
        {
            PortfolioId = snapshot.PortfolioId,
            Name = snapshot.Name,
            RiskLevel = status,
            Reason = reason,
            Health = indicator,
            DrillDownPath = snapshot.DrillDownPath
        };
    }
}
