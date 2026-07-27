using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Pure personal productivity metrics projection (US-507 / BR-3003, BR-3007). No I/O.
/// </summary>
public interface IPersonalMetricsService
{
    PersonalProductivityKpiResponse BuildKpis(
        MyWorkKpiSummaryResponse myWorkKpis,
        int completedDecisionCount,
        int activityEventCount);

    PersonalProductivityPerformanceIndicatorsResponse BuildPerformance(
        PersonalProductivityKpiResponse kpis,
        IReadOnlyList<MyWorkTaskCardResponse> tasks);

    PersonalProductivityStatisticsResponse BuildStatistics(
        PersonalProductivityKpiResponse kpis,
        IReadOnlyList<MyWorkTaskCardResponse> tasks,
        MyWorkCapacitySummaryResponse capacity,
        MyWorkWorkloadSummaryResponse workload);
}

/// <summary>
/// Pure period-over-period trend projection (US-507 / BR-3004, BR-3006). No I/O.
/// </summary>
public interface IPersonalTrendService
{
    PersonalProductivityTrendsResponse BuildTrends(
        DateOnly currentPeriodStart,
        DateOnly currentPeriodEnd,
        DateOnly previousPeriodStart,
        DateOnly previousPeriodEnd,
        MyWorkCapacitySummaryResponse currentCapacity,
        MyWorkWorkloadSummaryResponse currentWorkload,
        MyWorkCapacitySummaryResponse previousCapacity,
        MyWorkWorkloadSummaryResponse previousWorkload,
        int currentActivityCount,
        int previousActivityCount,
        int currentCompletedCount,
        int previousCompletedCount);
}

/// <summary>
/// Pure activity summary projection for pending/completed work and timeline (US-507). No I/O.
/// </summary>
public interface IActivitySummaryService
{
    PersonalProductivityActivitySummaryResponse Build(
        IReadOnlyList<MyWorkTaskCardResponse> pendingTasks,
        IReadOnlyList<MyWorkRecommendationCardResponse> pendingRecommendations,
        IReadOnlyList<MyWorkDecisionCardResponse> pendingDecisions,
        IReadOnlyList<PersonalProductivityActivityItemResponse> completedItems,
        IReadOnlyList<MyWorkActivityItemResponse> timeline);
}

/// <summary>
/// Read-only Personal Productivity Dashboard orchestration (US-507 / BR-3001..BR-3010).
/// Reuses My Work aggregation/timeline; never modifies operational data; records usage audit (BR-3010).
/// </summary>
public interface IPersonalProductivityDashboardService
{
    Task<PersonalProductivityDashboardResponse> GetDashboardAsync(
        PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PersonalProductivitySummaryResponse> GetSummaryAsync(
        PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PersonalProductivityKpiResponse> GetKpisAsync(
        PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PersonalProductivityTrendsResponse> GetTrendsAsync(
        PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<MyWorkCapacitySummaryResponse> GetCapacityAsync(
        PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<MyWorkWorkloadSummaryResponse> GetWorkloadAsync(
        PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PersonalProductivityActivitySummaryResponse> GetActivityAsync(
        PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PersonalProductivityStatisticsResponse> GetStatisticsAsync(
        PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
