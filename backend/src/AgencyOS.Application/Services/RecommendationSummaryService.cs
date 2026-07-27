using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

/// <summary>
/// Read-only Recommendation Workspace section builder (US-503 / BR-2601..BR-2610). Projects
/// existing Recommendation, Recommendation Workflow, Recommendation History, AI Recommendation,
/// Explainability, Executive Summary, and Recommendation Comparison data into thin, drill-down
/// ready section DTOs. Never calls a Create/Approve/Reject/Archive/Restore/Generate write API
/// (DEC-503-001).
/// </summary>
public class RecommendationSummaryService : IRecommendationSummaryService
{
    private const int RecommendationCardLimit = 50;
    private const int ApprovalCardLimit = 50;
    private const int RecentHistoryLimit = 20;
    private const int AiCardLimit = 20;
    private const int ExplainabilityCardLimit = 20;
    private const int ExecutiveSummaryCardLimit = 20;

    private readonly IRecommendationService _recommendationService;
    private readonly IRecommendationWorkflowService _recommendationWorkflowService;
    private readonly IRecommendationHistoryService _recommendationHistoryService;
    private readonly IAIRecommendationService _aiRecommendationService;
    private readonly IExplainabilityService _explainabilityService;
    private readonly IExecutiveRecommendationSummaryService _executiveRecommendationSummaryService;
    private readonly IRecommendationComparisonService _recommendationComparisonService;

    public RecommendationSummaryService(
        IRecommendationService recommendationService,
        IRecommendationWorkflowService recommendationWorkflowService,
        IRecommendationHistoryService recommendationHistoryService,
        IAIRecommendationService aiRecommendationService,
        IExplainabilityService explainabilityService,
        IExecutiveRecommendationSummaryService executiveRecommendationSummaryService,
        IRecommendationComparisonService recommendationComparisonService)
    {
        _recommendationService = recommendationService;
        _recommendationWorkflowService = recommendationWorkflowService;
        _recommendationHistoryService = recommendationHistoryService;
        _aiRecommendationService = aiRecommendationService;
        _explainabilityService = explainabilityService;
        _executiveRecommendationSummaryService = executiveRecommendationSummaryService;
        _recommendationComparisonService = recommendationComparisonService;
    }

    public async Task<RecommendationsSectionResponse> GetRecommendationsSectionAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        var recommendations = await _recommendationService.GetByCompanyIdAsync(
            companyId,
            new RecommendationQueryParameters { CompanyId = companyId, Archived = false },
            cancellationToken);

        var cards = recommendations
            .OrderByDescending(recommendation => recommendation.GeneratedAt)
            .Take(RecommendationCardLimit)
            .Select(recommendation => new RecommendationCardResponse
            {
                Id = recommendation.Id,
                RecommendationNumber = recommendation.RecommendationNumber,
                Title = recommendation.Title,
                Status = recommendation.Status,
                Score = recommendation.Score,
                Rank = recommendation.Rank,
                Version = recommendation.Version,
                Archived = recommendation.Archived,
                GeneratedAt = recommendation.GeneratedAt,
                GeneratedBy = recommendation.GeneratedBy,
                DrillDownPath = $"/recommendations/{recommendation.Id}"
            })
            .ToList();

