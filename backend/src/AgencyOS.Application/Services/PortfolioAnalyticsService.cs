using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;

namespace AgencyOS.Application.Services;

/// <summary>
/// Read-only Portfolio Analytics orchestration (US-404 / BR-2201..BR-2210). Resolves the active
/// Company (BR-2207), builds mission-scoped Recommendation/Decision snapshots per Portfolio from
/// stored snapshot fields and history (BR-2204..BR-2206), and delegates trend/comparison/health/
/// risk computation to the specialized analytics services. Never writes to a Portfolio and never
/// recalculates Capacity/Workload engines (BR-2201, BR-2209).
/// </summary>
public class PortfolioAnalyticsService : IPortfolioAnalyticsService
{
    private const int DefaultWindowDays = 180;

    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IRecommendationRepository _recommendationRepository;
    private readonly IRecommendationHistoryRepository _recommendationHistoryRepository;
    private readonly IDecisionRepository _decisionRepository;
    private readonly ICapacityHistoryService _capacityHistoryService;
    private readonly IWorkloadHistoryService _workloadHistoryService;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICompanyContext _companyContext;
    private readonly IDashboardHealthCalculationService _healthCalculationService;
    private readonly IPortfolioTrendAnalysisService _trendAnalysisService;
    private readonly IPortfolioComparisonService _comparisonService;
    private readonly IPortfolioHealthAnalyticsService _healthAnalyticsService;
    private readonly IPortfolioRiskAnalyticsService _riskAnalyticsService;

    public PortfolioAnalyticsService(
        IPortfolioRepository portfolioRepository,
        IRecommendationRepository recommendationRepository,
        IRecommendationHistoryRepository recommendationHistoryRepository,
        IDecisionRepository decisionRepository,
        ICapacityHistoryService capacityHistoryService,
        IWorkloadHistoryService workloadHistoryService,
        ICompanyRepository companyRepository,
        ICompanyContext companyContext,
        IDashboardHealthCalculationService healthCalculationService,
        IPortfolioTrendAnalysisService trendAnalysisService,
        IPortfolioComparisonService comparisonService,
        IPortfolioHealthAnalyticsService healthAnalyticsService,
        IPortfolioRiskAnalyticsService riskAnalyticsService)
    {
        _portfolioRepository = portfolioRepository;
        _recommendationRepository = recommendationRepository;
        _recommendationHistoryRepository = recommendationHistoryRepository;
        _decisionRepository = decisionRepository;
        _capacityHistoryService = capacityHistoryService;
        _workloadHistoryService = workloadHistoryService;
        _companyRepository = companyRepository;
        _companyContext = companyContext;
        _healthCalculationService = healthCalculationService;
        _trendAnalysisService = trendAnalysisService;
        _comparisonService = comparisonService;
        _healthAnalyticsService = healthAnalyticsService;
        _riskAnalyticsService = riskAnalyticsService;
    }

    public async Task<PortfolioAnalyticsOverviewResponse> GetOverviewAsync(
        PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters.CompanyId, cancellationToken);
        var (portfolios, snapshots) = await BuildSnapshotsAsync(companyId, parameters, cancellationToken);
        var (from, to) = ResolveDateWindow(parameters);

        var cards = snapshots
            .Select(snapshot => PortfolioAnalyticsSnapshotBuilder.ToCard(
                snapshot,
                _healthCalculationService.CalculateOverall([snapshot.PortfolioHealth])))
            .ToList();

