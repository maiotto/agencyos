namespace AgencyOS.Application.Interfaces;

using AgencyOS.Application.DTOs;

/// <summary>
/// Builds a side-by-side comparison of two Portfolios using stored snapshot fields plus
/// mission-scoped Recommendation/Decision counts (US-404 / BR-2202). Read-only; never mutates
/// either Portfolio.
/// </summary>
public interface IPortfolioComparisonService
{
    Task<PortfolioComparisonResponse> CompareAsync(
        Guid companyId,
        PortfolioCompareQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
