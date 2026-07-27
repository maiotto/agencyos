namespace AgencyOS.Application.Interfaces;

using AgencyOS.Application.DTOs;

/// <summary>
/// Builds Capacity/Workload/Health/Recommendation/Decision trend series bucketed by month
/// (US-404 / BR-2204..BR-2210). Reads exclusively from immutable Capacity/Workload History and
/// existing Recommendation/Decision repositories — never recalculates the underlying engines.
/// </summary>
public interface IPortfolioTrendAnalysisService
{
    Task<PortfolioTrendsResponse> BuildTrendsAsync(
        Guid companyId,
        PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
