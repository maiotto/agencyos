using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;

namespace AgencyOS.Application.Services;

/// <summary>
/// Read-only Enterprise Dashboard orchestration (US-403 / BR-2101..BR-2110).
/// Resolves the active Company (BR-2104), defaults the reporting window (BR-2105),
/// delegates section aggregation, and fills in period-over-period trend indicators (BR-2106).
/// </summary>
public class EnterpriseDashboardService : IEnterpriseDashboardService
{
    private const int DefaultWindowDays = 30;

    private readonly IDashboardAggregationService _aggregationService;
    private readonly IDashboardTrendService _trendService;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICompanyDecisionProfileRepository _companyDecisionProfileRepository;
    private readonly ICompanyContext _companyContext;

    public EnterpriseDashboardService(
        IDashboardAggregationService aggregationService,
        IDashboardTrendService trendService,
        ICompanyRepository companyRepository,
        ICompanyDecisionProfileRepository companyDecisionProfileRepository,
        ICompanyContext companyContext)
    {
        _aggregationService = aggregationService;
        _trendService = trendService;
        _companyRepository = companyRepository;
        _companyDecisionProfileRepository = companyDecisionProfileRepository;
        _companyContext = companyContext;
    }

    public async Task<EnterpriseDashboardResponse> GetDashboardAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);

        // Sections must run sequentially: all aggregation paths share the same scoped
        // DbContext via repositories, and EF Core does not allow concurrent operations
        // on one context instance.
        var summary = await BuildSummaryAsync(companyId, parameters, cancellationToken);
        var planning = await _aggregationService.BuildPlanningAsync(companyId, parameters, cancellationToken);
        var portfolio = await _aggregationService.BuildPortfolioAsync(companyId, parameters, cancellationToken);
        var capacity = await BuildCapacityAsync(companyId, parameters, cancellationToken);
        var workload = await BuildWorkloadAsync(companyId, parameters, cancellationToken);
        var recommendations = await BuildRecommendationsAsync(companyId, parameters, cancellationToken);
        var decisions = await BuildDecisionsAsync(companyId, parameters, cancellationToken);
        var ai = await BuildAiAsync(companyId, parameters, cancellationToken);
        var audit = await BuildAuditAsync(companyId, parameters, cancellationToken);

        var (from, to) = ResolveDateWindow(parameters);
        var (periodStart, periodEnd) = ResolvePeriodWindow(parameters);

        return new EnterpriseDashboardResponse
        {
            CompanyId = companyId,
            GeneratedAt = DateTimeOffset.UtcNow,
            From = from,
            To = to,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Summary = summary,
            Planning = planning,
            Portfolio = portfolio,
            Capacity = capacity,
            Workload = workload,
            Recommendations = recommendations,
            Decisions = decisions,
            Ai = ai,
            Audit = audit
        };
    }

    public async Task<EnterpriseDashboardSummaryResponse> GetSummaryAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        return await BuildSummaryAsync(companyId, parameters, cancellationToken);
    }

    public async Task<EnterpriseDashboardPlanningResponse> GetPlanningAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        return await _aggregationService.BuildPlanningAsync(companyId, parameters, cancellationToken);
    }

    public async Task<EnterpriseDashboardPortfolioResponse> GetPortfolioAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        return await _aggregationService.BuildPortfolioAsync(companyId, parameters, cancellationToken);
    }

    public async Task<EnterpriseDashboardCapacityResponse> GetCapacityAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        return await BuildCapacityAsync(companyId, parameters, cancellationToken);
    }

    public async Task<EnterpriseDashboardWorkloadResponse> GetWorkloadAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        return await BuildWorkloadAsync(companyId, parameters, cancellationToken);
    }

    public async Task<EnterpriseDashboardRecommendationsResponse> GetRecommendationsAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        return await BuildRecommendationsAsync(companyId, parameters, cancellationToken);
    }

    public async Task<EnterpriseDashboardDecisionsResponse> GetDecisionsAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        return await BuildDecisionsAsync(companyId, parameters, cancellationToken);
    }

    public async Task<EnterpriseDashboardAiResponse> GetAiAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        return await BuildAiAsync(companyId, parameters, cancellationToken);
    }

    public async Task<EnterpriseDashboardAuditResponse> GetAuditAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        return await BuildAuditAsync(companyId, parameters, cancellationToken);
    }

    private async Task<EnterpriseDashboardSummaryResponse> BuildSummaryAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var current = ResolveCurrentParameters(parameters);
        var previous = ResolvePreviousParameters(current);

        var currentResult = await _aggregationService.BuildSummaryAsync(companyId, current, cancellationToken);
        var previousResult = await _aggregationService.BuildSummaryAsync(companyId, previous, cancellationToken);

        currentResult.RecommendationTrend = _trendService.Calculate(
            currentResult.RecommendationCount,
            previousResult.RecommendationCount);
        currentResult.DecisionTrend = _trendService.Calculate(
            currentResult.DecisionCount,
            previousResult.DecisionCount);

        var defaultProfile = await _companyDecisionProfileRepository.GetDefaultActiveAsync(
            companyId,
            cancellationToken);
        currentResult.DefaultDecisionProfileName = defaultProfile?.Name;

        return currentResult;
    }

    private async Task<EnterpriseDashboardCapacityResponse> BuildCapacityAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var current = ResolveCurrentParameters(parameters);
        var previous = ResolvePreviousParameters(current);

        var currentResult = await _aggregationService.BuildCapacityAsync(companyId, current, cancellationToken);
        var previousResult = await _aggregationService.BuildCapacityAsync(companyId, previous, cancellationToken);

        currentResult.UtilizationTrend = _trendService.Calculate(
            currentResult.AverageUtilizationPercentage,
            previousResult.AverageUtilizationPercentage);

        return currentResult;
    }

    private async Task<EnterpriseDashboardWorkloadResponse> BuildWorkloadAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var current = ResolveCurrentParameters(parameters);
        var previous = ResolvePreviousParameters(current);

        var currentResult = await _aggregationService.BuildWorkloadAsync(companyId, current, cancellationToken);
        var previousResult = await _aggregationService.BuildWorkloadAsync(companyId, previous, cancellationToken);

        currentResult.WorkloadTrend = _trendService.Calculate(
            currentResult.AverageWorkloadPercentage,
            previousResult.AverageWorkloadPercentage);

        return currentResult;
    }

    private async Task<EnterpriseDashboardRecommendationsResponse> BuildRecommendationsAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var current = ResolveCurrentParameters(parameters);
        var previous = ResolvePreviousParameters(current);

        var currentResult = await _aggregationService.BuildRecommendationsAsync(companyId, current, cancellationToken);
        var previousResult = await _aggregationService.BuildRecommendationsAsync(companyId, previous, cancellationToken);

        currentResult.Trend = _trendService.Calculate(currentResult.TotalCount, previousResult.TotalCount);

        return currentResult;
    }

    private async Task<EnterpriseDashboardDecisionsResponse> BuildDecisionsAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var current = ResolveCurrentParameters(parameters);
        var previous = ResolvePreviousParameters(current);

        var currentResult = await _aggregationService.BuildDecisionsAsync(companyId, current, cancellationToken);
        var previousResult = await _aggregationService.BuildDecisionsAsync(companyId, previous, cancellationToken);

        currentResult.Trend = _trendService.Calculate(currentResult.TotalCount, previousResult.TotalCount);

        return currentResult;
    }

    private async Task<EnterpriseDashboardAiResponse> BuildAiAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var current = ResolveCurrentParameters(parameters);
        var previous = ResolvePreviousParameters(current);

        var currentResult = await _aggregationService.BuildAiAsync(companyId, current, cancellationToken);
        var previousResult = await _aggregationService.BuildAiAsync(companyId, previous, cancellationToken);

        currentResult.Trend = _trendService.Calculate(
            currentResult.AIRecommendationCount,
            previousResult.AIRecommendationCount);

        return currentResult;
    }

    private async Task<EnterpriseDashboardAuditResponse> BuildAuditAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var current = ResolveCurrentParameters(parameters);
        var previous = ResolvePreviousParameters(current);

        var currentResult = await _aggregationService.BuildAuditAsync(companyId, current, cancellationToken);
        var previousResult = await _aggregationService.BuildAuditAsync(companyId, previous, cancellationToken);

        currentResult.Trend = _trendService.Calculate(currentResult.EventCount, previousResult.EventCount);

        return currentResult;
    }

    private async Task<Guid> ResolveAndValidateCompanyIdAsync(
        EnterpriseDashboardQueryParameters parameters,
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

    private static EnterpriseDashboardQueryParameters ResolveCurrentParameters(
        EnterpriseDashboardQueryParameters parameters)
    {
        var (from, to) = ResolveDateWindow(parameters);
        var (periodStart, periodEnd) = ResolvePeriodWindow(parameters);

        return new EnterpriseDashboardQueryParameters
        {
            CompanyId = parameters.CompanyId,
            From = from,
            To = to,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd
        };
    }

    private static EnterpriseDashboardQueryParameters ResolvePreviousParameters(
        EnterpriseDashboardQueryParameters current)
    {
        var dateSpan = current.To!.Value - current.From!.Value;
        var previousTo = current.From!.Value;
        var previousFrom = previousTo - dateSpan;

        var periodSpanDays = current.PeriodEnd!.Value.DayNumber - current.PeriodStart!.Value.DayNumber;
        var previousPeriodEnd = current.PeriodStart!.Value.AddDays(-1);
        var previousPeriodStart = previousPeriodEnd.AddDays(-periodSpanDays);

        return new EnterpriseDashboardQueryParameters
        {
            CompanyId = current.CompanyId,
            From = previousFrom,
            To = previousTo,
            PeriodStart = previousPeriodStart,
            PeriodEnd = previousPeriodEnd
        };
    }

    private static (DateTimeOffset From, DateTimeOffset To) ResolveDateWindow(
        EnterpriseDashboardQueryParameters parameters)
    {
        var to = parameters.To ?? DateTimeOffset.UtcNow;
        var from = parameters.From ?? to.AddDays(-DefaultWindowDays);
        return (from, to);
    }

    private static (DateOnly PeriodStart, DateOnly PeriodEnd) ResolvePeriodWindow(
        EnterpriseDashboardQueryParameters parameters)
    {
        var periodEnd = parameters.PeriodEnd ?? DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var periodStart = parameters.PeriodStart ?? periodEnd.AddDays(-DefaultWindowDays);
        return (periodStart, periodEnd);
    }
}
