using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Pure, read-only Recommendation Workspace navigation (US-503 / BR-2607). Returns an ordered
/// set of deep-links into existing, already-audited frontend routes/APIs. Never touches a
/// repository and never introduces a new write path (DEC-503-001).
/// </summary>
public interface IRecommendationNavigationService
{
    RecommendationNavigationResponse GetNavigation(Guid companyId);
}
