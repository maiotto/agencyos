using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Read-only Decision Workspace overview aggregation (US-504 / BR-2701..BR-2710). Aggregates
/// company-scoped Decision counts from the existing <see cref="IDecisionService"/> (DEC-504-001).
/// Never mutates Decision data.
/// </summary>
public interface IDecisionOverviewService
{
    Task<DecisionOverviewResponse> GetOverviewAsync(
        Guid companyId,
        DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
