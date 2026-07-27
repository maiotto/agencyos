using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Read-only Executive Workspace section builder (US-505 / BR-2801..BR-2810). Wraps each existing
/// <see cref="IEnterpriseDashboardService"/> section getter into an Executive*SectionResponse that
/// embeds the underlying dashboard data plus a <c>DrillDownPath</c> into the matching operational
/// workspace or dashboard (BR-2809: no duplicate analytical data — every section is a thin
/// projection over the existing Enterprise Dashboard, never a new repository aggregation).
/// </summary>
public interface IExecutiveAggregationService
{
    Task<ExecutiveEnterpriseSectionResponse> BuildEnterpriseAsync(
        Guid companyId,
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutivePortfoliosSectionResponse> BuildPortfoliosAsync(
        Guid companyId,
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutiveRecommendationsSectionResponse> BuildRecommendationsAsync(
        Guid companyId,
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutiveDecisionsSectionResponse> BuildDecisionsAsync(
        Guid companyId,
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutiveCapacitySectionResponse> BuildCapacityAsync(
        Guid companyId,
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutiveWorkloadSectionResponse> BuildWorkloadAsync(
        Guid companyId,
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutiveAiSectionResponse> BuildAiAsync(
        Guid companyId,
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutiveAuditSectionResponse> BuildAuditAsync(
        Guid companyId,
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
