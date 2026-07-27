using System.Text.Json;
using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

/// <summary>
/// Builds raw Enterprise Dashboard section data directly from existing read repositories
/// (US-403 / BR-2102). Never recalculates Capacity/Workload history and never persists
/// new analytical storage — every number is a projection over already-computed data.
/// </summary>
public class DashboardAggregationService : IDashboardAggregationService
{
    private readonly IRecommendationRepository _recommendationRepository;
    private readonly IDecisionRepository _decisionRepository;
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly ICapacityHistoryRepository _capacityHistoryRepository;
    private readonly IWorkloadHistoryRepository _workloadHistoryRepository;
    private readonly IPlanningTemplateRepository _planningTemplateRepository;
    private readonly IAIRecommendationRepository _aiRecommendationRepository;
    private readonly IExplainabilityRepository _explainabilityRepository;
    private readonly IExecutiveRecommendationSummaryRepository _executiveRecommendationSummaryRepository;
    private readonly IAuditEventRepository _auditEventRepository;
    private readonly IDashboardHealthCalculationService _healthCalculationService;

    public DashboardAggregationService(
        IRecommendationRepository recommendationRepository,
        IDecisionRepository decisionRepository,
        IPortfolioRepository portfolioRepository,
        ICapacityHistoryRepository capacityHistoryRepository,
        IWorkloadHistoryRepository workloadHistoryRepository,
        IPlanningTemplateRepository planningTemplateRepository,
        IAIRecommendationRepository aiRecommendationRepository,
        IExplainabilityRepository explainabilityRepository,
        IExecutiveRecommendationSummaryRepository executiveRecommendationSummaryRepository,
        IAuditEventRepository auditEventRepository,
        IDashboardHealthCalculationService healthCalculationService)
    {
        _recommendationRepository = recommendationRepository;
        _decisionRepository = decisionRepository;
        _portfolioRepository = portfolioRepository;
        _capacityHistoryRepository = capacityHistoryRepository;
        _workloadHistoryRepository = workloadHistoryRepository;
        _planningTemplateRepository = planningTemplateRepository;
        _aiRecommendationRepository = aiRecommendationRepository;
        _explainabilityRepository = explainabilityRepository;
        _executiveRecommendationSummaryRepository = executiveRecommendationSummaryRepository;
        _auditEventRepository = auditEventRepository;
        _healthCalculationService = healthCalculationService;
    }

    public async Task<EnterpriseDashboardSummaryResponse> BuildSummaryAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var portfolios = await _portfolioRepository.GetAllAsync(
            new PortfolioQueryParameters
            {
                CompanyId = companyId,
                PeriodStart = parameters.PeriodStart,
                PeriodEnd = parameters.PeriodEnd
            },
            cancellationToken);

        var templates = await _planningTemplateRepository.GetAllAsync(
            new PlanningTemplateQueryParameters { CompanyId = companyId },
            cancellationToken);

        var recommendations = await _recommendationRepository.QueryAsync(
            new RecommendationQueryParameters
            {
                CompanyId = companyId,
                GeneratedFrom = parameters.From,
                GeneratedTo = parameters.To,
                IncludeArchived = true
            },
            cancellationToken);

        var decisions = await _decisionRepository.GetAllAsync(
            new DecisionQueryParameters
            {
                CompanyId = companyId,
                DecisionFrom = parameters.From,
                DecisionTo = parameters.To
            },
            cancellationToken);

        var overallHealth = _healthCalculationService.CalculateOverall(
            portfolios.Select(portfolio => portfolio.PortfolioHealth).ToList());