        return new RecommendationsSectionResponse
        {
            CompanyId = companyId,
            Recommendations = cards,
            ListAction = new RecommendationActionResponse
            {
                Key = "list-recommendations",
                Label = "List Recommendations",
                Category = "Recommendations",
                DrillDownPath = $"/recommendations?companyId={companyId}",
                Description = "Browse Recommendations for this Company."
            },
            GenerateAction = new RecommendationActionResponse
            {
                Key = "generate-recommendations",
                Label = "Generate Recommendations",
                Category = "Recommendations",
                DrillDownPath = $"/recommendations?companyId={companyId}",
                Description = "Recommendations are published by the Decision Engine ranking process, which runs outside the Workspace."
            },
            ArchiveRestoreAction = new RecommendationActionResponse
            {
                Key = "archive-restore",
                Label = "Archive / Restore",
                Category = "Recommendations",
                DrillDownPath = $"/recommendations?companyId={companyId}",
                Description = "Select a Recommendation from the list, then archive or restore it via its own detail actions."
            }
        };
    }

    public async Task<ApprovalSectionResponse> GetApprovalSectionAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        var companyRecommendationIdsTask = _recommendationService.GetByCompanyIdAsync(
            companyId,
            new RecommendationQueryParameters { CompanyId = companyId, IncludeArchived = true },
            cancellationToken);

        var workflowsTask = _recommendationWorkflowService.GetAllAsync(
            new RecommendationWorkflowQueryParameters { Status = RecommendationWorkflowStatus.PendingApproval },
            cancellationToken);

        await Task.WhenAll(companyRecommendationIdsTask, workflowsTask);

        var companyRecommendationIds = companyRecommendationIdsTask.Result
            .Select(recommendation => recommendation.Id)
            .ToHashSet();

        var cards = workflowsTask.Result
            .Where(workflow =>
                workflow.CompanyId == companyId
                || (workflow.CompanyId is null && companyRecommendationIds.Contains(workflow.RecommendationId)))
            .OrderByDescending(workflow => workflow.CreatedAt)
            .Take(ApprovalCardLimit)
            .Select(workflow => new RecommendationApprovalCardResponse
            {
                WorkflowId = workflow.Id,
                RecommendationId = workflow.RecommendationId,
                Title = workflow.Title,
                Status = workflow.Status,
                CreatedBy = workflow.CreatedBy,
                CreatedAt = workflow.CreatedAt,
                DrillDownPath = $"/recommendations/workflow/{workflow.Id}",
                ApprovePath = $"/recommendations/workflow/{workflow.Id}/approve",
                RejectPath = $"/recommendations/workflow/{workflow.Id}/reject"
            })
            .ToList();

        return new ApprovalSectionResponse
        {
            CompanyId = companyId,
            PendingApprovals = cards,
            ApprovalQueueAction = new RecommendationActionResponse
            {
                Key = "approval-queue",
                Label = "Approval Queue",
                Category = "Approval",
                DrillDownPath = "/recommendations/workflow",
                Description = "Review Recommendation Workflows awaiting approval.",
                RequiresHumanApproval = true
            },
            StartWorkflowAction = new RecommendationActionResponse
            {
                Key = "start-workflow",
                Label = "Start Workflow",
                Category = "Approval",
                DrillDownPath = "/recommendations/workflow/new",
                Description = "Start a new Recommendation approval Workflow."
            }
        };
    }

    public async Task<HistorySectionResponse> GetHistorySectionAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var history = await _recommendationHistoryService.GetAllAsync(
            new RecommendationHistoryQueryParameters
            {
                CompanyId = companyId,
                CreatedFrom = from,
                CreatedTo = to
            },
            cancellationToken);

        var items = history
            .OrderByDescending(entry => entry.CreatedAt)
            .Take(RecentHistoryLimit)
            .Select(entry => new RecommendationHistoryCardResponse
            {
                Id = entry.Id,
                RecommendationId = entry.RecommendationId,
                RecommendationNumber = entry.RecommendationNumber,
                EventType = entry.EventType,
                RecommendationStatus = entry.RecommendationStatus,
                WorkflowStatus = entry.WorkflowStatus,
                CreatedBy = entry.CreatedBy,
                CreatedAt = entry.CreatedAt,
                DrillDownPath = $"/recommendations/history/{entry.Id}"
            })
            .ToList();

        return new HistorySectionResponse
        {
            CompanyId = companyId,
            From = from,
            To = to,
            Items = items,
            Action = new RecommendationActionResponse
            {
                Key = "history",
                Label = "History",
                Category = "History",
                DrillDownPath = "/recommendations/history",
                Description = "Browse the immutable Recommendation History timeline (BR-2609)."
            }
        };
    }

    public async Task<CompareSectionResponse> GetCompareSectionAsync(
        Guid companyId,
        Guid? leftRecommendationId,
        Guid? rightRecommendationId,
        CancellationToken cancellationToken = default)
    {
        var action = new RecommendationActionResponse
        {
            Key = "compare",
            Label = "Compare",
            Category = "Compare",
            DrillDownPath = "/recommendations/compare",
            Description = "Compare two Recommendation History snapshots side by side."
        };

        if (!leftRecommendationId.HasValue
            || !rightRecommendationId.HasValue
            || leftRecommendationId.Value == rightRecommendationId.Value)
        {
            return new CompareSectionResponse
            {
                CompanyId = companyId,
                LeftRecommendationId = leftRecommendationId,
                RightRecommendationId = rightRecommendationId,
                HasComparison = false,
                Comparison = null,
                Action = action
            };
        }

        var comparison = await _recommendationComparisonService.CompareByIdsAsync(
            leftRecommendationId.Value,
            rightRecommendationId.Value,
            cancellationToken);

        return new CompareSectionResponse
        {
            CompanyId = companyId,
            LeftRecommendationId = leftRecommendationId,
            RightRecommendationId = rightRecommendationId,
            HasComparison = true,
            Comparison = comparison,
            Action = action
        };
    }

    public async Task<AiSectionResponse> GetAiSectionAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var aiTask = _aiRecommendationService.GetAllAsync(
            new AIRecommendationQueryParameters { CompanyId = companyId, GeneratedFrom = from, GeneratedTo = to },
            cancellationToken);

        var explainabilityTask = _explainabilityService.GetAllAsync(
            new ExplainabilityQueryParameters { CompanyId = companyId, GeneratedFrom = from, GeneratedTo = to },
            cancellationToken);

        await Task.WhenAll(aiTask, explainabilityTask);

        var aiCards = aiTask.Result
            .OrderByDescending(item => item.GeneratedAt)
            .Take(AiCardLimit)
            .Select(item => new AIRecommendationCardResponse
            {
                Id = item.Id,
                RecommendationId = item.RecommendationId,
                ConfidenceScore = item.ConfidenceScore,
                Status = item.Status,
                GeneratedAt = item.GeneratedAt,
                GeneratedBy = item.GeneratedBy,
                DrillDownPath = $"/ai-recommendations/{item.Id}"
            })
            .ToList();

        var explainabilityCards = explainabilityTask.Result
            .OrderByDescending(item => item.GeneratedAt)
            .Take(ExplainabilityCardLimit)
            .Select(item => new ExplainabilityCardResponse
            {
                Id = item.Id,
                RecommendationId = item.RecommendationId,
                ExplanationType = item.ExplanationType,
                Status = item.Status,
                GeneratedAt = item.GeneratedAt,
                DrillDownPath = $"/explainability/{item.Id}"
            })
            .ToList();

        return new AiSectionResponse
        {
            CompanyId = companyId,
            AiRecommendations = aiCards,
            Explainability = explainabilityCards,
            AiListAction = new RecommendationActionResponse
            {
                Key = "ai-recommendations",
                Label = "AI Recommendations",
                Category = "AI",
                DrillDownPath = $"/ai-recommendations?companyId={companyId}",
                Description = "Browse advisory AI Recommendations (BR-2605).",
                IsAdvisory = true
            },
            GenerateAiAction = new RecommendationActionResponse
            {
                Key = "generate-ai",
                Label = "Generate AI",
                Category = "AI",
                DrillDownPath = "/ai-recommendations/generate",
                Description = "Generate an advisory AI Recommendation for a Recommendation (BR-2605).",
                IsAdvisory = true
            },
            ExplainabilityListAction = new RecommendationActionResponse
            {
                Key = "explainability",
                Label = "Explainability",
                Category = "Explainability",
                DrillDownPath = $"/explainability?companyId={companyId}",
                Description = "Browse informational Explainability records (BR-2606).",
                IsInformational = true
            },
            GenerateExplainabilityAction = new RecommendationActionResponse
            {
                Key = "generate-explainability",
                Label = "Generate Explainability",
                Category = "Explainability",
                DrillDownPath = "/explainability/generate",
                Description = "Generate an informational Explainability record for a Recommendation (BR-2606).",
                IsInformational = true
            }
        };
    }

    public async Task<ExecutiveSummarySectionResponse> GetExecutiveSummarySectionAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var summaries = await _executiveRecommendationSummaryService.GetAllAsync(
            new ExecutiveRecommendationSummaryQueryParameters
            {
                CompanyId = companyId,
                GeneratedFrom = from,
                GeneratedTo = to
            },
            cancellationToken);

        var cards = summaries
            .OrderByDescending(summary => summary.GeneratedAt)
            .Take(ExecutiveSummaryCardLimit)
            .Select(summary => new ExecutiveSummaryCardResponse
            {
                Id = summary.Id,
                RecommendationId = summary.RecommendationId,
                SummaryVersion = summary.SummaryVersion,
                ConfidenceLevel = summary.ConfidenceLevel,
                Status = summary.Status,
                GeneratedAt = summary.GeneratedAt,
                DrillDownPath = $"/executive-summaries/{summary.Id}"
            })
            .ToList();

        return new ExecutiveSummarySectionResponse
        {
            CompanyId = companyId,
            Summaries = cards,
            ListAction = new RecommendationActionResponse
            {
                Key = "executive-summaries",
                Label = "Executive Summaries",
                Category = "ExecutiveSummary",
                DrillDownPath = $"/executive-summaries?companyId={companyId}",
                Description = "Browse Executive Recommendation Summaries."
            },
            GenerateAction = new RecommendationActionResponse
            {
                Key = "generate-executive-summary",
                Label = "Generate Executive Summary",
                Category = "ExecutiveSummary",
                DrillDownPath = "/executive-summaries/generate",
                Description = "Generate an Executive Recommendation Summary for a Recommendation."
            }
        };
    }
}
