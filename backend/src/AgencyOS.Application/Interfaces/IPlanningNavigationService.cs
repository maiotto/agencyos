using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Pure, read-only Planning Workspace navigation (US-502 / BR-2508). Returns an ordered set of
/// deep-links into existing, already-audited frontend routes. Never touches a repository and
/// never introduces a new write path (DEC-502-001).
/// </summary>
public interface IPlanningNavigationService
{
    PlanningNavigationResponse GetNavigation(Guid companyId);
}