        return new EnterpriseDashboardSummaryResponse
        {
            CompanyId = companyId,
            GeneratedAt = DateTimeOffset.UtcNow,
            PortfolioCount = portfolios.Count,
            ActivePortfolioCount = portfolios.Count(portfolio => portfolio.IsActive),
            PlanningTemplateCount = templates.Count,
            RecommendationCount = recommendations.Count,
            DecisionCount = decisions.Count,
            PendingDecisionCount = decisions.Count(decision =>
                !DecisionStatus.IsCompleted(decision.DecisionStatus)
                && !DecisionStatus.IsCancelled(decision.DecisionStatus)),
            CompletedDecisionCount = decisions.Count(decision =>
                DecisionStatus.IsCompleted(decision.DecisionStatus)),
            OverallHealth = overallHealth,
            DrillDownPath = $"/portfolios?companyId={companyId}"
        };
    }

    public async Task<EnterpriseDashboardPlanningResponse> BuildPlanningAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var templates = await _planningTemplateRepository.GetAllAsync(
            new PlanningTemplateQueryParameters { CompanyId = companyId },
            cancellationToken);

        return new EnterpriseDashboardPlanningResponse
        {
            TemplateCount = templates.Count,
            ActiveTemplateCount = templates.Count(template => template.IsActive),
            InactiveTemplateCount = templates.Count(template => template.IsInactive),
            StatusBreakdown = BuildStatusBreakdown(templates.Select(template => template.Status)),
            DrillDownPath = $"/planning-templates?companyId={companyId}"
        };
    }

    public async Task<EnterpriseDashboardPortfolioResponse> BuildPortfolioAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var portfolios = await _portfolioRepository.GetAllAsync(
            new PortfolioQueryParameters
            {
                CompanyId = companyId,
                PeriodStart = parameters.PeriodStart,
                PeriodEnd = parameters.PeriodEnd
            },
            cancellationToken);

        var utilizationValues = portfolios
            .Select(portfolio => TryReadDecimal(portfolio.CapacitySummary, "overallUtilizationPercentage"))
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();

        var workloadValues = portfolios
            .Select(portfolio => TryReadDecimal(portfolio.WorkloadSummary, "overallWorkloadPercentage"))
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();

        var averageUtilization = utilizationValues.Count > 0 ? utilizationValues.Average() : 0m;
        var averageWorkload = workloadValues.Count > 0 ? workloadValues.Average() : 0m;

        var overallHealth = _healthCalculationService.CalculateOverall(
            portfolios.Select(portfolio => portfolio.PortfolioHealth).ToList());

        return new EnterpriseDashboardPortfolioResponse
        {
            PortfolioCount = portfolios.Count,
            StatusBreakdown = BuildStatusBreakdown(portfolios.Select(portfolio => portfolio.Status)),
            HealthBreakdown = BuildStatusBreakdown(portfolios.Select(portfolio => portfolio.PortfolioHealth)),
            AverageUtilizationPercentage = decimal.Round(averageUtilization, 2, MidpointRounding.AwayFromZero),
            AverageWorkloadPercentage = decimal.Round(averageWorkload, 2, MidpointRounding.AwayFromZero),
            OverallHealth = overallHealth,
            DrillDownPath = $"/portfolios?companyId={companyId}"
        };
    }

    public async Task<EnterpriseDashboardCapacityResponse> BuildCapacityAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var histories = await _capacityHistoryRepository.QueryAsync(
            new CapacityHistoryQueryParameters
            {
                CompanyId = companyId,
                PeriodStart = parameters.PeriodStart,
                PeriodEnd = parameters.PeriodEnd
            },
            cancellationToken);

        var averageUtilization = histories.Count > 0
            ? histories.Average(history => history.UtilizationPercentage)
            : 0m;

        return new EnterpriseDashboardCapacityResponse
        {
            RecordCount = histories.Count,
            TotalCapacityHours = histories.Sum(history => history.CapacityHours),
            TotalAllocatedHours = histories.Sum(history => history.AllocatedHours),
            AverageUtilizationPercentage = decimal.Round(averageUtilization, 2, MidpointRounding.AwayFromZero),
            UtilizationHealth = _healthCalculationService.CalculateFromUtilization(averageUtilization, 0m),
            DrillDownPath = $"/capacity/history?companyId={companyId}"
        };
    }

    public async Task<EnterpriseDashboardWorkloadResponse> BuildWorkloadAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var histories = await _workloadHistoryRepository.QueryAsync(
            new WorkloadHistoryQueryParameters
            {
                CompanyId = companyId,
                PeriodStart = parameters.PeriodStart,
                PeriodEnd = parameters.PeriodEnd
            },
            cancellationToken);

        var averageWorkload = histories.Count > 0
            ? histories.Average(history => history.WorkloadPercentage)
            : 0m;

        return new EnterpriseDashboardWorkloadResponse
        {
            RecordCount = histories.Count,
            TotalAllocatedHours = histories.Sum(history => history.AllocatedHours),
            TotalCapacityHours = histories.Sum(history => history.CapacityHours),
            AverageWorkloadPercentage = decimal.Round(averageWorkload, 2, MidpointRounding.AwayFromZero),
            WorkloadHealth = _healthCalculationService.CalculateFromUtilization(0m, averageWorkload),
            DrillDownPath = $"/workload/history?companyId={companyId}"
        };
    }

    public async Task<EnterpriseDashboardRecommendationsResponse> BuildRecommendationsAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var recommendations = await _recommendationRepository.QueryAsync(
            new RecommendationQueryParameters
            {
                CompanyId = companyId,
                GeneratedFrom = parameters.From,
                GeneratedTo = parameters.To,
                IncludeArchived = true
            },
            cancellationToken);

        var scored = recommendations.Where(recommendation => recommendation.Score.HasValue).ToList();

        return new EnterpriseDashboardRecommendationsResponse
        {
            TotalCount = recommendations.Count,
            ActiveCount = recommendations.Count(recommendation => !recommendation.Archived),
            ArchivedCount = recommendations.Count(recommendation => recommendation.Archived),
            StatusBreakdown = BuildStatusBreakdown(recommendations.Select(recommendation => recommendation.Status)),
            AverageScore = scored.Count > 0
                ? decimal.Round(scored.Average(recommendation => recommendation.Score!.Value), 2, MidpointRounding.AwayFromZero)
                : null,
            DrillDownPath = $"/recommendations?companyId={companyId}"
        };
    }

    public async Task<EnterpriseDashboardDecisionsResponse> BuildDecisionsAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var decisions = await _decisionRepository.GetAllAsync(
            new DecisionQueryParameters
            {
                CompanyId = companyId,
                DecisionFrom = parameters.From,
                DecisionTo = parameters.To
            },
            cancellationToken);

        return new EnterpriseDashboardDecisionsResponse
        {
            TotalCount = decisions.Count,
            DecisionStatusBreakdown = BuildStatusBreakdown(decisions.Select(decision => decision.DecisionStatus)),
            ImplementationStatusBreakdown = BuildStatusBreakdown(
                decisions.Select(decision => decision.ImplementationStatus)),
            CompletedCount = decisions.Count(decision => DecisionStatus.IsCompleted(decision.DecisionStatus)),
            CancelledCount = decisions.Count(decision => DecisionStatus.IsCancelled(decision.DecisionStatus)),
            DrillDownPath = $"/decisions?companyId={companyId}"
        };
    }

    public async Task<EnterpriseDashboardAiResponse> BuildAiAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var aiRecommendations = await _aiRecommendationRepository.QueryAsync(
            new AIRecommendationQueryParameters
            {
                CompanyId = companyId,
                GeneratedFrom = parameters.From,
                GeneratedTo = parameters.To
            },
            cancellationToken);

        var explainabilities = await _explainabilityRepository.QueryAsync(
            new ExplainabilityQueryParameters
            {
                CompanyId = companyId,
                GeneratedFrom = parameters.From,
                GeneratedTo = parameters.To
            },
            cancellationToken);

        var executiveSummaries = await _executiveRecommendationSummaryRepository.QueryAsync(
            new ExecutiveRecommendationSummaryQueryParameters
            {
                CompanyId = companyId,
                GeneratedFrom = parameters.From,
                GeneratedTo = parameters.To
            },
            cancellationToken);

        return new EnterpriseDashboardAiResponse
        {
            AIRecommendationCount = aiRecommendations.Count,
            AverageConfidenceScore = aiRecommendations.Count > 0
                ? decimal.Round(aiRecommendations.Average(item => item.ConfidenceScore), 2, MidpointRounding.AwayFromZero)
                : 0m,
            ExplainabilityCount = explainabilities.Count,
            ExecutiveSummaryCount = executiveSummaries.Count,
            AverageExecutiveSummaryConfidenceLevel = executiveSummaries.Count > 0
                ? decimal.Round(executiveSummaries.Average(item => item.ConfidenceLevel), 2, MidpointRounding.AwayFromZero)
                : 0m,
            DrillDownPath = $"/ai-recommendations?companyId={companyId}"
        };
    }

    public async Task<EnterpriseDashboardAuditResponse> BuildAuditAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var events = await _auditEventRepository.QueryAsync(
            new AuditEventQueryParameters
            {
                CompanyId = companyId,
                OccurredFrom = parameters.From,
                OccurredTo = parameters.To
            },
            cancellationToken);

        return new EnterpriseDashboardAuditResponse
        {
            EventCount = events.Count,
            EventTypeBreakdown = BuildStatusBreakdown(events.Select(auditEvent => auditEvent.EventType)),
            EntityTypeBreakdown = BuildStatusBreakdown(events.Select(auditEvent => auditEvent.EntityType)),
            LastEventAt = events.Count > 0 ? events.Max(auditEvent => auditEvent.OccurredAt) : null,
            DrillDownPath = $"/audit?companyId={companyId}"
        };
    }

    private static IReadOnlyList<StatusCountItem> BuildStatusBreakdown(IEnumerable<string> values)
    {
        return values
            .GroupBy(value => value, StringComparer.OrdinalIgnoreCase)
            .Select(group => new StatusCountItem { Status = group.Key, Count = group.Count() })
            .OrderBy(item => item.Status, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static decimal? TryReadDecimal(string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty(propertyName, out var property)
                && property.TryGetDecimal(out var value))
            {
                return value;
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }
}
