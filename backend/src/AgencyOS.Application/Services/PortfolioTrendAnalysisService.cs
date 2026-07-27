using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;

namespace AgencyOS.Application.Services;

/// <summary>
/// Capacity/Workload/Health/Recommendation/Decision trend series bucketed by calendar month
/// (US-404 / BR-2204..BR-2210). Capacity/Workload figures always come from the immutable
/// <c>CapacityHistory</c>/<c>WorkloadHistory</c> aggregates (BR-2209) and the per-bucket
/// <c>HealthStatus</c> is derived via <see cref="PortfolioHealth.Calculate"/> — the same
/// deterministic threshold logic already used everywhere else (BR-2203).
/// </summary>
public class PortfolioTrendAnalysisService : IPortfolioTrendAnalysisService
{
    private const int MaxBuckets = 60;

    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IRecommendationRepository _recommendationRepository;
    private readonly IDecisionRepository _decisionRepository;
    private readonly ICapacityHistoryRepository _capacityHistoryRepository;
    private readonly IWorkloadHistoryRepository _workloadHistoryRepository;

    public PortfolioTrendAnalysisService(
        IPortfolioRepository portfolioRepository,
        IRecommendationRepository recommendationRepository,
        IDecisionRepository decisionRepository,
        ICapacityHistoryRepository capacityHistoryRepository,
        IWorkloadHistoryRepository workloadHistoryRepository)
    {
        _portfolioRepository = portfolioRepository;
        _recommendationRepository = recommendationRepository;
        _decisionRepository = decisionRepository;
        _capacityHistoryRepository = capacityHistoryRepository;
        _workloadHistoryRepository = workloadHistoryRepository;
    }

    public async Task<PortfolioTrendsResponse> BuildTrendsAsync(
        Guid companyId,
        PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        Portfolio? portfolio = null;
        if (parameters.PortfolioId.HasValue)
        {
            portfolio = await _portfolioRepository.GetByIdAsync(parameters.PortfolioId.Value, cancellationToken);
            if (portfolio is null)
            {
                throw new NotFoundException($"Portfolio with id '{parameters.PortfolioId}' was not found.");
            }

            if (portfolio.CompanyId != companyId)
            {
                throw new BusinessRuleException(
                    $"Portfolio '{parameters.PortfolioId}' does not belong to the resolved Company.");
            }
        }

        var periodStart = parameters.PeriodStart ?? DateOnly.FromDateTime((parameters.From ?? DateTimeOffset.UtcNow.AddDays(-180)).UtcDateTime);
        var periodEnd = parameters.PeriodEnd ?? DateOnly.FromDateTime((parameters.To ?? DateTimeOffset.UtcNow).UtcDateTime);

        var capacityHistory = await _capacityHistoryRepository.QueryAsync(
            new CapacityHistoryQueryParameters
            {
                CompanyId = companyId,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd
            },
            cancellationToken);

        var workloadHistory = await _workloadHistoryRepository.QueryAsync(
            new WorkloadHistoryQueryParameters
            {
                CompanyId = companyId,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd
            },
            cancellationToken);

        var (recommendations, decisions) = await LoadRecommendationsAndDecisionsAsync(
            companyId,
            portfolio,
            parameters,
            cancellationToken);

        var capacityByMonth = capacityHistory.ToLookup(history => MonthKey(history.CalculationDate.UtcDateTime));
        var workloadByMonth = workloadHistory.ToLookup(history => MonthKey(history.CalculationDate.UtcDateTime));
        var recommendationsByMonth = recommendations.ToLookup(recommendation => MonthKey(recommendation.GeneratedAt.UtcDateTime));
        var decisionsByMonth = decisions.ToLookup(decision => MonthKey(decision.DecisionDate.UtcDateTime));

        var buckets = BuildMonthBuckets(periodStart, periodEnd);
        var points = buckets.Select(bucket =>
        {
            var capacityValues = capacityByMonth[bucket.Label].Select(history => history.UtilizationPercentage).ToList();
            var workloadValues = workloadByMonth[bucket.Label].Select(history => history.WorkloadPercentage).ToList();

            decimal? capacityUtilization = capacityValues.Count > 0
                ? decimal.Round(capacityValues.Average(), 2, MidpointRounding.AwayFromZero)
                : null;
            decimal? workloadUtilization = workloadValues.Count > 0
                ? decimal.Round(workloadValues.Average(), 2, MidpointRounding.AwayFromZero)
                : null;

            string? healthStatus = capacityUtilization.HasValue || workloadUtilization.HasValue
                ? PortfolioHealth.Calculate(capacityUtilization ?? 0m, workloadUtilization ?? 0m, null)
                : null;

            return new PortfolioTrendPointResponse
            {
                PeriodLabel = bucket.Label,
                PeriodStart = bucket.Start,
                PeriodEnd = bucket.End,
                CapacityUtilization = capacityUtilization,
                WorkloadUtilization = workloadUtilization,
                HealthStatus = healthStatus,
                RecommendationCount = recommendationsByMonth[bucket.Label].Count(),
                DecisionCount = decisionsByMonth[bucket.Label].Count()
            };
        }).ToList();

        return new PortfolioTrendsResponse
        {
            CompanyId = companyId,
            PortfolioId = portfolio?.Id,
            GeneratedAt = DateTimeOffset.UtcNow,
            From = parameters.From,
            To = parameters.To,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Points = points,
            DrillDownPath = portfolio is not null ? $"/portfolios/{portfolio.Id}" : "/portfolio-analytics"
        };
    }

