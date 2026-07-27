using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

/// <summary>
/// Read-only Decision Workspace overview aggregation (US-504 / BR-2701..BR-2710). Aggregates
/// company-scoped counts directly from the existing <see cref="IDecisionService"/> (DEC-504-001).
/// Never mutates Decision data.
/// </summary>
public class DecisionOverviewService : IDecisionOverviewService
{
    private readonly IDecisionService _decisionService;

    public DecisionOverviewService(IDecisionService decisionService)
    {
        _decisionService = decisionService;
    }

    public async Task<DecisionOverviewResponse> GetOverviewAsync(
        Guid companyId,
        DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var decisions = await _decisionService.GetAllAsync(
            new DecisionQueryParameters
            {
                CompanyId = companyId,
                DecisionFrom = parameters.From,
                DecisionTo = parameters.To
            },
            cancellationToken);

        return new DecisionOverviewResponse
        {
            CompanyId = companyId,
            GeneratedAt = DateTimeOffset.UtcNow,
            From = parameters.From,
            To = parameters.To,
            Kpis = ComputeKpis(decisions)
        };
    }

    internal static DecisionKpiSummaryResponse ComputeKpis(IReadOnlyList<DecisionResponse> decisions) =>
        new()
        {
            TotalCount = decisions.Count,
            PendingCount = decisions.Count(decision => DecisionStatus.IsCreated(decision.DecisionStatus)),
            InProgressCount = decisions.Count(decision => DecisionStatus.IsInProgress(decision.DecisionStatus)),
            CompletedCount = decisions.Count(decision => DecisionStatus.IsCompleted(decision.DecisionStatus)),
            CancelledCount = decisions.Count(decision => DecisionStatus.IsCancelled(decision.DecisionStatus)),
            WithOutcomeCount = decisions.Count(decision => !string.IsNullOrWhiteSpace(decision.Outcome)),
            ImplementationNotStartedCount = decisions.Count(decision =>
                DecisionImplementationStatus.IsNotStarted(decision.ImplementationStatus))
        };
}
