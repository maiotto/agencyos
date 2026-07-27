namespace AgencyOS.Application.DTOs;

public class AuditEventQueryParameters
{
    public string? EntityType { get; set; }

    public Guid? EntityId { get; set; }

    public Guid? CompanyId { get; set; }

    public string? UserId { get; set; }

    public Guid? CorrelationId { get; set; }

    public string? EventType { get; set; }

    public string? Action { get; set; }

    public string? Search { get; set; }

    public DateTimeOffset? OccurredFrom { get; set; }

    public DateTimeOffset? OccurredTo { get; set; }

    public string? OrderBy { get; set; }

    public string? OrderDirection { get; set; }
}

public class AuditEventResponse
{
    public Guid Id { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public string? EntityVersion { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public Guid? CompanyId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; set; }

    public string Source { get; set; } = string.Empty;

    public Guid? CorrelationId { get; set; }

    public string? SessionId { get; set; }

    public string? RequestId { get; set; }

    public string? PreviousState { get; set; }

    public string? CurrentState { get; set; }

    public string? Metadata { get; set; }
}

public class AuditEventWriteRequest
{
    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public string? EntityVersion { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public Guid? CompanyId { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public DateTimeOffset? OccurredAt { get; set; }

    public string? Source { get; set; }

    public Guid? CorrelationId { get; set; }

    public string? SessionId { get; set; }

    public string? RequestId { get; set; }

    public string? PreviousState { get; set; }

    public string? CurrentState { get; set; }

    public string? Metadata { get; set; }
}
