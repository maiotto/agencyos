using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;

namespace AgencyOS.Application.Services;

/// <summary>
/// Pure, read-only Executive KPI mapping (US-505 / BR-2802, BR-2809). Maps existing Enterprise
/// Dashboard section responses into an <see cref="ExecutiveKpiSummaryResponse"/> — never calls a
/// repository or an <see cref="IEnterpriseDashboardService"/> method itself.
/// </summary>
public class ExecutiveKpiService : IExecutiveKpiService
{
    public ExecutiveKpiSummaryResponse BuildKpiSummary(
        EnterpriseDashboardSummaryResponse summary,
        EnterpriseDashboardCapacityResponse? capacity = null,
        EnterpriseDashboardWorkloadResponse? workload = null,
        EnterpriseDashboardAuditResponse? audit = null,
        HealthIndicator? overallHealth = null) =>
        new()
        {
            PortfolioCount = summary.PortfolioCount,
            ActivePortfolioCount = summary.ActivePortfolioCount,
            RecommendationCount = summary.RecommendationCount,
            DecisionCount = summary.DecisionCount,
            PendingDecisionCount = summary.PendingDecisionCount,
            CompletedDecisionCount = summary.CompletedDecisionCount,
            CapacityUtilizationPercentage = capacity?.AverageUtilizationPercentage ?? 0m,
            WorkloadPercentage = workload?.AverageWorkloadPercentage ?? 0m,
            AuditEventCount = audit?.EventCount ?? 0,
            OverallHealth = overallHealth ?? summary.OverallHealth
        };
}
