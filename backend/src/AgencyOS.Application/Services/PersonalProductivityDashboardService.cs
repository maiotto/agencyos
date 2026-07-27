using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

/// <summary>
/// Read-only Personal Productivity Dashboard orchestration (US-507 / BR-3001..BR-3010).
/// Reuses My Work aggregation and timeline; records view usage via Audit (BR-3010);
/// never modifies operational data (BR-3002, BR-3007).
/// </summary>
public class PersonalProductivityDashboardService : IPersonalProductivityDashboardService
{
    private const int DefaultWindowDays = 30;
    private const int DefaultUpcomingDeadlineWindowDays = 14;
    private const string SystemUserId = "system";

    private readonly IPersonalDashboardAggregationService _aggregationService;
    private readonly IPersonalTimelineService _timelineService;
    private readonly IPersonalKpiService _kpiService;
    private readonly IPersonalMetricsService _metricsService;
    private readonly IPersonalTrendService _trendService;
    private readonly IActivitySummaryService _activitySummaryService;
    private readonly IDecisionService _decisionService;
    private readonly ICompanyRepository _companyRepository;
    private readonly IExecutionResourceRepository _executionResourceRepository;
    private readonly ICompanyContext _companyContext;
    private readonly IAuditContext _auditContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<PersonalProductivityDashboardService> _logger;

