namespace AgencyOS.Application.Interfaces;

using AgencyOS.Application.DTOs;

/// <summary>
/// Computes deterministic Portfolio health distribution and risk indicators from a set of
/// already-built <see cref="PortfolioAnalyticsSnapshot"/> projections (US-404 / BR-2203).
/// Pure calculation — never queries a repository directly, mirroring
/// <see cref="IDashboardHealthCalculationService"/>.
/// </summary>
public interface IPortfolioHealthAnalyticsService
{
    PortfolioHealthAnalyticsResponse BuildHealthAnalytics(
        Guid companyId,
        IReadOnlyList<PortfolioAnalyticsSnapshot> snapshots);
}
