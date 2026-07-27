using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Read-only Recommendation Workspace overview aggregation (US-503 / BR-2601..BR-2610).
/// Aggregates company-scoped counts/summaries from Recommendations, Recommendation Workflows,
/// AI Recommendations, Explainability, Executive Summaries, and Recommendation History. Never
/// mutates any of them and never bypasses mandatory human approval (DEC-503-001).
/// </summary>
public interface IRecommendationOverviewService
{
    Task<RecommendationOverviewResponse> GetOverviewAsync(
        Guid companyId,
        RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
