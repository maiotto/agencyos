using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Read-only Decision Workspace orchestration (US-504 / BR-2701..BR-2710). A read-only
/// orchestration facade over the existing Decision lifecycle, Recommendation linkage, Decision
/// Timeline, and Decision Audit services (DEC-504-001) — resolves the active Company
/// (<c>parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId</c>),
/// aggregates existing data, and exposes navigation/drill-down targets. Never calls
/// CreateAsync/StartImplementationAsync/CompleteAsync/CancelAsync/RecordOutcomeAsync and never
/// bypasses mandatory human approval; the Decision lifecycle is never modified.
/// </summary>
public interface IDecisionWorkspaceService
{
    Task<DecisionWorkspaceResponse> GetWorkspaceAsync(
        DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<DecisionOverviewResponse> GetOverviewAsync(
        DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<DecisionsSectionResponse> GetDecisionsAsync(
        DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<DecisionTimelineSectionResponse> GetTimelineAsync(
        DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<DecisionOutcomesSectionResponse> GetOutcomesAsync(
        DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<DecisionAuditSectionResponse> GetAuditAsync(
        DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<DecisionKpiSummaryResponse> GetKpisAsync(
        DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
