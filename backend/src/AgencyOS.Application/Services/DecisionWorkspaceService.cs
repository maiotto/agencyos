using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;

namespace AgencyOS.Application.Services;

/// <summary>
/// Read-only Decision Workspace orchestration (US-504 / BR-2701..BR-2710; DEC-504-001). A
/// read-only orchestration facade over the existing Decision lifecycle, Recommendation linkage,
/// Decision Timeline, and Decision Audit services. Resolves the active Company, defaults the
/// reporting window (trailing 30 days), and delegates every section to the existing service that
/// already owns it — never calls CreateAsync/StartImplementationAsync/CompleteAsync/CancelAsync/
/// RecordOutcomeAsync and never bypasses mandatory human approval. Create Decision/Start
/// Implementation/Complete/Cancel/Record Outcome are exposed purely as navigation Decision
/// Actions.
/// </summary>
public class DecisionWorkspaceService : IDecisionWorkspaceService
{
    private const int DefaultWindowDays = 30;

    private readonly IDecisionOverviewService _decisionOverviewService;
    private readonly IDecisionNavigationService _decisionNavigationService;
    private readonly IDecisionSummaryService _decisionSummaryService;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICompanyContext _companyContext;

    public DecisionWorkspaceService(
        IDecisionOverviewService decisionOverviewService,
        IDecisionNavigationService decisionNavigationService,
        IDecisionSummaryService decisionSummaryService,
        ICompanyRepository companyRepository,
        ICompanyContext companyContext)
    {
        _decisionOverviewService = decisionOverviewService;
        _decisionNavigationService = decisionNavigationService;
        _decisionSummaryService = decisionSummaryService;
        _companyRepository = companyRepository;
        _companyContext = companyContext;
    }

    public async Task<DecisionWorkspaceResponse> GetWorkspaceAsync(
        DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);

        var overviewTask = _decisionOverviewService.GetOverviewAsync(companyId, resolved, cancellationToken);
        var decisionsTask = _decisionSummaryService.GetDecisionsSectionAsync(
            companyId,
            resolved.From,
            resolved.To,
            cancellationToken);
        var timelineTask = _decisionSummaryService.GetTimelineSectionAsync(
            companyId,
            resolved.From,
            resolved.To,
            resolved.DecisionId,
            cancellationToken);
        var outcomesTask = _decisionSummaryService.GetOutcomesSectionAsync(
            companyId,
            resolved.From,
            resolved.To,
            cancellationToken);
        var auditTask = _decisionSummaryService.GetAuditSectionAsync(
            companyId,
            resolved.From,
            resolved.To,
            cancellationToken);

        await Task.WhenAll(overviewTask, decisionsTask, timelineTask, outcomesTask, auditTask);

        var overview = overviewTask.Result;
        var navigation = _decisionNavigationService.GetNavigation(companyId);

        return new DecisionWorkspaceResponse
        {
            CompanyId = companyId,
            GeneratedAt = DateTimeOffset.UtcNow,
            From = resolved.From,
            To = resolved.To,
            DecisionId = resolved.DecisionId,
            Kpis = overview.Kpis,
            Overview = overview,
            Decisions = decisionsTask.Result,
            Timeline = timelineTask.Result,
            Outcomes = outcomesTask.Result,
            Audit = auditTask.Result,
            Navigation = navigation
        };
    }

    public async Task<DecisionOverviewResponse> GetOverviewAsync(
        DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _decisionOverviewService.GetOverviewAsync(companyId, resolved, cancellationToken);
    }

    public async Task<DecisionsSectionResponse> GetDecisionsAsync(
        DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _decisionSummaryService.GetDecisionsSectionAsync(
            companyId,
            resolved.From,
            resolved.To,
            cancellationToken);
    }

    public async Task<DecisionTimelineSectionResponse> GetTimelineAsync(
        DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _decisionSummaryService.GetTimelineSectionAsync(
            companyId,
            resolved.From,
            resolved.To,
            resolved.DecisionId,
            cancellationToken);
    }

    public async Task<DecisionOutcomesSectionResponse> GetOutcomesAsync(
        DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _decisionSummaryService.GetOutcomesSectionAsync(
            companyId,
            resolved.From,
            resolved.To,
            cancellationToken);
    }

    public async Task<DecisionAuditSectionResponse> GetAuditAsync(
        DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _decisionSummaryService.GetAuditSectionAsync(
            companyId,
            resolved.From,
            resolved.To,
            cancellationToken);
    }

    public async Task<DecisionKpiSummaryResponse> GetKpisAsync(
        DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        var overview = await _decisionOverviewService.GetOverviewAsync(companyId, resolved, cancellationToken);
        return overview.Kpis;
    }

    /// <summary>Resolves CompanyId per DEC-504-001 and validates it references an existing Company.</summary>
    private async Task<Guid> ResolveAndValidateCompanyIdAsync(
        DecisionWorkspaceQueryParameters parameters,
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

    /// <summary>Resolves the From/To reporting window (trailing 30 days by default).</summary>
    private static DecisionWorkspaceQueryParameters ResolveWindow(DecisionWorkspaceQueryParameters parameters)
    {
        var to = parameters.To ?? DateTimeOffset.UtcNow;
        var from = parameters.From ?? to.AddDays(-DefaultWindowDays);

        return new DecisionWorkspaceQueryParameters
        {
            CompanyId = parameters.CompanyId,
            From = from,
            To = to,
            DecisionId = parameters.DecisionId
        };
    }
}
