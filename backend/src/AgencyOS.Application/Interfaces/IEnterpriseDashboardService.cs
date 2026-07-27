namespace AgencyOS.Application.Interfaces;

using AgencyOS.Application.DTOs;

/// <summary>
/// Read-only Enterprise Dashboard orchestration (US-403 / BR-2101..BR-2110).
/// Resolves the active Company, delegates section aggregation, and fills in
/// period-over-period trend indicators.
/// </summary>
public interface IEnterpriseDashboardService
{
    Task<EnterpriseDashboardResponse> GetDashboardAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<EnterpriseDashboardSummaryResponse> GetSummaryAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<EnterpriseDashboardPlanningResponse> GetPlanningAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<EnterpriseDashboardPortfolioResponse> GetPortfolioAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<EnterpriseDashboardCapacityResponse> GetCapacityAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<EnterpriseDashboardWorkloadResponse> GetWorkloadAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<EnterpriseDashboardRecommendationsResponse> GetRecommendationsAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<EnterpriseDashboardDecisionsResponse> GetDecisionsAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<EnterpriseDashboardAiResponse> GetAiAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<EnterpriseDashboardAuditResponse> GetAuditAsync(
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
