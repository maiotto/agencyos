using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Read-only Executive Workspace overview aggregation (US-505 / BR-2801..BR-2810). Combines the
/// existing Enterprise Dashboard Summary, Portfolio, Capacity, Workload, and Audit sections into
/// Executive KPIs, an overall health rollup (reused from
/// <see cref="IDashboardHealthCalculationService"/> — BR-2805), and a short narrative. Never
/// mutates data (DEC-505-001).
/// </summary>
public interface IExecutiveOverviewService
{
    Task<ExecutiveOverviewResponse> GetOverviewAsync(
        Guid companyId,
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
