using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Read-only Planning History (Audit Trail) projection (US-502 / BR-2510: the Workspace is fully
/// auditable). Queries <see cref="IAuditQueryService"/> for a Company within an optional From/To
/// window and filters down to planning-related entity types. Never introduces a new write path.
/// </summary>
public interface IPlanningHistoryService
{
    Task<PlanningHistoryResponse> GetHistoryAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default);
}
