using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

/// <summary>
/// Read-only Recommendation Workspace overview aggregation (US-503 / BR-2601..BR-2610).
/// Aggregates company-scoped counts directly from existing Recommendation, Recommendation
/// Workflow, AI Recommendation, Explainability, Executive Summary, and Recommendation History
/// services (DEC-503-001). Never mutates any of them.
/// </summary>
public class RecommendationOverviewService : IRecommendationOverviewService
{
    private readonly IRecommendationService _recommendationService;
    private readonly IRecommendationWorkflowService _recommendationWorkflowService;
    private readonly IRecommendationHistoryService _recommendationHistoryService;
    private readonly IAIRecommendationService _aiRecommendationService;
    private readonly IExplainabilityService _explainabilityService;
    private readonly IExecutiveRecommendationSummaryService _executiveRecommendationSummaryService;

    public RecommendationOverviewService(
        IRecommendationService recommendationService,
        IRecommendationWorkflowService recommendationWorkflowService,
        IRecommendationHistoryService recommendationHistoryService,
        IAIRecommendationService aiRecommendationService,
        IExplainabilityService explainabilityService,
        IExecutiveRecommendationSummaryService executiveRecommendationSummaryService)
    {
        _recommendationService = recommendationService;
        _recommendationWorkflowService = recommendationWorkflowService;
        _recommendationHistoryService = recommendationHistoryService;
        _aiRecommendationService = aiRecommendationService;
        _explainabilityService = explainabilityService;
        _executiveRecommendationSummaryService = executiveRecommendationSummaryService;
    }

    public async Task<RecommendationOverviewResponse> GetOverviewAsync(
        Guid companyId,
        RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var recommendations = await _recommendationService.GetByCompanyIdAsync(
            companyId,
            new RecommendationQueryParameters { CompanyId = companyId, IncludeArchived = true },
            cancellationToken);

        var workflows = await _recommendationWorkflowService.GetAllAsync(
            new RecommendationWorkflowQueryParameters { Status = RecommendationWorkflowStatus.PendingApproval },
            cancellationToken);

        var ai = await _aiRecommendationService.GetAllAsync(
            new AIRecommendationQueryParameters
            {
                CompanyId = companyId,
                GeneratedFrom = parameters.From,
                GeneratedTo = parameters.To
            },
            cancellationToken);

        var explainability = await _explainabilityService.GetAllAsync(
            new ExplainabilityQueryParameters
            {
                CompanyId = companyId,
                GeneratedFrom = parameters.From,
                GeneratedTo = parameters.To
            },
            cancellationToken);

        var executiveSummary = await _executiveRecommendationSummaryService.GetAllAsync(
            new ExecutiveRecommendationSummaryQueryParameters
            {
                CompanyId = companyId,
                GeneratedFrom = parameters.From,
                GeneratedTo = parameters.To
            },
            cancellationToken);

        var history = await _recommendationHistoryService.GetAllAsync(
            new RecommendationHistoryQueryParameters
            {
                CompanyId = companyId,
                CreatedFrom = parameters.From,
                CreatedTo = parameters.To
            },
            cancellationToken);

        var companyRecommendationIds = recommendations
            .Select(recommendation => recommendation.Id)
            .ToHashSet();

        var pendingApprovalCount = workflows.Count(workflow =>
            workflow.CompanyId == companyId
            || (workflow.CompanyId is null && companyRecommendationIds.Contains(workflow.RecommendationId)));

        return new RecommendationOverviewResponse
        {
            CompanyId = companyId,
            GeneratedAt = DateTimeOffset.UtcNow,
            From = parameters.From,
            To = parameters.To,
            Kpis = new RecommendationKpiSummaryResponse
            {
                ActiveRecommendationCount = recommendations.Count(recommendation => !recommendation.Archived),
                ArchivedRecommendationCount = recommendations.Count(recommendation => recommendation.Archived),
                PendingApprovalCount = pendingApprovalCount,
                AiRecommendationCount = ai.Count,
                ExplainabilityCount = explainability.Count,
                ExecutiveSummaryCount = executiveSummary.Count,
                HistoryEventCount = history.Count
            }
        };
    }
}
