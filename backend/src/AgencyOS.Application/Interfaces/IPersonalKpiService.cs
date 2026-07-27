using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Pure calculation of personal KPIs from already-aggregated My Work Dashboard sections
/// (US-501 / BR-2407). Performs no I/O and no business calculations beyond simple counting.
/// </summary>
public interface IPersonalKpiService
{
    MyWorkKpiSummaryResponse Calculate(
        IReadOnlyList<MyWorkMissionCardResponse> missions,
        IReadOnlyList<MyWorkTaskCardResponse> tasks,
        IReadOnlyList<MyWorkRecommendationCardResponse> recommendations,
        IReadOnlyList<MyWorkDecisionCardResponse> decisions,
        MyWorkCapacitySummaryResponse capacity,
        MyWorkWorkloadSummaryResponse workload,
        DateOnly today,
        int upcomingDeadlineWindowDays = 14);
}
