using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Detects Portfolio-level Mission overlap and Resource conflicts across a Cross-Portfolio Plan
/// selection (US-405 / BR-2305). Resource conflicts are delegated to the existing
/// <c>IAllocationConflictDetectionService</c> — never re-implemented. Purely advisory: no conflict
/// is ever auto-resolved.
/// </summary>
public interface ICrossPortfolioConflictDetectionService
{
    Task<ConflictSummaryResponse> DetectAsync(
        IReadOnlyList<Portfolio> portfolios,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default);
}
