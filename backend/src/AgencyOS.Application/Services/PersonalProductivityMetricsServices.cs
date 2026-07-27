using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;

namespace AgencyOS.Application.Services;

/// <summary>
/// Pure personal productivity metrics (US-507). Never modifies operational behavior (BR-3007).
/// </summary>
public class PersonalMetricsService : IPersonalMetricsService
{
    public PersonalProductivityKpiResponse BuildKpis(
        MyWorkKpiSummaryResponse myWorkKpis,
        int completedDecisionCount,
        int activityEventCount)
    {
        var pendingUnits = myWorkKpis.PendingDecisionCount + myWorkKpis.AssignedTaskCount;
        var completedUnits = completedDecisionCount;
        var denominator = pendingUnits + completedUnits;
        decimal? completionRate = denominator == 0
            ? null
            : Math.Round(100m * completedUnits / denominator, 2);

        return new PersonalProductivityKpiResponse
        {
            AssignedTaskCount = myWorkKpis.AssignedTaskCount,
            AssignedMissionCount = myWorkKpis.AssignedMissionCount,
            PendingRecommendationCount = myWorkKpis.PendingRecommendationCount,
            PendingDecisionCount = myWorkKpis.PendingDecisionCount,
            CompletedDecisionCount = completedDecisionCount,
            OverdueTaskCount = myWorkKpis.OverdueTaskCount,
            UpcomingDeadlineCount = myWorkKpis.UpcomingDeadlineCount,
            ActivityEventCount = activityEventCount,
            UtilizationPercentage = myWorkKpis.UtilizationPercentage,
            WorkloadPercentage = myWorkKpis.WorkloadPercentage,
            CompletionRatePercentage = completionRate
        };
    }

    public PersonalProductivityPerformanceIndicatorsResponse BuildPerformance(
        PersonalProductivityKpiResponse kpis,
        IReadOnlyList<MyWorkTaskCardResponse> tasks)
    {
        decimal? onTimePercentage = null;
        if (tasks.Count > 0)
        {
            var onTime = tasks.Count(task => !task.IsOverdue);
            onTimePercentage = Math.Round(100m * onTime / tasks.Count, 2);
        }

        var focusHint = "Balanced";
        if (kpis.OverdueTaskCount > 0)
        {
            focusHint = "Address overdue work";
        }
        else if (kpis.UtilizationPercentage is > 90m || kpis.WorkloadPercentage is > 90m)
        {
            focusHint = "High utilization — protect capacity";
        }
        else if (kpis.PendingDecisionCount + kpis.PendingRecommendationCount > 0)
        {
            focusHint = "Clear pending decisions and approvals";
        }

        return new PersonalProductivityPerformanceIndicatorsResponse
        {
            UtilizationPercentage = kpis.UtilizationPercentage,
            WorkloadPercentage = kpis.WorkloadPercentage,
            CompletionRatePercentage = kpis.CompletionRatePercentage,
            OnTimeTaskPercentage = onTimePercentage,
            OverdueTaskCount = kpis.OverdueTaskCount,
            ActivityIntensity = kpis.ActivityEventCount,
            FocusHint = focusHint
        };
    }

    public PersonalProductivityStatisticsResponse BuildStatistics(
        PersonalProductivityKpiResponse kpis,
        IReadOnlyList<MyWorkTaskCardResponse> tasks,
        MyWorkCapacitySummaryResponse capacity,
        MyWorkWorkloadSummaryResponse workload)
    {
        return new PersonalProductivityStatisticsResponse
        {
            MissionCount = kpis.AssignedMissionCount,
            PendingTaskCount = kpis.AssignedTaskCount,
            PendingRecommendationCount = kpis.PendingRecommendationCount,
            PendingDecisionCount = kpis.PendingDecisionCount,
            CompletedDecisionCount = kpis.CompletedDecisionCount,
            OverdueTaskCount = kpis.OverdueTaskCount,
            UpcomingDeadlineCount = kpis.UpcomingDeadlineCount,
            ActivityEventCount = kpis.ActivityEventCount,
            TotalPendingPlannedHours = tasks.Sum(task => task.AssignmentPlannedHours),
            CapacityHours = capacity.HasData ? capacity.TotalCapacityHours : null,
            WorkloadHours = workload.HasData ? workload.TotalPlannedHours : null,
            NavigationLinks =
            [
                new PersonalProductivityNavigationLinkResponse
                {
                    Label = "My Work",
                    Path = "/my-work",
                    Category = "Work"
                },
                new PersonalProductivityNavigationLinkResponse
                {
                    Label = "Planning Workspace",
                    Path = "/planning-workspace",
                    Category = "Planning"
                },
                new PersonalProductivityNavigationLinkResponse
                {
                    Label = "Recommendation Workspace",
                    Path = "/recommendation-workspace",
                    Category = "Recommendations"
                },
                new PersonalProductivityNavigationLinkResponse
                {
                    Label = "Decision Workspace",
                    Path = "/decision-workspace",
                    Category = "Decisions"
                },
                new PersonalProductivityNavigationLinkResponse
                {
                    Label = "Notification Center",
                    Path = "/notifications",
                    Category = "Notifications"
                },
                new PersonalProductivityNavigationLinkResponse
                {
                    Label = "Capacity",
                    Path = "/capacity",
                    Category = "Capacity"
                },
                new PersonalProductivityNavigationLinkResponse
                {
                    Label = "Workload History",
                    Path = "/workload/history",
                    Category = "Workload"
                },
                new PersonalProductivityNavigationLinkResponse
                {
                    Label = "Audit Trail",
                    Path = "/audit",
                    Category = "Audit"
                }
            ]
        };
    }
}

