using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;

namespace AgencyOS.Application.Services;

/// <summary>
/// Read-only Recommendation Workspace orchestration (US-503 / BR-2601..BR-2610; DEC-503-001). A
/// read-only orchestration facade over existing Recommendation, Recommendation Workflow, AI
/// Recommendation, Explainability, Executive Summary, Recommendation History, and Recommendation
/// Comparison services. Resolves the active Company, defaults the reporting window (trailing 30
/// days), and delegates every section to the existing service that already owns it — never calls
/// a Create/Approve/Reject/Archive/Restore/Generate write API and never bypasses mandatory human
/// approval. Generate/Approve/Reject/Archive/Restore/Start Workflow/Generate AI/Generate
/// Explainability/Generate Executive Summary are exposed purely as navigation Recommendation
/// Actions.
/// </summary>
public class RecommendationWorkspaceService : IRecommendationWorkspaceService
{
    private const int DefaultWindowDays = 30;

    private readonly IRecommendationOverviewService _recommendationOverviewService;
    private readonly IRecommendationNavigationService _recommendationNavigationService;
    private readonly IRecommendationSummaryService _recommendationSummaryService;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICompanyContext _companyContext;

    public RecommendationWorkspaceService(
        IRecommendationOverviewService recommendationOverviewService,
        IRecommendationNavigationService recommendationNavigationService,
        IRecommendationSummaryService recommendationSummaryService,
        ICompanyRepository companyRepository,
        ICompanyContext companyContext)
    {
        _recommendationOverviewService = recommendationOverviewService;
        _recommendationNavigationService = recommendationNavigationService;
        _recommendationSummaryService = recommendationSummaryService;
        _companyRepository = companyRepository;
        _companyContext = companyContext;
    }

    public async Task<RecommendationWorkspaceResponse> GetWorkspaceAsync(
        RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);

        var overviewTask = _recommendationOverviewService.GetOverviewAsync(companyId, resolved, cancellationToken);
        var recommendationsTask = _recommendationSummaryService.GetRecommendationsSectionAsync(
            companyId,
            cancellationToken);
        var approvalTask = _recommendationSummaryService.GetApprovalSectionAsync(companyId, cancellationToken);
        var historyTask = _recommendationSummaryService.GetHistorySectionAsync(
            companyId,
            resolved.From,
            resolved.To,
            cancellationToken);
        var compareTask = _recommendationSummaryService.GetCompareSectionAsync(
            companyId,
            resolved.LeftRecommendationId,
            resolved.RightRecommendationId,
            cancellationToken);
        var aiTask = _recommendationSummaryService.GetAiSectionAsync(
            companyId,
            resolved.From,
            resolved.To,
            cancellationToken);
        var executiveSummaryTask = _recommendationSummaryService.GetExecutiveSummarySectionAsync(
            companyId,
            resolved.From,
            resolved.To,
            cancellationToken);

        await Task.WhenAll(
            overviewTask,
            recommendationsTask,
            approvalTask,
            historyTask,
            compareTask,
            aiTask,
            executiveSummaryTask);

        var overview = overviewTask.Result;
        var navigation = _recommendationNavigationService.GetNavigation(companyId);

        return new RecommendationWorkspaceResponse
        {
            CompanyId = companyId,
            GeneratedAt = DateTimeOffset.UtcNow,
            From = resolved.From,
            To = resolved.To,
            Kpis = overview.Kpis,
            Overview = overview,
            Recommendations = recommendationsTask.Result,
            Approval = approvalTask.Result,
            History = historyTask.Result,
            Compare = compareTask.Result,
            Ai = aiTask.Result,
            ExecutiveSummary = executiveSummaryTask.Result,
            Navigation = navigation
        };
    }

    public async Task<RecommendationOverviewResponse> GetOverviewAsync(
        RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _recommendationOverviewService.GetOverviewAsync(companyId, resolved, cancellationToken);
    }

    public async Task<RecommendationsSectionResponse> GetRecommendationsSectionAsync(
        RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        return await _recommendationSummaryService.GetRecommendationsSectionAsync(companyId, cancellationToken);
    }

    public async Task<ApprovalSectionResponse> GetApprovalSectionAsync(
        RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        return await _recommendationSummaryService.GetApprovalSectionAsync(companyId, cancellationToken);
    }

    public async Task<HistorySectionResponse> GetHistorySectionAsync(
        RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _recommendationSummaryService.GetHistorySectionAsync(
            companyId,
            resolved.From,
            resolved.To,
            cancellationToken);
    }

    public async Task<CompareSectionResponse> GetCompareSectionAsync(
        RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        return await _recommendationSummaryService.GetCompareSectionAsync(
            companyId,
            parameters.LeftRecommendationId,
            parameters.RightRecommendationId,
            cancellationToken);
    }

    public async Task<AiSectionResponse> GetAiSectionAsync(
        RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _recommendationSummaryService.GetAiSectionAsync(
            companyId,
            resolved.From,
            resolved.To,
            cancellationToken);
    }

    public async Task<ExecutiveSummarySectionResponse> GetExecutiveSummaryAsync(
        RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _recommendationSummaryService.GetExecutiveSummarySectionAsync(
            companyId,
            resolved.From,
            resolved.To,
            cancellationToken);
    }

    /// <summary>Resolves CompanyId per DEC-503-001 and validates it references an existing Company.</summary>
    private async Task<Guid> ResolveAndValidateCompanyIdAsync(
        RecommendationWorkspaceQueryParameters parameters,
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
    private static RecommendationWorkspaceQueryParameters ResolveWindow(
        RecommendationWorkspaceQueryParameters parameters)
    {
        var to = parameters.To ?? DateTimeOffset.UtcNow;
        var from = parameters.From ?? to.AddDays(-DefaultWindowDays);

        return new RecommendationWorkspaceQueryParameters
        {
            CompanyId = parameters.CompanyId,
            From = from,
            To = to,
            LeftRecommendationId = parameters.LeftRecommendationId,
            RightRecommendationId = parameters.RightRecommendationId
        };
    }
}
