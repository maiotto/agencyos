using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;

namespace AgencyOS.Application.Services;

/// <summary>
/// Read-only Executive Workspace overview aggregation (US-505 / BR-2801..BR-2810). Combines the
/// existing Enterprise Dashboard Summary, Portfolio, Capacity, Workload, and Audit sections into
/// Executive KPIs and an overall health rollup reused from
/// <see cref="IDashboardHealthCalculationService"/> (BR-2805) — never introduces new health logic
/// and never mutates data (DEC-505-001).
/// </summary>
public class ExecutiveOverviewService : IExecutiveOverviewService
{
    private readonly IEnterpriseDashboardService _enterpriseDashboardService;
    private readonly IExecutiveKpiService _executiveKpiService;
    private readonly IDashboardHealthCalculationService _dashboardHealthCalculationService;

    public ExecutiveOverviewService(
        IEnterpriseDashboardService enterpriseDashboardService,
        IExecutiveKpiService executiveKpiService,
        IDashboardHealthCalculationService dashboardHealthCalculationService)
    {
        _enterpriseDashboardService = enterpriseDashboardService;
        _executiveKpiService = executiveKpiService;
        _dashboardHealthCalculationService = dashboardHealthCalculationService;
    }

    public async Task<ExecutiveOverviewResponse> GetOverviewAsync(
        Guid companyId,
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var dashboardParameters = ExecutiveWorkspaceMapping.ToDashboardParameters(companyId, parameters);

        var summaryTask = _enterpriseDashboardService.GetSummaryAsync(dashboardParameters, cancellationToken);
        var portfolioTask = _enterpriseDashboardService.GetPortfolioAsync(dashboardParameters, cancellationToken);
        var capacityTask = _enterpriseDashboardService.GetCapacityAsync(dashboardParameters, cancellationToken);
        var workloadTask = _enterpriseDashboardService.GetWorkloadAsync(dashboardParameters, cancellationToken);
        var auditTask = _enterpriseDashboardService.GetAuditAsync(dashboardParameters, cancellationToken);

        await Task.WhenAll(summaryTask, portfolioTask, capacityTask, workloadTask, auditTask);

        var summary = summaryTask.Result;
        var portfolio = portfolioTask.Result;
        var capacity = capacityTask.Result;
        var workload = workloadTask.Result;
        var audit = auditTask.Result;

        var overallHealth = _dashboardHealthCalculationService.CalculateOverall(
            [summary.OverallHealth.Status, portfolio.OverallHealth.Status]);

        var kpis = _executiveKpiService.BuildKpiSummary(summary, capacity, workload, audit, overallHealth);

        return new ExecutiveOverviewResponse
        {
            CompanyId = companyId,
            GeneratedAt = DateTimeOffset.UtcNow,
            From = parameters.From,
            To = parameters.To,
            Kpis = kpis,
            OverallHealth = overallHealth,
            Narrative = BuildNarrative(summary, capacity, workload, audit),
            NavigationTip =
                "Drill into any section below to open the underlying operational workspace or dashboard — "
                + "Executive Workspace is read-only and never modifies data."
        };
    }

    private static string BuildNarrative(
        EnterpriseDashboardSummaryResponse summary,
        EnterpriseDashboardCapacityResponse capacity,
        EnterpriseDashboardWorkloadResponse workload,
        EnterpriseDashboardAuditResponse audit) =>
        $"{summary.PortfolioCount} portfolios ({summary.ActivePortfolioCount} active), "
        + $"{summary.RecommendationCount} recommendations, "
        + $"{summary.DecisionCount} decisions ({summary.PendingDecisionCount} pending), "
        + $"{capacity.AverageUtilizationPercentage:0.#}% capacity utilization, "
        + $"{workload.AverageWorkloadPercentage:0.#}% workload, "
        + $"{audit.EventCount} audit events in the selected period.";
}