/// <summary>
/// Pure period-over-period trend calculation (US-507). Historical inputs are treated as immutable (BR-3004).
/// </summary>
public class PersonalTrendService : IPersonalTrendService
{
    private const decimal StableEpsilon = 0.5m;

    public PersonalProductivityTrendsResponse BuildTrends(
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
        int previousCompletedCount)
    {
        return new PersonalProductivityTrendsResponse
        {
            CurrentPeriodStart = currentPeriodStart,
            CurrentPeriodEnd = currentPeriodEnd,
            PreviousPeriodStart = previousPeriodStart,
            PreviousPeriodEnd = previousPeriodEnd,
            Points =
            [
                BuildPoint(
                    "CapacityUtilization",
                    currentCapacity.HasData ? currentCapacity.UtilizationPercentage : null,
                    previousCapacity.HasData ? previousCapacity.UtilizationPercentage : null,
                    "%"),
                BuildPoint(
                    "WorkloadUtilization",
                    currentWorkload.HasData ? currentWorkload.WorkloadPercentage : null,
                    previousWorkload.HasData ? previousWorkload.WorkloadPercentage : null,
                    "%"),
                BuildPoint(
                    "ActivityEvents",
                    currentActivityCount,
                    previousActivityCount,
                    "count"),
                BuildPoint(
                    "CompletedDecisions",
                    currentCompletedCount,
                    previousCompletedCount,
                    "count")
            ]
        };
    }

    private static PersonalProductivityTrendPointResponse BuildPoint(
        string metric,
        decimal? current,
        decimal? previous,
        string unit)
    {
        decimal? delta = null;
        var direction = "Stable";

        if (current.HasValue && previous.HasValue)
        {
            delta = Math.Round(current.Value - previous.Value, 2);
            if (delta > StableEpsilon)
            {
                direction = "Up";
            }
            else if (delta < -StableEpsilon)
            {
                direction = "Down";
            }
        }
        else if (current.HasValue && !previous.HasValue)
        {
            direction = "Up";
            delta = current;
        }
        else if (!current.HasValue && previous.HasValue)
        {
            direction = "Down";
            delta = -previous;
        }

        return new PersonalProductivityTrendPointResponse
        {
            Metric = metric,
            CurrentValue = current,
            PreviousValue = previous,
            Delta = delta,
            Direction = direction,
            Unit = unit
        };
    }
}

/// <summary>
/// Pure pending/completed/timeline activity summary (US-507 / BR-3008 drill-down paths).
/// </summary>
public class ActivitySummaryService : IActivitySummaryService
{
    public PersonalProductivityActivitySummaryResponse Build(
        IReadOnlyList<MyWorkTaskCardResponse> pendingTasks,
        IReadOnlyList<MyWorkRecommendationCardResponse> pendingRecommendations,
        IReadOnlyList<MyWorkDecisionCardResponse> pendingDecisions,
        IReadOnlyList<PersonalProductivityActivityItemResponse> completedItems,
        IReadOnlyList<MyWorkActivityItemResponse> timeline)
    {
        var pending = new List<PersonalProductivityActivityItemResponse>();

        pending.AddRange(pendingTasks.Select(task => new PersonalProductivityActivityItemResponse
        {
            Id = task.Id,
            Kind = "Task",
            Title = task.Name,
            Status = task.IsOverdue ? "Overdue" : (task.Status ?? "Active"),
            OccurredAt = null,
            DrillDownPath = task.DrillDownPath
        }));

        pending.AddRange(pendingRecommendations.Select(item => new PersonalProductivityActivityItemResponse
        {
            Id = item.Id,
            Kind = "Recommendation",
            Title = item.Title,
            Status = item.Status,
            OccurredAt = item.CreatedAt,
            DrillDownPath = item.DrillDownPath
        }));

        pending.AddRange(pendingDecisions.Select(item => new PersonalProductivityActivityItemResponse
        {
            Id = item.Id,
            Kind = "Decision",
            Title = $"Decision {item.DecisionStatus}",
            Status = item.DecisionStatus,
            OccurredAt = item.DecisionDate,
            DrillDownPath = item.DrillDownPath
        }));

        return new PersonalProductivityActivitySummaryResponse
        {
            Pending = pending,
            Completed = completedItems,
            Timeline = timeline
        };
    }
}
