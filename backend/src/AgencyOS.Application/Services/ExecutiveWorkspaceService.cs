using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;

namespace AgencyOS.Application.Services;

/// <summary>
/// Read-only Executive Workspace orchestration (US-505 / BR-2801..BR-2810; DEC-505-001). A
/// read-only orchestration facade that primarily reuses <see cref="IEnterpriseDashboardService"/>
/// section methods via <see cref="IExecutiveAggregationService"/>. Resolves the active Company,
/// defaults the reporting window (trailing 30 days), and delegates every section to the existing
/// Enterprise Dashboard — never writes operational data. Adds executive navigation deep-links
/// into every existing operational workspace and dashboard.
/// </summary>
public class ExecutiveWorkspaceService : IExecutiveWorkspaceService
{
    private const int DefaultWindowDays = 30;

    private readonly IExecutiveOverviewService _executiveOverviewService;
    private readonly IExecutiveAggregationService _executiveAggregationService;
    private readonly IExecutiveNavigationService _executiveNavigationService;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICompanyContext _companyContext;

    public ExecutiveWorkspaceService(
        IExecutiveOverviewService executiveOverviewService,
        IExecutiveAggregationService executiveAggregationService,
        IExecutiveNavigationService executiveNavigationService,
        ICompanyRepository companyRepository,
        ICompanyContext companyContext)
    {
        _executiveOverviewService = executiveOverviewService;
        _executiveAggregationService = executiveAggregationService;
        _executiveNavigationService = executiveNavigationService;
        _companyRepository = companyRepository;
        _companyContext = companyContext;
    }

    public async Task<ExecutiveWorkspaceResponse> GetWorkspaceAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);

        var overview = await _executiveOverviewService.GetOverviewAsync(companyId, resolved, cancellationToken);
        var enterprise = await _executiveAggregationService.BuildEnterpriseAsync(companyId, resolved, cancellationToken);
        var portfolios = await _executiveAggregationService.BuildPortfoliosAsync(companyId, resolved, cancellationToken);
        var recommendations = await _executiveAggregationService.BuildRecommendationsAsync(companyId, resolved, cancellationToken);
        var decisions = await _executiveAggregationService.BuildDecisionsAsync(companyId, resolved, cancellationToken);
        var capacity = await _executiveAggregationService.BuildCapacityAsync(companyId, resolved, cancellationToken);
        var workload = await _executiveAggregationService.BuildWorkloadAsync(companyId, resolved, cancellationToken);
        var ai = await _executiveAggregationService.BuildAiAsync(companyId, resolved, cancellationToken);
        var audit = await _executiveAggregationService.BuildAuditAsync(companyId, resolved, cancellationToken);
        var navigation = _executiveNavigationService.GetNavigation(companyId);

        return new ExecutiveWorkspaceResponse
        {
            CompanyId = companyId,
            GeneratedAt = DateTimeOffset.UtcNow,
            From = resolved.From,
            To = resolved.To,
            PeriodStart = resolved.PeriodStart,
            PeriodEnd = resolved.PeriodEnd,
            Kpis = overview.Kpis,
            Overview = overview,
            Enterprise = enterprise,
            Portfolios = portfolios,
            Recommendations = recommendations,
            Decisions = decisions,
            Capacity = capacity,
            Workload = workload,
            Ai = ai,
            Audit = audit,
            Navigation = navigation
        };
    }

    public async Task<ExecutiveOverviewResponse> GetOverviewAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _executiveOverviewService.GetOverviewAsync(companyId, resolved, cancellationToken);
    }

    public async Task<ExecutiveEnterpriseSectionResponse> GetEnterpriseAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _executiveAggregationService.BuildEnterpriseAsync(companyId, resolved, cancellationToken);
    }

    public async Task<ExecutivePortfoliosSectionResponse> GetPortfoliosAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _executiveAggregationService.BuildPortfoliosAsync(companyId, resolved, cancellationToken);
    }

    public async Task<ExecutiveRecommendationsSectionResponse> GetRecommendationsAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _executiveAggregationService.BuildRecommendationsAsync(companyId, resolved, cancellationToken);
    }

    public async Task<ExecutiveDecisionsSectionResponse> GetDecisionsAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _executiveAggregationService.BuildDecisionsAsync(companyId, resolved, cancellationToken);
    }

    public async Task<ExecutiveCapacitySectionResponse> GetCapacityAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _executiveAggregationService.BuildCapacityAsync(companyId, resolved, cancellationToken);
    }

    public async Task<ExecutiveWorkloadSectionResponse> GetWorkloadAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _executiveAggregationService.BuildWorkloadAsync(companyId, resolved, cancellationToken);
    }

    public async Task<ExecutiveAiSectionResponse> GetAiAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _executiveAggregationService.BuildAiAsync(companyId, resolved, cancellationToken);
    }

    public async Task<ExecutiveAuditSectionResponse> GetAuditAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _executiveAggregationService.BuildAuditAsync(companyId, resolved, cancellationToken);
    }

    /// <summary>Resolves CompanyId per DEC-505-001 and validates it references an existing Company.</summary>
    private async Task<Guid> ResolveAndValidateCompanyIdAsync(
        ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var companyId = parameters.CompanyId ?? _companyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId;

        var company = await _companyRepository.GetByIdAsync(companyId, cancellationToken);
        if (company is null)
        {
            throw new NotFoundException($"Company with id '{companyId}' was not found.");
        }

        return companyId;
    }

    /// <summary>Resolves the From/To and PeriodStart/PeriodEnd reporting window (trailing 30 days by default).</summary>
    private static ExecutiveWorkspaceQueryParameters ResolveWindow(ExecutiveWorkspaceQueryParameters parameters)
    {
        var to = parameters.To ?? DateTimeOffset.UtcNow;
        var from = parameters.From ?? to.AddDays(-DefaultWindowDays);

        var periodEnd = parameters.PeriodEnd ?? DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var periodStart = parameters.PeriodStart ?? periodEnd.AddDays(-DefaultWindowDays);

        return new ExecutiveWorkspaceQueryParameters
        {
            CompanyId = parameters.CompanyId,
            From = from,
            To = to,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd
        };
    }
}
