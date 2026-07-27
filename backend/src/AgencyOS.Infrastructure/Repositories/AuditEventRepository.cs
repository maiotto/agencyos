using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class AuditEventRepository : IAuditEventRepository
{
    private readonly ApplicationDbContext _context;

    public AuditEventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AuditEvent>> QueryAsync(
        AuditEventQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(parameters);
        query = ApplyOrdering(query, parameters);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<AuditEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(audit => audit.Id == id, cancellationToken);
    }

    public async Task<AuditEvent> AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        _context.AuditEvents.Add(auditEvent);
        await _context.SaveChangesAsync(cancellationToken);
        return auditEvent;
    }

    private IQueryable<AuditEvent> BuildFilteredQuery(AuditEventQueryParameters parameters)
    {
        var query = _context.AuditEvents.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.EntityType))
        {
            var entityType = parameters.EntityType.Trim().ToLowerInvariant();
            query = query.Where(audit => audit.EntityType.ToLower() == entityType);
        }

        if (parameters.EntityId.HasValue)
        {
            query = query.Where(audit => audit.EntityId == parameters.EntityId.Value);
        }

        if (parameters.CompanyId.HasValue)
        {
            query = query.Where(audit => audit.CompanyId == parameters.CompanyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.UserId))
        {
            var userId = parameters.UserId.Trim().ToLowerInvariant();
            query = query.Where(audit => audit.UserId.ToLower() == userId);
        }

        if (parameters.CorrelationId.HasValue)
        {
            query = query.Where(audit => audit.CorrelationId == parameters.CorrelationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.EventType))
        {
            var eventType = parameters.EventType.Trim().ToLowerInvariant();
            query = query.Where(audit => audit.EventType.ToLower() == eventType);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Action))
        {
            var action = parameters.Action.Trim().ToLowerInvariant();
            query = query.Where(audit => audit.Action.ToLower().Contains(action));
        }

        if (parameters.OccurredFrom.HasValue)
        {
            query = query.Where(audit => audit.OccurredAt >= parameters.OccurredFrom.Value);
        }

        if (parameters.OccurredTo.HasValue)
        {
            query = query.Where(audit => audit.OccurredAt <= parameters.OccurredTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLowerInvariant();
            query = query.Where(audit =>
                audit.EntityType.ToLower().Contains(search)
                || audit.EventType.ToLower().Contains(search)
                || audit.Action.ToLower().Contains(search)
                || audit.UserId.ToLower().Contains(search)
                || audit.UserName.ToLower().Contains(search)
                || audit.EntityId.ToString().ToLower().Contains(search)
                || (audit.Metadata != null && audit.Metadata.ToLower().Contains(search)));
        }

        return query;
    }

    private static IQueryable<AuditEvent> ApplyOrdering(
        IQueryable<AuditEvent> query,
        AuditEventQueryParameters parameters)
    {
        var descending = !string.Equals(parameters.OrderDirection, "asc", StringComparison.OrdinalIgnoreCase);
        return (parameters.OrderBy?.Trim().ToLowerInvariant()) switch
        {
            "entitytype" => descending
                ? query.OrderByDescending(audit => audit.EntityType)
                : query.OrderBy(audit => audit.EntityType),
            "eventtype" => descending
                ? query.OrderByDescending(audit => audit.EventType)
                : query.OrderBy(audit => audit.EventType),
            "userid" => descending
                ? query.OrderByDescending(audit => audit.UserId)
                : query.OrderBy(audit => audit.UserId),
            _ => descending
                ? query.OrderByDescending(audit => audit.OccurredAt)
                : query.OrderBy(audit => audit.OccurredAt)
        };
    }
}
