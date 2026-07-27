namespace AgencyOS.Application.Interfaces;

using AgencyOS.Application.DTOs;

/// <summary>
/// Builds raw Enterprise Dashboard section data from existing repositories (US-403 / BR-2102).
/// Never recalculates Capacity/Workload history and never persists new analytical storage.
/// Trend fields are left at their defaults — <see cref="AgencyOS.Application.Interfaces.IEnterpriseDashboardService"/>
/// fills them in by calling the same builder for a previous, equal-length period.
/// </summary>
public interface IDashboardAggregationService
{
    Task<EnterpriseDashboardSummaryResponse> BuildSummaryAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<EnterpriseDashboardPlanningResponse> BuildPlanningAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<EnterpriseDashboardPortfolioResponse> BuildPortfolioAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<EnterpriseDashboardCapacityResponse> BuildCapacityAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<EnterpriseDashboardWorkloadResponse> BuildWorkloadAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<EnterpriseDashboardRecommendationsResponse> BuildRecommendationsAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<EnterpriseDashboardDecisionsResponse> BuildDecisionsAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<EnterpriseDashboardAiResponse> BuildAiAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<EnterpriseDashboardAuditResponse> BuildAuditAsync(
        Guid companyId,
        EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
