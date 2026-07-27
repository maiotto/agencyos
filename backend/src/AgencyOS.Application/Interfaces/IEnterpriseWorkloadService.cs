using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Builds an enterprise-wide Workload view across a set of Portfolios for Cross-Portfolio Planning
/// (US-405 / BR-2304). Reuses each Portfolio's stored WorkloadSummary snapshot and, when a planning
/// period is supplied, complements it with a live <c>IWorkloadCalculatorService</c> aggregate. Never
/// recalculates or persists a Portfolio's own Workload snapshot.
/// </summary>
public interface IEnterpriseWorkloadService
{
    Task<EnterpriseWorkloadResponse> BuildAsync(
        IReadOnlyList<Portfolio> portfolios,
        DateOnly? periodStart,
        DateOnly? periodEnd,
        CancellationToken cancellationToken = default);
}
