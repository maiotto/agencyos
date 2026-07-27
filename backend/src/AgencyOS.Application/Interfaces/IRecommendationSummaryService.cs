using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Read-only Recommendation Workspace section builder (US-503 / BR-2601..BR-2610). Projects
/// existing Recommendation, Recommendation Workflow, Recommendation History, AI Recommendation,
/// Explainability, Executive Summary, and Recommendation Comparison data into thin, drill-down
/// ready section DTOs. Never calls a Create/Approve/Reject/Archive/Restore/Generate write API
/// on any of the underlying services (DEC-503-001).
/// </summary>
public interface IRecommendationSummaryService
{
    Task<RecommendationsSectionResponse> GetRecommendationsSectionAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task<ApprovalSectionResponse> GetApprovalSectionAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task<HistorySectionResponse> GetHistorySectionAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default);

    Task<CompareSectionResponse> GetCompareSectionAsync(
        Guid companyId,
        Guid? leftRecommendationId,
        Guid? rightRecommendationId,
        CancellationToken cancellationToken = default);

    Task<AiSectionResponse> GetAiSectionAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default);

    Task<ExecutiveSummarySectionResponse> GetExecutiveSummarySectionAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default);
}
