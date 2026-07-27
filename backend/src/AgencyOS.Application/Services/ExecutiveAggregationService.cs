using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;

namespace AgencyOS.Application.Services;

/// <summary>
/// Read-only Executive Workspace section builder (US-505 / BR-2801..BR-2810). Wraps each existing
/// <see cref="IEnterpriseDashboardService"/> section getter into an Executive*SectionResponse —
/// never re-aggregates from repositories directly (BR-2809: no duplicate analytical data).
/// </summary>
public class ExecutiveAggregationService : IExecutiveAggregationService
{
    private readonly IEnterpriseDashboardService _enterpriseDashboardService;

    public ExecutiveAggregationService(IEnterpriseDashboardService enterpriseDashboardService)
    {
        _enterpriseDashboardService = enterpriseDashboardService;
    }

    public async Task<ExecutiveEnterpriseSectionResponse> BuildEnterpriseAsync(
        Guid companyId,
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var summary = await _enterpriseDashboardService.GetSummaryAsync(
            ExecutiveWorkspaceMapping.ToDashboardParameters(companyId, parameters),
            cancellationToken);

        return new ExecutiveEnterpriseSectionResponse
        {
            CompanyId = companyId,
            Summary = summary
        };
    }

    public async Task<ExecutivePortfoliosSectionResponse> BuildPortfoliosAsync(
        Guid companyId,
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await _enterpriseDashboardService.GetPortfolioAsync(
            ExecutiveWorkspaceMapping.ToDashboardParameters(companyId, parameters),
            cancellationToken);

        return new ExecutivePortfoliosSectionResponse
        {
            CompanyId = companyId,
            Portfolio = portfolio
        };
    }

    public async Task<ExecutiveRecommendationsSectionResponse> BuildRecommendationsAsync(
        Guid companyId,
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var recommendations = await _enterpriseDashboardService.GetRecommendationsAsync(
            ExecutiveWorkspaceMapping.ToDashboardParameters(companyId, parameters),
            cancellationToken);

        return new ExecutiveRecommendationsSectionResponse
        {
            CompanyId = companyId,
            Recommendations = recommendations
        };
    }

    public async Task<ExecutiveDecisionsSectionResponse> BuildDecisionsAsync(
        Guid companyId,
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var decisions = await _enterpriseDashboardService.GetDecisionsAsync(
            ExecutiveWorkspaceMapping.ToDashboardParameters(companyId, parameters),
            cancellationToken);

        return new ExecutiveDecisionsSectionResponse
        {
            CompanyId = companyId,
            Decisions = decisions
        };
    }

    public async Task<ExecutiveCapacitySectionResponse> BuildCapacityAsync(
        Guid companyId,
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var capacity = await _enterpriseDashboardService.GetCapacityAsync(
            ExecutiveWorkspaceMapping.ToDashboardParameters(companyId, parameters),
            cancellationToken);

        return new ExecutiveCapacitySectionResponse
        {
            CompanyId = companyId,
            Capacity = capacity
        };
    }

    public async Task<ExecutiveWorkloadSectionResponse> BuildWorkloadAsync(
        Guid companyId,
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var workload = await _enterpriseDashboardService.GetWorkloadAsync(
            ExecutiveWorkspaceMapping.ToDashboardParameters(companyId, parameters),
            cancellationToken);

        return new ExecutiveWorkloadSectionResponse
        {
            CompanyId = companyId,
            Workload = workload
        };
    }

    public async Task<ExecutiveAiSectionResponse> BuildAiAsync(
        Guid companyId,
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var ai = await _enterpriseDashboardService.GetAiAsync(
            ExecutiveWorkspaceMapping.ToDashboardParameters(companyId, parameters),
            cancellationToken);

        return new ExecutiveAiSectionResponse
        {
            CompanyId = companyId,
            Ai = ai
        };
    }

    public async Task<ExecutiveAuditSectionResponse> BuildAuditAsync(
        Guid companyId,
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var audit = await _enterpriseDashboardService.GetAuditAsync(
            ExecutiveWorkspaceMapping.ToDashboardParameters(companyId, parameters),
            cancellationToken);

        return new ExecutiveAuditSectionResponse
        {
            CompanyId = companyId,
            Audit = audit
        };
    }
}
