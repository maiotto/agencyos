using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IAuditEventRepository
{
    Task<IReadOnlyList<AuditEvent>> QueryAsync(
        AuditEventQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<AuditEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AuditEvent> AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Automatic audit recorder. Failures never propagate to callers (BR-1510).
/// </summary>
public interface IAuditService
{
    Task RecordSafeAsync(AuditEventWriteRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Read-only audit queries (BR-1509).
/// </summary>
public interface IAuditQueryService
{
    Task<IReadOnlyList<AuditEventResponse>> GetAllAsync(
        AuditEventQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditEventResponse>> FilterAsync(
        AuditEventQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<AuditEventResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditEventResponse>> GetByEntityIdAsync(
        Guid entityId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditEventResponse>> GetByCorrelationIdAsync(
        Guid correlationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditEventResponse>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditEventResponse>> GetByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Request-scoped correlation/session/request identifiers (BR-1508).
/// </summary>
public interface IAuditContext
{
    Guid? CorrelationId { get; }

    string? SessionId { get; }

    string? RequestId { get; }

    string? UserId { get; }

    string? UserName { get; }
}
