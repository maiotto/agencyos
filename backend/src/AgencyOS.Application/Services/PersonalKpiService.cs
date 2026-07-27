using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;

namespace AgencyOS.Application.Services;

/// <summary>
/// Pure calculation of personal KPIs from already-aggregated My Work Dashboard sections
/// (US-501 / BR-2407). Performs no I/O — every value is derived from counts already computed
/// by <see cref="IPersonalDashboardAggregationService"/>.
/// </summary>
public class PersonalKpiService : IPersonalKpiService
{
    public MyWorkKpiSummaryResponse Calculate(
        IReadOnlyList<MyWorkMissionCardResponse> missions,
        IReadOnlyList<MyWorkTaskCardResponse> tasks,
        IReadOnlyList<MyWorkRecommendationCardResponse> recommendations,
        IReadOnlyList<MyWorkDecisionCardResponse> decisions,
        MyWorkCapacitySummaryResponse capacity,
        MyWorkWorkloadSummaryResponse workload,
        DateOnly today,
        int upcomingDeadlineWindowDays = 14)
    {
        var deadlineHorizon = today.AddDays(upcomingDeadlineWindowDays);

        var overdueTaskCount = tasks.Count(task => task.IsOverdue);
        var upcomingDeadlineCount = tasks.Count(task =>
            !task.IsOverdue
            && task.PlannedEnd.HasValue
            && task.PlannedEnd.Value >= today
            && task.PlannedEnd.Value <= deadlineHorizon);

        return new MyWorkKpiSummaryResponse
        {
            AssignedTaskCount = tasks.Count,
            AssignedMissionCount = missions.Count,
            PendingRecommendationCount = recommendations.Count,
            PendingDecisionCount = decisions.Count,
            OverdueTaskCount = overdueTaskCount,
            UpcomingDeadlineCount = upcomingDeadlineCount,
            UtilizationPercentage = capacity.HasData ? capacity.UtilizationPercentage : null,
            WorkloadPercentage = workload.HasData ? workload.WorkloadPercentage : null
        };
    }
}
