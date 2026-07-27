using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Pure, read-only Decision Workspace navigation (US-504 / BR-2707). Returns an ordered set of
/// deep-links into existing, already-audited frontend routes. Never touches a repository and
/// never introduces a new write path (DEC-504-001).
/// </summary>
public interface IDecisionNavigationService
{
    DecisionNavigationResponse GetNavigation(Guid companyId);
}
