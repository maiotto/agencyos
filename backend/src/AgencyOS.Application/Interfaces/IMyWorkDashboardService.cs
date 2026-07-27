using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Read-only My Work Dashboard orchestration (US-501 / BR-2401..BR-2410). Resolves the caller's
/// Company/User/ExecutionResource identity (DEC-501-001), delegates section aggregation, and
/// never writes to any repository.
/// </summary>
public interface IMyWorkDashboardService
{
    Task<MyWorkDashboardResponse> GetDashboardAsync(
        MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<MyWorkSummaryResponse> GetSummaryAsync(
        MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<MyWorkTasksResponse> GetTasksAsync(
        MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<MyWorkMissionsResponse> GetMissionsAsync(
        MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<MyWorkRecommendationsResponse> GetRecommendationsAsync(
        MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<MyWorkDecisionsResponse> GetDecisionsAsync(
        MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<MyWorkActivityResponse> GetActivityAsync(
        MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<MyWorkKpiSummaryResponse> GetKpisAsync(
        MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
