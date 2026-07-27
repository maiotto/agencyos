namespace AgencyOS.Application.Interfaces;

using AgencyOS.Application.DTOs;

/// <summary>
/// Read-only Portfolio Analytics orchestration (US-404 / BR-2201..BR-2210). Resolves the active
/// Company, builds mission-scoped Recommendation/Decision snapshots per Portfolio, and delegates
/// trends/comparison/health/risk/ranking computation to the specialized analytics services.
/// Never modifies a Portfolio and never recalculates Capacity/Workload engines.
/// </summary>
public interface IPortfolioAnalyticsService
{
    Task<PortfolioAnalyticsOverviewResponse> GetOverviewAsync(
        PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PortfolioAnalyticsDetailResponse> GetDetailAsync(
        Guid portfolioId,
        PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PortfolioTrendsResponse> GetTrendsAsync(
        PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PortfolioComparisonResponse> GetComparisonAsync(
        PortfolioCompareQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PortfolioRankingResponse> GetRankingAsync(
        PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PortfolioHealthAnalyticsResponse> GetHealthAnalyticsAsync(
        PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PortfolioPerformanceResponse> GetPerformanceAsync(
        PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
