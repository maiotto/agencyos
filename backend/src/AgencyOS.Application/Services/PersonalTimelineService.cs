using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;

namespace AgencyOS.Application.Services;

/// <summary>
/// Builds the caller's Activity Timeline from existing Audit Events (US-501 / BR-2405).
/// Uses <see cref="IAuditQueryService.GetByUserIdAsync"/> and applies CompanyId/From/To
/// narrowing in memory since that query does not accept those filters. Read-only: never
/// writes Audit Events.
/// </summary>
public class PersonalTimelineService : IPersonalTimelineService
{
    private readonly IAuditQueryService _auditQueryService;

    public PersonalTimelineService(IAuditQueryService auditQueryService)
    {
        _auditQueryService = auditQueryService;
    }

    public async Task<IReadOnlyList<MyWorkActivityItemResponse>> GetTimelineAsync(
        string userId,
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var events = await _auditQueryService.GetByUserIdAsync(userId, cancellationToken);

        return events
            .Where(auditEvent => auditEvent.CompanyId is null || auditEvent.CompanyId == companyId)
            .Where(auditEvent => !from.HasValue || auditEvent.OccurredAt >= from.Value)
            .Where(auditEvent => !to.HasValue || auditEvent.OccurredAt <= to.Value)
            .OrderByDescending(auditEvent => auditEvent.OccurredAt)
            .Select(auditEvent => new MyWorkActivityItemResponse
            {
                Id = auditEvent.Id,
                OccurredAt = auditEvent.OccurredAt,
                EntityType = auditEvent.EntityType,
                EntityId = auditEvent.EntityId,
                EventType = auditEvent.EventType,
                Action = auditEvent.Action,
                Summary = $"{auditEvent.Action} {auditEvent.EntityType} ({auditEvent.EventType})",
                DrillDownPath = $"/audit/{auditEvent.Id}"
            })
            .ToList();
    }
}
