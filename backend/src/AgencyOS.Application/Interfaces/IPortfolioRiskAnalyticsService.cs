namespace AgencyOS.Application.Interfaces;

using AgencyOS.Application.DTOs;

/// <summary>
/// Flags at-risk Portfolios (Overloaded/AtRisk health, low Recommendation scores, cancelled
/// Decisions) from a set of already-built <see cref="PortfolioAnalyticsSnapshot"/> projections
/// (US-404 / BR-2210). Pure calculation — never queries a repository directly.
/// </summary>
public interface IPortfolioRiskAnalyticsService
{
    IReadOnlyList<PortfolioRiskIndicatorResponse> BuildRiskIndicators(
        IReadOnlyList<PortfolioAnalyticsSnapshot> snapshots);
}
