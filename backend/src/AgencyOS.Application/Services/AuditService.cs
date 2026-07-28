using System.Text.Json;
using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class AuditService : IAuditService, IAuditQueryService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly IAuditEventRepository _repository;
    private readonly IAuditContext _auditContext;
    private readonly INotificationGenerationService _notificationGenerationService;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IAuditEventRepository repository,
        IAuditContext auditContext,
        INotificationGenerationService notificationGenerationService,
        ILogger<AuditService> logger)
    {
        _repository = repository;
        _auditContext = auditContext;
        _notificationGenerationService = notificationGenerationService;
        _logger = logger;
    }

    public async Task RecordSafeAsync(
        AuditEventWriteRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = FirstNonEmpty(request.UserId, _auditContext.UserId, "system");
            var userName = FirstNonEmpty(request.UserName, _auditContext.UserName, userId);
            var source = FirstNonEmpty(request.Source, AuditSources.Api);

            var auditEvent = AuditEvent.Create(
                entityType: request.EntityType,
                entityId: request.EntityId,
                entityVersion: request.EntityVersion,
                eventType: request.EventType,
                action: request.Action,
                companyId: request.CompanyId,
                userId: userId,
                userName: userName,
                occurredAt: request.OccurredAt ?? DateTimeOffset.UtcNow,
                source: source,
                correlationId: request.CorrelationId ?? _auditContext.CorrelationId,
                sessionId: request.SessionId ?? _auditContext.SessionId,
                requestId: request.RequestId ?? _auditContext.RequestId,
                previousState: request.PreviousState,
                currentState: request.CurrentState,
                metadata: request.Metadata);

            await _repository.AddAsync(auditEvent, cancellationToken);
            _logger.LogInformation(
                "Audit event recorded {AuditEventId} Entity={EntityType}/{EntityId} Event={EventType}",
                auditEvent.Id,
                auditEvent.EntityType,
                auditEvent.EntityId,
                auditEvent.EventType);

            await GenerateNotificationFromAuditSafeAsync(auditEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            // BR-1510: audit failures must never prevent business transactions.
            _logger.LogError(
                ex,
                "Audit generation failed independently for Entity={EntityType}/{EntityId} Event={EventType}",
                request.EntityType,
                request.EntityId,
                request.EventType);
        }
    }

    public async Task<IReadOnlyList<AuditEventResponse>> GetAllAsync(
        AuditEventQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var items = await _repository.QueryAsync(parameters, cancellationToken);
        return items.Select(MapToResponse).ToList();
    }

    public Task<IReadOnlyList<AuditEventResponse>> FilterAsync(
        AuditEventQueryParameters parameters,
        CancellationToken cancellationToken = default) =>
        GetAllAsync(parameters, cancellationToken);

    public async Task<AuditEventResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            throw new NotFoundException($"Audit event with id '{id}' was not found.");
        }

        return MapToResponse(item);
    }

    public Task<IReadOnlyList<AuditEventResponse>> GetByEntityIdAsync(
        Guid entityId,
        CancellationToken cancellationToken = default) =>
        GetAllAsync(new AuditEventQueryParameters { EntityId = entityId }, cancellationToken);

    public Task<IReadOnlyList<AuditEventResponse>> GetByCorrelationIdAsync(
        Guid correlationId,
        CancellationToken cancellationToken = default) =>
        GetAllAsync(new AuditEventQueryParameters { CorrelationId = correlationId }, cancellationToken);

    public Task<IReadOnlyList<AuditEventResponse>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default) =>
        GetAllAsync(new AuditEventQueryParameters { UserId = userId }, cancellationToken);

    public Task<IReadOnlyList<AuditEventResponse>> GetByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default) =>
        GetAllAsync(new AuditEventQueryParameters { CompanyId = companyId }, cancellationToken);

    public static string SerializeState(object? state)
    {
        if (state is null)
        {
            return "null";
        }

        if (state is string text)
        {
            return text;
        }

        return JsonSerializer.Serialize(state, JsonOptions);
    }

    private async Task GenerateNotificationFromAuditSafeAsync(
        AuditEvent auditEvent,
        CancellationToken cancellationToken)
    {
        // Never notify on Notification actions (BR-2910 audit must not recurse into generation loops).
        if (string.Equals(auditEvent.EntityType, AuditEntityTypes.Notification, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var mapping = MapAuditToNotification(auditEvent);
        if (mapping is null)
        {
            return;
        }

        await _notificationGenerationService.GenerateSafeAsync(
            new NotificationGenerationRequest
            {
                CompanyId = auditEvent.CompanyId,
                UserId = auditEvent.UserId,
                Title = mapping.Value.Title,
                Message = mapping.Value.Message,
                Category = mapping.Value.Category,
                Priority = mapping.Value.Priority,
                SourceEntity = mapping.Value.SourceEntity,
                SourceEntityId = mapping.Value.SourceEntityId
            },
            cancellationToken);
    }

    private static (string Title, string Message, string Category, string Priority, string SourceEntity, Guid? SourceEntityId)? MapAuditToNotification(
        AuditEvent auditEvent)
    {
        if (string.Equals(auditEvent.EntityType, AuditEntityTypes.Decision, StringComparison.OrdinalIgnoreCase))
        {
            return (
                $"Decision {auditEvent.EventType}",
                $"Decision action '{auditEvent.Action}' was recorded.",
                NotificationCategory.Decision,
                NotificationPriority.High,
                NotificationSourceEntities.Decision,
                auditEvent.EntityId);
        }

        if (string.Equals(auditEvent.EntityType, AuditEntityTypes.RecommendationWorkflow, StringComparison.OrdinalIgnoreCase))
        {
            return (
                $"Recommendation workflow {auditEvent.EventType}",
                $"Recommendation workflow action '{auditEvent.Action}' was recorded.",
                NotificationCategory.Recommendation,
                NotificationPriority.High,
                NotificationSourceEntities.RecommendationWorkflow,
                auditEvent.EntityId);
        }

        if (string.Equals(auditEvent.EntityType, AuditEntityTypes.CapacityHistory, StringComparison.OrdinalIgnoreCase))
        {
            return (
                "Capacity calculation recorded",
                $"Capacity history action '{auditEvent.Action}' was recorded.",
                NotificationCategory.Capacity,
                NotificationPriority.Medium,
                NotificationSourceEntities.CapacityHistory,
                auditEvent.EntityId);
        }

        if (string.Equals(auditEvent.EntityType, AuditEntityTypes.Portfolio, StringComparison.OrdinalIgnoreCase))
        {
            return (
                $"Portfolio {auditEvent.EventType}",
                $"Portfolio action '{auditEvent.Action}' was recorded.",
                NotificationCategory.Portfolio,
                NotificationPriority.Medium,
                NotificationSourceEntities.Portfolio,
                auditEvent.EntityId);
        }

        if (string.Equals(auditEvent.EntityType, AuditEntityTypes.PlanningTemplate, StringComparison.OrdinalIgnoreCase))
        {
            return (
                $"Planning template {auditEvent.EventType}",
                $"Planning template action '{auditEvent.Action}' was recorded.",
                NotificationCategory.Planning,
                NotificationPriority.Medium,
                NotificationSourceEntities.PlanningWorkspace,
                auditEvent.EntityId);
        }

        if (string.Equals(auditEvent.EntityType, AuditEntityTypes.ExecutiveRecommendationSummary, StringComparison.OrdinalIgnoreCase))
        {
            return (
                "Executive summary updated",
                $"Executive summary action '{auditEvent.Action}' was recorded.",
                NotificationCategory.Executive,
                NotificationPriority.Medium,
                NotificationSourceEntities.ExecutiveWorkspace,
                auditEvent.EntityId);
        }

        if (string.Equals(auditEvent.EntityType, AuditEntityTypes.Company, StringComparison.OrdinalIgnoreCase))
        {
            return (
                $"Company {auditEvent.EventType}",
                $"Company action '{auditEvent.Action}' was recorded.",
                NotificationCategory.Company,
                NotificationPriority.Low,
                NotificationSourceEntities.Company,
                auditEvent.EntityId);
        }

        if (string.Equals(auditEvent.EntityType, AuditEntityTypes.Recommendation, StringComparison.OrdinalIgnoreCase))
        {
            return (
                $"Recommendation {auditEvent.EventType}",
                $"Recommendation action '{auditEvent.Action}' was recorded.",
                NotificationCategory.Recommendation,
                NotificationPriority.Medium,
                NotificationSourceEntities.RecommendationWorkspace,
                auditEvent.EntityId);
        }

        // US-506 / DEC-506-001: notify only for the mapped business entity types above
        // (plus direct Portfolio-create / Company-context hooks). Do not notify for
        // usage audits, advisory simulations, or other unmapped audit entities.
        return null;
    }

    private static AuditEventResponse MapToResponse(AuditEvent audit) =>
        new()
        {
            Id = audit.Id,
            EntityType = audit.EntityType,
            EntityId = audit.EntityId,
            EntityVersion = audit.EntityVersion,
            EventType = audit.EventType,
            Action = audit.Action,
            CompanyId = audit.CompanyId,
            UserId = audit.UserId,
            UserName = audit.UserName,
            OccurredAt = audit.OccurredAt,
            Source = audit.Source,
            CorrelationId = audit.CorrelationId,
            SessionId = audit.SessionId,
            RequestId = audit.RequestId,
            PreviousState = audit.PreviousState,
            CurrentState = audit.CurrentState,
            Metadata = audit.Metadata
        };

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return "system";
    }
}
