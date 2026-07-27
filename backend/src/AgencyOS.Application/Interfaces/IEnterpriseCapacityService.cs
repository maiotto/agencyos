using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Builds an enterprise-wide Capacity view across a set of Portfolios for Cross-Portfolio Planning
/// (US-405 / BR-2303). Reuses each Portfolio's stored CapacitySummary snapshot and, when a planning
/// period is supplied, complements it with a live <c>ICapacityCalculatorService</c> aggregate. Never
/// recalculates or persists a Portfolio's own Capacity snapshot.
/// </summary>
public interface IEnterpriseCapacityService
{
    Task<EnterpriseCapacityResponse> BuildAsync(
        IReadOnlyList<Portfolio> portfolios,
        DateOnly? periodStart,
        DateOnly? periodEnd,
        CancellationToken cancellationToken = default);
}