    public PersonalProductivityDashboardService(
        IPersonalDashboardAggregationService aggregationService,
        IPersonalTimelineService timelineService,
        IPersonalKpiService kpiService,
        IPersonalMetricsService metricsService,
        IPersonalTrendService trendService,
        IActivitySummaryService activitySummaryService,
        IDecisionService decisionService,
        ICompanyRepository companyRepository,
        IExecutionResourceRepository executionResourceRepository,
        ICompanyContext companyContext,
        IAuditContext auditContext,
        IAuditService auditService,
        ILogger<PersonalProductivityDashboardService> logger)
    {
        _aggregationService = aggregationService;
        _timelineService = timelineService;
        _kpiService = kpiService;
        _metricsService = metricsService;
        _trendService = trendService;
        _activitySummaryService = activitySummaryService;
        _decisionService = decisionService;
        _companyRepository = companyRepository;
        _executionResourceRepository = executionResourceRepository;
        _companyContext = companyContext;
        _auditContext = auditContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<PersonalProductivityDashboardResponse> GetDashboardAsync(
        PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await LoadSnapshotAsync(parameters, includePreviousPeriod: true, cancellationToken);
        await RecordUsageSafeAsync(snapshot.Identity, "PersonalProductivityDashboard.View", cancellationToken);

        return new PersonalProductivityDashboardResponse
        {
            CompanyId = snapshot.Identity.CompanyId,
            UserId = snapshot.Identity.UserId,
            ExecutionResourceId = snapshot.Identity.ExecutionResourceId,
            GeneratedAt = DateTimeOffset.UtcNow,
            From = snapshot.From,
            To = snapshot.To,
            PeriodStart = snapshot.PeriodStart,
            PeriodEnd = snapshot.PeriodEnd,
            Summary = BuildSummary(snapshot),
            Kpis = snapshot.Kpis,
            Trends = snapshot.Trends,
            Capacity = snapshot.Capacity,
            Workload = snapshot.Workload,
            Activity = snapshot.Activity,
            Performance = snapshot.Performance,
            Statistics = snapshot.Statistics
        };
    }

    public async Task<PersonalProductivitySummaryResponse> GetSummaryAsync(
        PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await LoadSnapshotAsync(parameters, includePreviousPeriod: false, cancellationToken);
        await RecordUsageSafeAsync(snapshot.Identity, "PersonalProductivityDashboard.Summary", cancellationToken);
        return BuildSummary(snapshot);
    }

    public async Task<PersonalProductivityKpiResponse> GetKpisAsync(
        PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await LoadSnapshotAsync(parameters, includePreviousPeriod: false, cancellationToken);
        await RecordUsageSafeAsync(snapshot.Identity, "PersonalProductivityDashboard.Kpis", cancellationToken);
        return snapshot.Kpis;
    }

    public async Task<PersonalProductivityTrendsResponse> GetTrendsAsync(
        PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await LoadSnapshotAsync(parameters, includePreviousPeriod: true, cancellationToken);
        await RecordUsageSafeAsync(snapshot.Identity, "PersonalProductivityDashboard.Trends", cancellationToken);
        return snapshot.Trends;
    }

    public async Task<MyWorkCapacitySummaryResponse> GetCapacityAsync(
        PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await LoadSnapshotAsync(parameters, includePreviousPeriod: false, cancellationToken);
        await RecordUsageSafeAsync(snapshot.Identity, "PersonalProductivityDashboard.Capacity", cancellationToken);
        return snapshot.Capacity;
    }

    public async Task<MyWorkWorkloadSummaryResponse> GetWorkloadAsync(
        PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await LoadSnapshotAsync(parameters, includePreviousPeriod: false, cancellationToken);
        await RecordUsageSafeAsync(snapshot.Identity, "PersonalProductivityDashboard.Workload", cancellationToken);
        return snapshot.Workload;
    }

    public async Task<PersonalProductivityActivitySummaryResponse> GetActivityAsync(
        PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await LoadSnapshotAsync(parameters, includePreviousPeriod: false, cancellationToken);
        await RecordUsageSafeAsync(snapshot.Identity, "PersonalProductivityDashboard.Activity", cancellationToken);
        return snapshot.Activity;
    }

    public async Task<PersonalProductivityStatisticsResponse> GetStatisticsAsync(
        PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await LoadSnapshotAsync(parameters, includePreviousPeriod: false, cancellationToken);
        await RecordUsageSafeAsync(snapshot.Identity, "PersonalProductivityDashboard.Statistics", cancellationToken);
        return snapshot.Statistics;
    }

    private async Task<DashboardSnapshot> LoadSnapshotAsync(
        PersonalProductivityDashboardQueryParameters parameters,
        bool includePreviousPeriod,
        CancellationToken cancellationToken)
    {
        var identity = await ResolveIdentityAsync(parameters, cancellationToken);
        var (from, to) = ResolveDateWindow(parameters);
        var (periodStart, periodEnd) = ResolvePeriodWindow(parameters);
        var (previousStart, previousEnd) = ResolvePreviousPeriod(periodStart, periodEnd);
        var previousFrom = from.AddDays(-(to - from).TotalDays);
        var previousTo = from.AddTicks(-1);

        var missionsTask = _aggregationService.GetMissionsAsync(identity.ExecutionResourceId, cancellationToken);
        var tasksTask = _aggregationService.GetTasksAsync(identity.ExecutionResourceId, cancellationToken);
        var recommendationsTask = _aggregationService.GetRecommendationsAsync(
            identity.CompanyId,
            identity.UserId,
            cancellationToken);
        var decisionsTask = _aggregationService.GetDecisionsAsync(
            identity.CompanyId,
            identity.UserId,
            cancellationToken);
        var capacityTask = _aggregationService.GetCapacityAsync(
            identity.ExecutionResourceId,
            periodStart,
            periodEnd,
            cancellationToken);
        var workloadTask = _aggregationService.GetWorkloadAsync(
            identity.ExecutionResourceId,
            periodStart,
            periodEnd,
            cancellationToken);
        var activityTask = _timelineService.GetTimelineAsync(
            identity.UserId,
            identity.CompanyId,
            from,
            to,
            cancellationToken);
        var completedTask = LoadCompletedDecisionsAsync(identity, from, to, cancellationToken);

        Task<MyWorkCapacitySummaryResponse>? previousCapacityTask = null;
        Task<MyWorkWorkloadSummaryResponse>? previousWorkloadTask = null;
        Task<IReadOnlyList<MyWorkActivityItemResponse>>? previousActivityTask = null;
        Task<IReadOnlyList<PersonalProductivityActivityItemResponse>>? previousCompletedTask = null;

        if (includePreviousPeriod)
        {
            previousCapacityTask = _aggregationService.GetCapacityAsync(
                identity.ExecutionResourceId,
                previousStart,
                previousEnd,
                cancellationToken);
            previousWorkloadTask = _aggregationService.GetWorkloadAsync(
                identity.ExecutionResourceId,
                previousStart,
                previousEnd,
                cancellationToken);
            previousActivityTask = _timelineService.GetTimelineAsync(
                identity.UserId,
                identity.CompanyId,
                previousFrom,
                previousTo,
                cancellationToken);
            previousCompletedTask = LoadCompletedDecisionsAsync(
                identity,
                previousFrom,
                previousTo,
                cancellationToken);
        }

        var awaitables = new List<Task>
        {
            missionsTask,
            tasksTask,
            recommendationsTask,
            decisionsTask,
            capacityTask,
            workloadTask,
            activityTask,
            completedTask
        };

        if (includePreviousPeriod)
        {
            awaitables.Add(previousCapacityTask!);
            awaitables.Add(previousWorkloadTask!);
            awaitables.Add(previousActivityTask!);
            awaitables.Add(previousCompletedTask!);
        }

        await Task.WhenAll(awaitables);

        var tasks = tasksTask.Result;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var myWorkKpis = _kpiService.Calculate(
            missionsTask.Result,
            tasks,
            recommendationsTask.Result,
            decisionsTask.Result,
            capacityTask.Result,
            workloadTask.Result,
            today,
            DefaultUpcomingDeadlineWindowDays);

        var completed = completedTask.Result;
        var activity = activityTask.Result;
        var kpis = _metricsService.BuildKpis(myWorkKpis, completed.Count, activity.Count);
        var performance = _metricsService.BuildPerformance(kpis, tasks);
        var statistics = _metricsService.BuildStatistics(kpis, tasks, capacityTask.Result, workloadTask.Result);
        var activitySummary = _activitySummaryService.Build(
            tasks,
            recommendationsTask.Result,
            decisionsTask.Result,
            completed,
            activity);

        var trends = includePreviousPeriod
            ? _trendService.BuildTrends(
                periodStart,
                periodEnd,
                previousStart,
                previousEnd,
                capacityTask.Result,
                workloadTask.Result,
                previousCapacityTask!.Result,
                previousWorkloadTask!.Result,
                activity.Count,
                previousActivityTask!.Result.Count,
                completed.Count,
                previousCompletedTask!.Result.Count)
            : new PersonalProductivityTrendsResponse
            {
                CurrentPeriodStart = periodStart,
                CurrentPeriodEnd = periodEnd,
                PreviousPeriodStart = previousStart,
                PreviousPeriodEnd = previousEnd
            };

        return new DashboardSnapshot(
            identity,
            from,
            to,
            periodStart,
            periodEnd,
            kpis,
            capacityTask.Result,
            workloadTask.Result,
            activitySummary,
            performance,
            statistics,
            trends);
    }

    private async Task<IReadOnlyList<PersonalProductivityActivityItemResponse>> LoadCompletedDecisionsAsync(
        Identity identity,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var decisions = await _decisionService.FilterAsync(
            new DecisionQueryParameters { CompanyId = identity.CompanyId },
            cancellationToken);

        var completed = decisions
            .Where(decision => DecisionStatus.IsCompleted(decision.DecisionStatus))
            .Where(decision =>
                (!decision.CompletedDate.HasValue && decision.DecisionDate >= from && decision.DecisionDate <= to)
                || (decision.CompletedDate.HasValue
                    && decision.CompletedDate.Value >= from
                    && decision.CompletedDate.Value <= to))
            .ToList();

        if (!string.Equals(identity.UserId, SystemUserId, StringComparison.OrdinalIgnoreCase))
        {
            var own = completed
                .Where(decision => string.Equals(decision.CreatedBy, identity.UserId, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (own.Count > 0)
            {
                completed = own;
            }
        }

        return completed
            .OrderByDescending(decision => decision.CompletedDate ?? decision.DecisionDate)
            .Select(decision => new PersonalProductivityActivityItemResponse
            {
                Id = decision.Id,
                Kind = "Decision",
                Title = $"Decision completed ({decision.ImplementationStatus})",
                Status = decision.DecisionStatus,
                OccurredAt = decision.CompletedDate ?? decision.DecisionDate,
                DrillDownPath = $"/decisions/{decision.Id}"
            })
            .ToList();
    }

    private static PersonalProductivitySummaryResponse BuildSummary(DashboardSnapshot snapshot) =>
        new()
        {
            CompanyId = snapshot.Identity.CompanyId,
            UserId = snapshot.Identity.UserId,
            ExecutionResourceId = snapshot.Identity.ExecutionResourceId,
            GeneratedAt = DateTimeOffset.UtcNow,
            From = snapshot.From,
            To = snapshot.To,
            PeriodStart = snapshot.PeriodStart,
            PeriodEnd = snapshot.PeriodEnd,
            PendingWorkCount = snapshot.Activity.Pending.Count,
            CompletedWorkCount = snapshot.Activity.Completed.Count,
            ActivityEventCount = snapshot.Kpis.ActivityEventCount,
            Kpis = snapshot.Kpis,
            Capacity = snapshot.Capacity,
            Workload = snapshot.Workload
        };

    private async Task RecordUsageSafeAsync(
        Identity identity,
        string action,
        CancellationToken cancellationToken)
    {
        await _auditService.RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.PersonalProductivityDashboard,
                EntityId = identity.CompanyId,
                EventType = AuditEventTypes.Executed,
                Action = action,
                CompanyId = identity.CompanyId,
                UserId = identity.UserId,
                UserName = identity.UserId,
                Source = AuditSources.Api,
                CurrentState = AuditService.SerializeState(new
                {
                    identity.CompanyId,
                    identity.UserId,
                    identity.ExecutionResourceId,
                    Action = action
                }),
                Metadata = AuditService.SerializeState(new { Surface = "PersonalProductivityDashboard" })
            },
            cancellationToken);

        _logger.LogInformation(
            "Personal Productivity Dashboard usage recorded Action={Action} User={UserId} Company={CompanyId}",
            action,
            identity.UserId,
            identity.CompanyId);
    }

    private async Task<Identity> ResolveIdentityAsync(
        PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var companyId = parameters.CompanyId ?? _companyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId;

        var company = await _companyRepository.GetByIdAsync(companyId, cancellationToken);
        if (company is null)
        {
            throw new NotFoundException($"Company with id '{companyId}' was not found.");
        }

        var userId = !string.IsNullOrWhiteSpace(parameters.UserId)
            ? parameters.UserId.Trim()
            : (!string.IsNullOrWhiteSpace(_auditContext.UserId) ? _auditContext.UserId!.Trim() : SystemUserId);

        var executionResourceId = parameters.ExecutionResourceId
            ?? await ResolveExecutionResourceIdByCodeAsync(userId, cancellationToken);

        return new Identity(companyId, userId, executionResourceId);
    }

    private async Task<Guid?> ResolveExecutionResourceIdByCodeAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        if (string.Equals(userId, SystemUserId, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var activeResources = await _executionResourceRepository.GetAllAsync(
            new ExecutionResourceQueryParameters { Status = ExecutionResourceStatus.Active },
            cancellationToken);

        var match = activeResources.FirstOrDefault(resource =>
            string.Equals(resource.Code, userId, StringComparison.OrdinalIgnoreCase));

        return match?.Id;
    }

    private static (DateTimeOffset From, DateTimeOffset To) ResolveDateWindow(
        PersonalProductivityDashboardQueryParameters parameters)
    {
        var to = parameters.To ?? DateTimeOffset.UtcNow;
        var from = parameters.From ?? to.AddDays(-DefaultWindowDays);
        return (from, to);
    }

    private static (DateOnly PeriodStart, DateOnly PeriodEnd) ResolvePeriodWindow(
        PersonalProductivityDashboardQueryParameters parameters)
    {
        var periodEnd = parameters.PeriodEnd ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var periodStart = parameters.PeriodStart ?? periodEnd.AddDays(-(DefaultWindowDays - 1));
        return (periodStart, periodEnd);
    }

    private static (DateOnly PreviousStart, DateOnly PreviousEnd) ResolvePreviousPeriod(
        DateOnly periodStart,
        DateOnly periodEnd)
    {
        var lengthDays = periodEnd.DayNumber - periodStart.DayNumber + 1;
        var previousEnd = periodStart.AddDays(-1);
        var previousStart = previousEnd.AddDays(-(lengthDays - 1));
        return (previousStart, previousEnd);
    }

    private sealed record Identity(Guid CompanyId, string UserId, Guid? ExecutionResourceId);

    private sealed record DashboardSnapshot(
        Identity Identity,
        DateTimeOffset From,
        DateTimeOffset To,
        DateOnly PeriodStart,
        DateOnly PeriodEnd,
        PersonalProductivityKpiResponse Kpis,
        MyWorkCapacitySummaryResponse Capacity,
        MyWorkWorkloadSummaryResponse Workload,
        PersonalProductivityActivitySummaryResponse Activity,
        PersonalProductivityPerformanceIndicatorsResponse Performance,
        PersonalProductivityStatisticsResponse Statistics,
        PersonalProductivityTrendsResponse Trends);
}
