using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Builds raw My Work Dashboard section data directly from existing read repositories/services
/// (US-501 / BR-2401..BR-2410). Never writes to any repository.
/// </summary>
public interface IPersonalDashboardAggregationService
{
    /// <summary>
    /// Active Missions (BR-2406) reachable through the resolved Execution Resource's active
    /// Assignments. Returns an empty list when <paramref name="executionResourceId"/> is null.
    /// </summary>
    Task<IReadOnlyList<MyWorkMissionCardResponse>> GetMissionsAsync(
        Guid? executionResourceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Active Tasks (BR-2406) reachable through the resolved Execution Resource's active
    /// Assignments. Returns an empty list when <paramref name="executionResourceId"/> is null.
    /// </summary>
    Task<IReadOnlyList<MyWorkTaskCardResponse>> GetTasksAsync(
        Guid? executionResourceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Company-scoped pending RecommendationWorkflows (Status=PendingApproval) optionally unioned
    /// with workflows for Recommendations the caller generated (DEC-501-001).
    /// </summary>
    Task<IReadOnlyList<MyWorkRecommendationCardResponse>> GetRecommendationsAsync(
        Guid companyId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pending Decisions (DecisionStatus Created or InProgress) for the Company, preferring
    /// entries CreatedBy the caller when UserId is not "system" (DEC-501-001).
    /// </summary>
    Task<IReadOnlyList<MyWorkDecisionCardResponse>> GetDecisionsAsync(
        Guid companyId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Capacity summary for the resolved Execution Resource via
    /// <c>ICapacityCalculatorService.GetByResourceIdAsync</c>. Returns an empty summary
    /// (<c>HasData = false</c>) when the resource is unresolved or the calculation cannot run.
    /// </summary>
    Task<MyWorkCapacitySummaryResponse> GetCapacityAsync(
        Guid? executionResourceId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Workload summary for the resolved Execution Resource via
    /// <c>IWorkloadCalculatorService.GetByResourceIdAsync</c>. Returns an empty summary
    /// (<c>HasData = false</c>) when the resource is unresolved or the calculation cannot run.
    /// </summary>
    Task<MyWorkWorkloadSummaryResponse> GetWorkloadAsync(
        Guid? executionResourceId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default);
}
