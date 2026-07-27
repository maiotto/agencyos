using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Pure, read-only Executive Workspace navigation (US-505 / BR-2807). Returns an ordered set of
/// deep-links into existing, already-audited frontend routes and dashboards. Never touches a
/// repository and never introduces a new write path (DEC-505-001).
/// </summary>
public interface IExecutiveNavigationService
{
    ExecutiveNavigationResponse GetNavigation(Guid companyId);
}
