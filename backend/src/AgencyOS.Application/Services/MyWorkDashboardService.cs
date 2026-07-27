using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;

namespace AgencyOS.Application.Services;

/// <summary>
/// Read-only My Work Dashboard orchestration (US-501 / BR-2401..BR-2410). Resolves the caller's
/// Company/User/ExecutionResource identity per DEC-501-001, delegates section aggregation to
/// <see cref="IPersonalDashboardAggregationService"/> and <see cref="IPersonalTimelineService"/>,
/// and computes KPIs via <see cref="IPersonalKpiService"/>. Never writes to any repository.
/// </summary>
public class MyWorkDashboardService : IMyWorkDashboardService
{
    private const int DefaultWindowDays = 30;
    private const int DefaultUpcomingDeadlineWindowDays = 14;
    private const string SystemUserId = "system";

    private readonly IPersonalDashboardAggregationService _aggregationService;
    private readonly IPersonalTimelineService _timelineService;
    private readonly IPersonalKpiService _kpiService;
    private readonly ICompanyRepository _companyRepository;
    private readonly IExecutionResourceRepository _executionResourceRepository;
    private readonly ICompanyContext _companyContext;
    private readonly IAuditContext _auditContext;

    public MyWorkDashboardService(
        IPersonalDashboardAggregationService aggregationService,
        IPersonalTimelineService timelineService,
        IPersonalKpiService kpiService,
        ICompanyRepository companyRepository,
        IExecutionResourceRepository executionResourceRepository,
        ICompanyContext companyContext,
        IAuditContext auditContext)
    {
        _aggregationService = aggregationService;
        _timelineService = timelineService;
        _kpiService = kpiService;
        _companyRepository = companyRepository;
        _executionResourceRepository = executionResourceRepository;
        _companyContext = companyContext;
        _auditContext = auditContext;
    }

    public async Task<MyWorkDashboardResponse> GetDashboardAsync(
        MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var identity = await ResolveIdentityAsync(parameters, cancellationToken);
        var (from, to) = ResolveDateWindow(parameters);
        var (periodStart, periodEnd) = ResolvePeriodWindow(parameters);

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

        await Task.WhenAll(
            missionsTask,
            tasksTask,
            recommendationsTask,
            decisionsTask,
            capacityTask,
            workloadTask,
            activityTask);

        var tasks = tasksTask.Result;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var kpis = _kpiService.Calculate(
            missionsTask.Result,
            tasks,
            recommendationsTask.Result,
            decisionsTask.Result,
            capacityTask.Result,
            workloadTask.Result,
            today,
            DefaultUpcomingDeadlineWindowDays);
        var (overdue, upcoming) = BuildDeadlines(tasks, today);

        return new MyWorkDashboardResponse
        {
            CompanyId = identity.CompanyId,
            UserId = identity.UserId,
            ExecutionResourceId = identity.ExecutionResourceId,
            GeneratedAt = DateTimeOffset.UtcNow,
            From = from,
            To = to,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Kpis = kpis,
            Capacity = capacityTask.Result,
            Workload = workloadTask.Result,
            Missions = missionsTask.Result,
            Tasks = tasks,
            Recommendations = recommendationsTask.Result,
            Decisions = decisionsTask.Result,
            OverdueTasks = overdue,
            UpcomingDeadlines = upcoming,
            Activity = activityTask.Result
        };
    }

    public async Task<MyWorkSummaryResponse> GetSummaryAsync(
        MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var identity = await ResolveIdentityAsync(parameters, cancellationToken);
        var (periodStart, periodEnd) = ResolvePeriodWindow(parameters);

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

        await Task.WhenAll(missionsTask, tasksTask, recommendationsTask, decisionsTask, capacityTask, workloadTask);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var kpis = _kpiService.Calculate(
            missionsTask.Result,
            tasksTask.Result,
            recommendationsTask.Result,
            decisionsTask.Result,
            capacityTask.Result,
            workloadTask.Result,
            today,
            DefaultUpcomingDeadlineWindowDays);

        return new MyWorkSummaryResponse
        {
            CompanyId = identity.CompanyId,
            UserId = identity.UserId,
            ExecutionResourceId = identity.ExecutionResourceId,
            GeneratedAt = DateTimeOffset.UtcNow,
            MissionCount = missionsTask.Result.Count,
            TaskCount = tasksTask.Result.Count,
            RecommendationCount = recommendationsTask.Result.Count,
            DecisionCount = decisionsTask.Result.Count,
            Kpis = kpis,
            Capacity = capacityTask.Result,
            Workload = workloadTask.Result
        };
    }

