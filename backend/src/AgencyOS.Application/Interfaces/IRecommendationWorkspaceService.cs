using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Read-only Recommendation Workspace orchestration (US-503 / BR-2601..BR-2610). A read-only
/// orchestration facade over existing Recommendation, Recommendation Workflow, AI
/// Recommendation, Explainability, Executive Summary, Recommendation History, and
/// Recommendation Comparison services (DEC-503-001) — resolves the active Company
/// (<c>parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId</c>),
/// aggregates existing data, and exposes navigation/drill-down targets. Never calls a
/// Create/Approve/Reject/Archive/Restore/Generate write API and never bypasses mandatory human
/// approval; the Decision Engine and Recommendation lifecycle are never modified.
/// </summary>
public interface IRecommendationWorkspaceService
{
    Task<RecommendationWorkspaceResponse> GetWorkspaceAsync(
        RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<RecommendationOverviewResponse> GetOverviewAsync(
        RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<RecommendationsSectionResponse> GetRecommendationsSectionAsync(
        RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ApprovalSectionResponse> GetApprovalSectionAsync(
        RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<HistorySectionResponse> GetHistorySectionAsync(
        RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<CompareSectionResponse> GetCompareSectionAsync(
        RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<AiSectionResponse> GetAiSectionAsync(
        RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutiveSummarySectionResponse> GetExecutiveSummaryAsync(
        RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
