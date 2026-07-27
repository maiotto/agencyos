using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Pure, read-only Executive KPI mapping (US-505 / BR-2802, BR-2809). Maps the existing
/// <see cref="EnterpriseDashboardSummaryResponse"/> (plus optional Capacity/Workload/Audit
/// sections and an optional overall health override) into an <see cref="ExecutiveKpiSummaryResponse"/>.
/// Never calls a repository or an <see cref="IEnterpriseDashboardService"/> method itself — every
/// input is supplied by the caller.
/// </summary>
public interface IExecutiveKpiService
{
    ExecutiveKpiSummaryResponse BuildKpiSummary(
        EnterpriseDashboardSummaryResponse summary,
        EnterpriseDashboardCapacityResponse? capacity = null,
        EnterpriseDashboardWorkloadResponse? workload = null,
        EnterpriseDashboardAuditResponse? audit = null,
        HealthIndicator? overallHealth = null);
}