    public async Task<MyWorkTasksResponse> GetTasksAsync(
        MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var identity = await ResolveIdentityAsync(parameters, cancellationToken);
        var tasks = await _aggregationService.GetTasksAsync(identity.ExecutionResourceId, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var (overdue, upcoming) = BuildDeadlines(tasks, today);

        return new MyWorkTasksResponse
        {
            Tasks = tasks,
            OverdueTasks = overdue,
            UpcomingDeadlines = upcoming
        };
    }

    public async Task<MyWorkMissionsResponse> GetMissionsAsync(
        MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var identity = await ResolveIdentityAsync(parameters, cancellationToken);
        var missions = await _aggregationService.GetMissionsAsync(identity.ExecutionResourceId, cancellationToken);
        return new MyWorkMissionsResponse { Missions = missions };
    }

    public async Task<MyWorkRecommendationsResponse> GetRecommendationsAsync(
        MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var identity = await ResolveIdentityAsync(parameters, cancellationToken);
        var recommendations = await _aggregationService.GetRecommendationsAsync(
            identity.CompanyId,
            identity.UserId,
            cancellationToken);
        return new MyWorkRecommendationsResponse { Recommendations = recommendations };
    }

    public async Task<MyWorkDecisionsResponse> GetDecisionsAsync(
        MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var identity = await ResolveIdentityAsync(parameters, cancellationToken);
        var decisions = await _aggregationService.GetDecisionsAsync(
            identity.CompanyId,
            identity.UserId,
            cancellationToken);
        return new MyWorkDecisionsResponse { Decisions = decisions };
    }

    public async Task<MyWorkActivityResponse> GetActivityAsync(
        MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var identity = await ResolveIdentityAsync(parameters, cancellationToken);
        var (from, to) = ResolveDateWindow(parameters);
        var items = await _timelineService.GetTimelineAsync(
            identity.UserId,
            identity.CompanyId,
            from,
            to,
            cancellationToken);
        return new MyWorkActivityResponse { Items = items };
    }

    public async Task<MyWorkKpiSummaryResponse> GetKpisAsync(
        MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var identity = await ResolveIdentityAsync(parameters, cancellationToken);
        var (periodStart, periodEnd) = ResolvePeriodWindow(parameters);

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

        await Task.WhenAll(missionsTask, tasksTask, recommendationsTask, decisionsTask, capacityTask, workloadTask);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return _kpiService.Calculate(
            missionsTask.Result,
            tasksTask.Result,
            recommendationsTask.Result,
            decisionsTask.Result,
            capacityTask.Result,
            workloadTask.Result,
            today,
            DefaultUpcomingDeadlineWindowDays);
    }

    /// <summary>Resolves CompanyId/UserId/ExecutionResourceId per DEC-501-001.</summary>
    private async Task<Identity> ResolveIdentityAsync(
        MyWorkDashboardQueryParameters parameters,
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

    /// <summary>
    /// Optional heuristic (DEC-501-001): the first active Execution Resource whose Code equals
    /// UserId, case-insensitive. Never runs for the "system" pseudo-user.
    /// </summary>
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
        MyWorkDashboardQueryParameters parameters)
    {
        var to = parameters.To ?? DateTimeOffset.UtcNow;
        var from = parameters.From ?? to.AddDays(-DefaultWindowDays);
        return (from, to);
    }

    private static (DateOnly PeriodStart, DateOnly PeriodEnd) ResolvePeriodWindow(
        MyWorkDashboardQueryParameters parameters)
    {
        var periodEnd = parameters.PeriodEnd ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var periodStart = parameters.PeriodStart ?? periodEnd.AddDays(-DefaultWindowDays);
        return (periodStart, periodEnd);
    }

    private static (
        IReadOnlyList<MyWorkDeadlineItemResponse> Overdue,
        IReadOnlyList<MyWorkDeadlineItemResponse> Upcoming) BuildDeadlines(
        IReadOnlyList<MyWorkTaskCardResponse> tasks,
        DateOnly today,
        int upcomingWindowDays = DefaultUpcomingDeadlineWindowDays)
    {
        var overdue = new List<MyWorkDeadlineItemResponse>();
        var upcoming = new List<MyWorkDeadlineItemResponse>();

        foreach (var task in tasks.Where(task => task.PlannedEnd.HasValue))
        {
            var plannedEnd = task.PlannedEnd!.Value;
            var daysRemaining = plannedEnd.DayNumber - today.DayNumber;

            var item = new MyWorkDeadlineItemResponse
            {
                TaskId = task.Id,
                MissionId = task.MissionId,
                TaskName = task.Name,
                MissionName = task.MissionName,
                PlannedEnd = plannedEnd,
                IsOverdue = task.IsOverdue,
                DaysRemaining = daysRemaining,
                DrillDownPath = task.DrillDownPath
            };

            if (task.IsOverdue)
            {
                overdue.Add(item);
            }
            else if (daysRemaining <= upcomingWindowDays)
            {
                upcoming.Add(item);
            }
        }

        return (
            overdue.OrderBy(item => item.PlannedEnd).ToList(),
            upcoming.OrderBy(item => item.PlannedEnd).ToList());
    }

    private sealed record Identity(Guid CompanyId, string UserId, Guid? ExecutionResourceId);
}