    private async Task<(IReadOnlyList<Recommendation> Recommendations, IReadOnlyList<Decision> Decisions)>
        LoadRecommendationsAndDecisionsAsync(
            Guid companyId,
            Portfolio? portfolio,
            PortfolioAnalyticsQueryParameters parameters,
            CancellationToken cancellationToken)
    {
        if (portfolio is null)
        {
            var companyRecommendations = await _recommendationRepository.QueryAsync(
                new RecommendationQueryParameters
                {
                    CompanyId = companyId,
                    GeneratedFrom = parameters.From,
                    GeneratedTo = parameters.To,
                    IncludeArchived = true
                },
                cancellationToken);

            var companyDecisions = await _decisionRepository.GetAllAsync(
                new DecisionQueryParameters
                {
                    CompanyId = companyId,
                    DecisionFrom = parameters.From,
                    DecisionTo = parameters.To
                },
                cancellationToken);

            return (companyRecommendations, companyDecisions);
        }

        var recommendations = new List<Recommendation>();
        var decisions = new List<Decision>();

        foreach (var mission in portfolio.Missions)
        {
            var missionRecommendations = await _recommendationRepository.GetByMissionIdAsync(
                mission.MissionId,
                new RecommendationQueryParameters
                {
                    CompanyId = companyId,
                    GeneratedFrom = parameters.From,
                    GeneratedTo = parameters.To,
                    IncludeArchived = true
                },
                cancellationToken);
            recommendations.AddRange(missionRecommendations);

            var missionDecisions = await _decisionRepository.GetAllAsync(
                new DecisionQueryParameters
                {
                    CompanyId = companyId,
                    MissionId = mission.MissionId,
                    DecisionFrom = parameters.From,
                    DecisionTo = parameters.To
                },
                cancellationToken);
            decisions.AddRange(missionDecisions);
        }

        return (recommendations, decisions);
    }

    private static string MonthKey(DateTime value) => value.ToString("yyyy-MM");

    private static string MonthKey(DateOnly value) => $"{value.Year:D4}-{value.Month:D2}";

    private static List<(DateOnly Start, DateOnly End, string Label)> BuildMonthBuckets(
        DateOnly rangeStart,
        DateOnly rangeEnd)
    {
        var buckets = new List<(DateOnly Start, DateOnly End, string Label)>();

        if (rangeEnd < rangeStart)
        {
            (rangeStart, rangeEnd) = (rangeEnd, rangeStart);
        }

        var cursor = new DateOnly(rangeStart.Year, rangeStart.Month, 1);
        var endCursor = new DateOnly(rangeEnd.Year, rangeEnd.Month, 1);

        var iterations = 0;
        while (cursor <= endCursor && iterations < MaxBuckets)
        {
            var monthEnd = cursor.AddMonths(1).AddDays(-1);
            var bucketStart = cursor < rangeStart ? rangeStart : cursor;
            var bucketEnd = monthEnd > rangeEnd ? rangeEnd : monthEnd;

            buckets.Add((bucketStart, bucketEnd, MonthKey(cursor)));

            cursor = cursor.AddMonths(1);
            iterations++;
        }

        return buckets;
    }
}