        return new PortfolioAnalyticsOverviewResponse
        {
            CompanyId = companyId,
            GeneratedAt = DateTimeOffset.UtcNow,
            From = from,
            To = to,
            PeriodStart = parameters.PeriodStart,
            PeriodEnd = parameters.PeriodEnd,
            PortfolioCount = portfolios.Count,
            OverallHealth = _healthCalculationService.CalculateOverall(
                snapshots.Select(snapshot => snapshot.PortfolioHealth).ToList()),
            Portfolios = cards,
            DrillDownPath = $"/portfolios?companyId={companyId}"
        };
    }

    public async Task<PortfolioAnalyticsDetailResponse> GetDetailAsync(
        Guid portfolioId,
        PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters.CompanyId, cancellationToken);
        var portfolio = await _portfolioRepository.GetByIdAsync(portfolioId, cancellationToken);
        if (portfolio is null)
        {
            throw new NotFoundException($"Portfolio with id '{portfolioId}' was not found.");
        }

        if (portfolio.CompanyId != companyId)
        {
            throw new BusinessRuleException($"Portfolio '{portfolioId}' does not belong to the resolved Company.");
        }

        var (from, to) = ResolveDateWindow(parameters);
        var missionIds = portfolio.Missions.Select(mission => mission.MissionId).ToList();

        var recommendations = new List<Recommendation>();
        var decisions = new List<Decision>();

        foreach (var missionId in missionIds)
        {
            var missionRecommendations = await _recommendationRepository.GetByMissionIdAsync(
                missionId,
                new RecommendationQueryParameters
                {
                    CompanyId = companyId,
                    GeneratedFrom = from,
                    GeneratedTo = to,
                    IncludeArchived = true
                },
                cancellationToken);
            recommendations.AddRange(missionRecommendations);

            var missionDecisions = await _decisionRepository.GetAllAsync(
                new DecisionQueryParameters
                {
                    CompanyId = companyId,
                    MissionId = missionId,
                    DecisionFrom = from,
                    DecisionTo = to
                },
                cancellationToken);
            decisions.AddRange(missionDecisions);
        }

        var snapshot = PortfolioAnalyticsSnapshotBuilder.BuildFrom(portfolio, missionIds, recommendations, decisions);

        var recommendationIds = recommendations.Select(recommendation => recommendation.Id).ToHashSet();
        var companyRecommendationHistory = await _recommendationHistoryRepository.QueryAsync(
            new RecommendationHistoryQueryParameters
            {
                CompanyId = companyId,
                CreatedFrom = from,
                CreatedTo = to
            },
            cancellationToken);
        var recommendationHistoryCount = companyRecommendationHistory
            .Count(history => recommendationIds.Contains(history.RecommendationId));

        var historicalCapacity = await _capacityHistoryService.AggregateAsync(
            new CapacityHistoryQueryParameters
            {
                CompanyId = companyId,
                PeriodStart = portfolio.PlanningPeriodStart,
                PeriodEnd = portfolio.PlanningPeriodEnd
            },
            cancellationToken);
        var historicalWorkload = await _workloadHistoryService.AggregateAsync(
            new WorkloadHistoryQueryParameters
            {
                CompanyId = companyId,
                PeriodStart = portfolio.PlanningPeriodStart,
                PeriodEnd = portfolio.PlanningPeriodEnd
            },
            cancellationToken);

        return new PortfolioAnalyticsDetailResponse
        {
            PortfolioId = portfolio.Id,
            CompanyId = companyId,
            Name = portfolio.Name,
            Description = portfolio.Description,
            Status = portfolio.Status,
            PlanningPeriodStart = portfolio.PlanningPeriodStart,
            PlanningPeriodEnd = portfolio.PlanningPeriodEnd,
            Health = _healthCalculationService.CalculateOverall([portfolio.PortfolioHealth]),
            MissionCount = snapshot.MissionCount,
            MissionIds = missionIds,
            UtilizationPercentage = snapshot.UtilizationPercentage,
            WorkloadPercentage = snapshot.WorkloadPercentage,
            HistoricalAverageUtilizationPercentage = historicalCapacity.RecordCount > 0
                ? historicalCapacity.AverageUtilizationPercentage
                : null,
            HistoricalAverageWorkloadPercentage = historicalWorkload.RecordCount > 0
                ? historicalWorkload.AverageWorkloadPercentage
                : null,
            HistoricalCapacityRecordCount = historicalCapacity.RecordCount,
            HistoricalWorkloadRecordCount = historicalWorkload.RecordCount,
            RecommendationCount = snapshot.RecommendationCount,
            RecommendationStatusBreakdown = BuildStatusBreakdown(
                recommendations.Select(recommendation => recommendation.Status)),
            AverageRecommendationScore = snapshot.AverageRecommendationScore,
            RecommendationHistoryCount = recommendationHistoryCount,
            DecisionCount = snapshot.DecisionCount,
            DecisionStatusBreakdown = BuildStatusBreakdown(decisions.Select(decision => decision.DecisionStatus)),
            CompletedDecisionCount = snapshot.CompletedDecisionCount,
            CancelledDecisionCount = snapshot.CancelledDecisionCount,
            DrillDownPath = $"/portfolios/{portfolio.Id}"
        };
    }

    public async Task<PortfolioTrendsResponse> GetTrendsAsync(
        PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters.CompanyId, cancellationToken);
        return await _trendAnalysisService.BuildTrendsAsync(companyId, parameters, cancellationToken);
    }

    public async Task<PortfolioComparisonResponse> GetComparisonAsync(
        PortfolioCompareQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters.CompanyId, cancellationToken);
        return await _comparisonService.CompareAsync(companyId, parameters, cancellationToken);
    }

    public async Task<PortfolioRankingResponse> GetRankingAsync(
        PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters.CompanyId, cancellationToken);
        var (_, snapshots) = await BuildSnapshotsAsync(companyId, parameters, cancellationToken);

        return new PortfolioRankingResponse
        {
            CompanyId = companyId,
            GeneratedAt = DateTimeOffset.UtcNow,
            Items = BuildRanking(snapshots)
        };
    }

    public async Task<PortfolioHealthAnalyticsResponse> GetHealthAnalyticsAsync(
        PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters.CompanyId, cancellationToken);
        var (_, snapshots) = await BuildSnapshotsAsync(companyId, parameters, cancellationToken);
        var response = _healthAnalyticsService.BuildHealthAnalytics(companyId, snapshots);
        response.RiskIndicators = MergeRiskIndicators(response.RiskIndicators, _riskAnalyticsService.BuildRiskIndicators(snapshots));
        return response;
    }

    public async Task<PortfolioPerformanceResponse> GetPerformanceAsync(
        PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters.CompanyId, cancellationToken);
        var (_, snapshots) = await BuildSnapshotsAsync(companyId, parameters, cancellationToken);

        var utilizationValues = snapshots
            .Where(snapshot => snapshot.UtilizationPercentage.HasValue)
            .Select(snapshot => snapshot.UtilizationPercentage!.Value)
            .ToList();
        var workloadValues = snapshots
            .Where(snapshot => snapshot.WorkloadPercentage.HasValue)
            .Select(snapshot => snapshot.WorkloadPercentage!.Value)
            .ToList();
        var scoredSnapshots = snapshots
            .Where(snapshot => snapshot.AverageRecommendationScore.HasValue)
            .ToList();

        var totalRecommendations = snapshots.Sum(snapshot => snapshot.RecommendationCount);
        var totalDecisions = snapshots.Sum(snapshot => snapshot.DecisionCount);
        var totalCompleted = snapshots.Sum(snapshot => snapshot.CompletedDecisionCount);
        var totalCancelled = snapshots.Sum(snapshot => snapshot.CancelledDecisionCount);

        return new PortfolioPerformanceResponse
        {
            CompanyId = companyId,
            GeneratedAt = DateTimeOffset.UtcNow,
            PortfolioCount = snapshots.Count,
            AverageUtilizationPercentage = utilizationValues.Count > 0
                ? Round(utilizationValues.Average())
                : 0m,
            AverageWorkloadPercentage = workloadValues.Count > 0
                ? Round(workloadValues.Average())
                : 0m,
            AverageRecommendationScore = scoredSnapshots.Count > 0
                ? Round(scoredSnapshots.Average(snapshot => snapshot.AverageRecommendationScore!.Value))
                : null,
            RecommendationCount = totalRecommendations,
            DecisionCount = totalDecisions,
            DecisionCompletionRatePercentage = totalDecisions > 0
                ? Round((decimal)totalCompleted / totalDecisions * 100m)
                : 0m,
            DecisionCancellationRatePercentage = totalDecisions > 0
                ? Round((decimal)totalCancelled / totalDecisions * 100m)
                : 0m,
            RecommendationEffectivenessPercentage = totalDecisions > 0
                ? Round((decimal)totalCompleted / totalDecisions * 100m)
                : 0m,
            DrillDownPath = $"/portfolios?companyId={companyId}"
        };
    }

    private async Task<(IReadOnlyList<Portfolio> Portfolios, IReadOnlyList<PortfolioAnalyticsSnapshot> Snapshots)>
        BuildSnapshotsAsync(
            Guid companyId,
            PortfolioAnalyticsQueryParameters parameters,
            CancellationToken cancellationToken)
    {
        var portfolios = await _portfolioRepository.GetAllAsync(
            new PortfolioQueryParameters
            {
                CompanyId = companyId,
                PeriodStart = parameters.PeriodStart,
                PeriodEnd = parameters.PeriodEnd
            },
            cancellationToken);

        var (from, to) = ResolveDateWindow(parameters);

        var recommendations = await _recommendationRepository.QueryAsync(
            new RecommendationQueryParameters
            {
                CompanyId = companyId,
                GeneratedFrom = from,
                GeneratedTo = to,
                IncludeArchived = true
            },
            cancellationToken);

        var decisions = await _decisionRepository.GetAllAsync(
            new DecisionQueryParameters
            {
                CompanyId = companyId,
                DecisionFrom = from,
                DecisionTo = to
            },
            cancellationToken);

        var snapshots = portfolios
            .Select(portfolio => PortfolioAnalyticsSnapshotBuilder.Build(portfolio, recommendations, decisions))
            .ToList();

        return (portfolios, snapshots);
    }

    private IReadOnlyList<PortfolioRankingItemResponse> BuildRanking(
        IReadOnlyList<PortfolioAnalyticsSnapshot> snapshots)
    {
        var ranked = snapshots
            .Select(snapshot =>
            {
                var healthScore = HealthScore(snapshot.PortfolioHealth);
                var utilizationScore = UtilizationScore(snapshot.UtilizationPercentage ?? snapshot.WorkloadPercentage);
                decimal? effectiveness = snapshot.DecisionCount > 0
                    ? Round((decimal)snapshot.CompletedDecisionCount / snapshot.DecisionCount * 100m)
                    : null;
                var effectivenessScore = effectiveness ?? 50m;
                var score = Round((0.4m * healthScore) + (0.35m * utilizationScore) + (0.25m * effectivenessScore));

                return new
                {
                    Snapshot = snapshot,
                    Score = score,
                    Effectiveness = effectiveness,
                    Health = _healthCalculationService.CalculateOverall([snapshot.PortfolioHealth])
                };
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Snapshot.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return ranked
            .Select((item, index) => new PortfolioRankingItemResponse
            {
                Rank = index + 1,
                PortfolioId = item.Snapshot.PortfolioId,
                Name = item.Snapshot.Name,
                Score = item.Score,
                Health = item.Health,
                UtilizationPercentage = item.Snapshot.UtilizationPercentage,
                WorkloadPercentage = item.Snapshot.WorkloadPercentage,
                RecommendationEffectivenessPercentage = item.Effectiveness,
                DrillDownPath = item.Snapshot.DrillDownPath
            })
            .ToList();
    }

    private static IReadOnlyList<PortfolioRiskIndicatorResponse> MergeRiskIndicators(
        IReadOnlyList<PortfolioRiskIndicatorResponse> healthBased,
        IReadOnlyList<PortfolioRiskIndicatorResponse> riskBased)
    {
        var merged = new Dictionary<Guid, PortfolioRiskIndicatorResponse>();
        foreach (var indicator in healthBased)
        {
            merged[indicator.PortfolioId] = indicator;
        }

        foreach (var indicator in riskBased)
        {
            merged[indicator.PortfolioId] = indicator;
        }

        return merged.Values
            .OrderByDescending(indicator => RiskLevelWeight(indicator.RiskLevel))
            .ThenBy(indicator => indicator.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static int RiskLevelWeight(string riskLevel) => riskLevel switch
    {
        "Critical" or "Overloaded" => 3,
        "High" or "AtRisk" => 2,
        "Medium" => 1,
        _ => 0
    };

    private static decimal HealthScore(string health) => PortfolioHealth.Canonicalize(health) switch
    {
        PortfolioHealth.Healthy => 100m,
        PortfolioHealth.Underutilized => 60m,
        PortfolioHealth.AtRisk => 30m,
        PortfolioHealth.Overloaded => 0m,
        _ => 50m
    };

    private static decimal UtilizationScore(decimal? utilization)
    {
        if (!utilization.HasValue)
        {
            return 50m;
        }

        var value = utilization.Value;
        return value <= 100m ? value : Math.Max(0m, 200m - value);
    }

    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    private static IReadOnlyList<StatusCountItem> BuildStatusBreakdown(IEnumerable<string> values) =>
        values
            .GroupBy(value => value, StringComparer.OrdinalIgnoreCase)
            .Select(group => new StatusCountItem { Status = group.Key, Count = group.Count() })
            .OrderBy(item => item.Status, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private async Task<Guid> ResolveAndValidateCompanyIdAsync(Guid? companyId, CancellationToken cancellationToken)
    {
        var resolvedCompanyId = companyId ?? _companyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId;

        var company = await _companyRepository.GetByIdAsync(resolvedCompanyId, cancellationToken);
        if (company is null)
        {
            throw new NotFoundException($"Company with id '{resolvedCompanyId}' was not found.");
        }

        return resolvedCompanyId;
    }

    private static (DateTimeOffset From, DateTimeOffset To) ResolveDateWindow(
        PortfolioAnalyticsQueryParameters parameters)
    {
        var to = parameters.To ?? DateTimeOffset.UtcNow;
        var from = parameters.From ?? to.AddDays(-DefaultWindowDays);
        return (from, to);
    }
}
