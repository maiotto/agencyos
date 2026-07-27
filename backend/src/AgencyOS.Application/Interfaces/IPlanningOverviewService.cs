using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Read-only Planning Workspace overview aggregation (US-502 / BR-2501..BR-2510). Aggregates
/// company-scoped counts/summaries from Planning Templates, Portfolios, Capacity/Workload
/// History, and Cross-Portfolio Planning scenarios. Never recalculates an engine and never
/// persists a Scenario (DEC-502-001).
/// </summary>
public interface IPlanningOverviewService
{
    Task<PlanningOverviewResponse> GetOverviewAsync(
        Guid companyId,
        PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
