using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

/// <summary>
/// Read-only Planning History (Audit Trail) projection (US-502 / BR-2510: the Workspace is fully
/// auditable). Queries the existing <see cref="IAuditQueryService"/> for a Company within an
/// optional From/To window and filters down to planning-related entity types. Introduces no new
/// write path — every event was already recorded by the module that produced it.
/// </summary>
public class PlanningHistoryService : IPlanningHistoryService
{
    private static readonly HashSet<string> PlanningEntityTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        AuditEntityTypes.PlanningTemplate,
        AuditEntityTypes.Portfolio,
        AuditEntityTypes.CapacityHistory,
        AuditEntityTypes.WorkloadHistory,
        AuditEntityTypes.CrossPortfolioPlan
    };

    private readonly IAuditQueryService _auditQueryService;

    public PlanningHistoryService(IAuditQueryService auditQueryService)
    {
        _auditQueryService = auditQueryService;
    }

    public async Task<PlanningHistoryResponse> GetHistoryAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var events = await _auditQueryService.GetAllAsync(
            new AuditEventQueryParameters
            {
                CompanyId = companyId,
                OccurredFrom = from,
                OccurredTo = to
            },
            cancellationToken);

        var items = events
            .Where(auditEvent => PlanningEntityTypes.Contains(auditEvent.EntityType))
            .OrderByDescending(auditEvent => auditEvent.OccurredAt)
            .Select(auditEvent => new PlanningHistoryItemResponse
            {
                Id = auditEvent.Id,
                EntityType = auditEvent.EntityType,
                EntityId = auditEvent.EntityId,
                EventType = auditEvent.EventType,
                Action = auditEvent.Action,
                OccurredAt = auditEvent.OccurredAt,
                UserId = auditEvent.UserId,
                DrillDownPath = $"/audit/{auditEvent.Id}"
            })
            .ToList();

        return new PlanningHistoryResponse
        {
            CompanyId = companyId,
            From = from,
            To = to,
            Items = items,
            Action = new PlanningActionResponse
            {
                Key = "audit-trail",
                Label = "Audit Trail",
                Category = "History",
                DrillDownPath = $"/audit?companyId={companyId}",
                Description = "Full Audit Trail — the durable, tamper-evident record behind Planning History (BR-2510)."
            }
        };
    }
}
