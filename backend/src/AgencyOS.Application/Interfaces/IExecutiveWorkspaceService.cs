using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Read-only Executive Workspace orchestration (US-505 / BR-2801..BR-2810; DEC-505-001). A
/// read-only orchestration façade that primarily reuses <see cref="IEnterpriseDashboardService"/>
/// section methods — resolves the active Company
/// (<c>parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId</c>),
/// maps the query window onto <see cref="EnterpriseDashboardQueryParameters"/>, and exposes
/// executive navigation deep-links into every existing operational workspace and dashboard.
/// Introduces no new persistence and never modifies operational data.
/// </summary>
public interface IExecutiveWorkspaceService
{
    Task<ExecutiveWorkspaceResponse> GetWorkspaceAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutiveOverviewResponse> GetOverviewAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutiveEnterpriseSectionResponse> GetEnterpriseAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutivePortfoliosSectionResponse> GetPortfoliosAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutiveRecommendationsSectionResponse> GetRecommendationsAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutiveDecisionsSectionResponse> GetDecisionsAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutiveCapacitySectionResponse> GetCapacityAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutiveWorkloadSectionResponse> GetWorkloadAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutiveAiSectionResponse> GetAiAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutiveAuditSectionResponse> GetAuditAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
