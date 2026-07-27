using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class NotificationQueryService : INotificationQueryService
{
    private readonly INotificationRepository _repository;
    private readonly ICompanyContext _companyContext;
    private readonly IAuditContext _auditContext;

    public NotificationQueryService(
        INotificationRepository repository,
        ICompanyContext companyContext,
        IAuditContext auditContext)
    {
        _repository = repository;
        _companyContext = companyContext;
        _auditContext = auditContext;
    }

    public async Task<IReadOnlyList<NotificationResponse>> GetAllAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var scoped = ApplyScope(parameters);
        var items = await _repository.QueryAsync(scoped, cancellationToken);
        return items.Select(NotificationMapper.ToResponse).ToList();
    }

    public Task<IReadOnlyList<NotificationResponse>> FilterAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default) =>
        GetAllAsync(parameters, cancellationToken);

    public async Task<NotificationResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await GetScopedOrThrowAsync(id, cancellationToken);
        return NotificationMapper.ToResponse(item);
    }

    public async Task<NotificationUnreadCountResponse> GetUnreadCountAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var scoped = ApplyScope(parameters);
        var companyId = scoped.CompanyId
            ?? throw new BusinessRuleException("CompanyId is required for unread count.");
        var userId = scoped.UserId
            ?? throw new BusinessRuleException("UserId is required for unread count.");

        var count = await _repository.CountUnreadAsync(companyId, userId, cancellationToken);
        return new NotificationUnreadCountResponse { UnreadCount = count };
    }

    private NotificationQueryParameters ApplyScope(NotificationQueryParameters parameters)
    {
        return new NotificationQueryParameters
        {
            CompanyId = parameters.CompanyId
                ?? _companyContext.CompanyId
                ?? AgencyOSCompanies.DefaultCompanyId,
            UserId = FirstNonEmpty(parameters.UserId, _auditContext.UserId, "system"),
            Category = parameters.Category,
            Priority = parameters.Priority,
            Status = parameters.Status,
            Archived = parameters.Archived,
            SourceEntity = parameters.SourceEntity,
            SourceEntityId = parameters.SourceEntityId,
            CreatedFrom = parameters.CreatedFrom,
            CreatedTo = parameters.CreatedTo,
            Search = parameters.Search,
            OrderBy = parameters.OrderBy,
            OrderDirection = parameters.OrderDirection
        };
    }

    private async Task<Notification> GetScopedOrThrowAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            throw new NotFoundException($"Notification with id '{id}' was not found.");
        }

        var companyId = _companyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId;
        var userId = FirstNonEmpty(_auditContext.UserId, "system");

        if (item.CompanyId != companyId || !string.Equals(item.UserId, userId, StringComparison.Ordinal))
        {
            throw new NotFoundException($"Notification with id '{id}' was not found.");
        }

        return item;
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return string.Empty;
    }
}

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly ICompanyContext _companyContext;
    private readonly IAuditContext _auditContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository repository,
        ICompanyContext companyContext,
        IAuditContext auditContext,
        IAuditService auditService,
        ILogger<NotificationService> logger)
    {
        _repository = repository;
        _companyContext = companyContext;
        _auditContext = auditContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<NotificationResponse> MarkReadAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await GetScopedOrThrowAsync(id, cancellationToken);
        var previous = NotificationMapper.Snapshot(item);
        item.MarkRead(DateTimeOffset.UtcNow);
        var updated = await _repository.UpdateAsync(item, cancellationToken);
        await RecordActionAuditAsync(
            updated,
            AuditEventTypes.StatusChanged,
            "Notification.MarkRead",
            previous,
            cancellationToken);
        return NotificationMapper.ToResponse(updated);
    }

    public async Task<NotificationResponse> MarkUnreadAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await GetScopedOrThrowAsync(id, cancellationToken);
        var previous = NotificationMapper.Snapshot(item);
        item.MarkUnread();
        var updated = await _repository.UpdateAsync(item, cancellationToken);
        await RecordActionAuditAsync(
            updated,
            AuditEventTypes.StatusChanged,
            "Notification.MarkUnread",
            previous,
            cancellationToken);
        return NotificationMapper.ToResponse(updated);
    }

    public async Task<NotificationResponse> ArchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await GetScopedOrThrowAsync(id, cancellationToken);
        var previous = NotificationMapper.Snapshot(item);
        item.Archive();
        var updated = await _repository.UpdateAsync(item, cancellationToken);
        await RecordActionAuditAsync(
            updated,
            AuditEventTypes.Archived,
            "Notification.Archive",
            previous,
            cancellationToken);
        return NotificationMapper.ToResponse(updated);
    }

    private async Task RecordActionAuditAsync(
        Notification notification,
        string eventType,
        string action,
        string previousState,
        CancellationToken cancellationToken)
    {
        await _auditService.RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.Notification,
                EntityId = notification.Id,
                EventType = eventType,
                Action = action,
                CompanyId = notification.CompanyId,
                UserId = notification.UserId,
                UserName = notification.UserId,
                Source = AuditSources.Api,
                PreviousState = previousState,
                CurrentState = NotificationMapper.Snapshot(notification)
            },
            cancellationToken);

        _logger.LogInformation(
            "Notification action {Action} applied to {NotificationId} for User={UserId}",
            action,
            notification.Id,
            notification.UserId);
    }

    private async Task<Notification> GetScopedOrThrowAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            throw new NotFoundException($"Notification with id '{id}' was not found.");
        }

        var companyId = _companyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId;
        var userId = FirstNonEmpty(_auditContext.UserId, "system");

        if (item.CompanyId != companyId || !string.Equals(item.UserId, userId, StringComparison.Ordinal))
        {
            throw new NotFoundException($"Notification with id '{id}' was not found.");
        }

        return item;
    }

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

public class NotificationGenerationService : INotificationGenerationService
{
    private readonly INotificationRepository _repository;
    private readonly ICompanyContext _companyContext;
    private readonly IAuditContext _auditContext;
    private readonly ILogger<NotificationGenerationService> _logger;

    public NotificationGenerationService(
        INotificationRepository repository,
        ICompanyContext companyContext,
        IAuditContext auditContext,
        ILogger<NotificationGenerationService> logger)
    {
        _repository = repository;
        _companyContext = companyContext;
        _auditContext = auditContext;
        _logger = logger;
    }

    public async Task GenerateSafeAsync(
        NotificationGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var companyId = request.CompanyId
                ?? _companyContext.CompanyId
                ?? AgencyOSCompanies.DefaultCompanyId;
            var userId = FirstNonEmpty(request.UserId, _auditContext.UserId, "system");

            var notification = Notification.Create(
                companyId,
                userId,
                request.Title,
                request.Message,
                request.Category,
                request.Priority,
                request.SourceEntity,
                request.SourceEntityId,
                DateTimeOffset.UtcNow);

            if (!notification.Validate())
            {
                throw new InvalidOperationException("Notification failed domain validation.");
            }

            await _repository.AddAsync(notification, cancellationToken);

            _logger.LogInformation(
                "Notification generated {NotificationId} Category={Category} User={UserId} Source={SourceEntity}",
                notification.Id,
                notification.Category,
                notification.UserId,
                notification.SourceEntity);
        }
        catch (Exception ex)
        {
            // BR-2909: Notification generation failures never affect business transactions.
            _logger.LogWarning(
                ex,
                "Notification generation failed for Source={SourceEntity}/{SourceEntityId}; business transaction continues.",
                request.SourceEntity,
                request.SourceEntityId);
        }
    }

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

internal static class NotificationMapper
{
    public static NotificationResponse ToResponse(Notification notification) =>
        new()
        {
            Id = notification.Id,
            CompanyId = notification.CompanyId,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            Category = notification.Category,
            Priority = notification.Priority,
            Status = notification.Status,
            SourceEntity = notification.SourceEntity,
            SourceEntityId = notification.SourceEntityId,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt,
            Archived = notification.Archived,
            NavigationPath = ResolveNavigationPath(notification.SourceEntity, notification.SourceEntityId)
        };

    public static string Snapshot(Notification notification) =>
        AuditService.SerializeState(new
        {
            notification.Id,
            notification.CompanyId,
            notification.UserId,
            notification.Title,
            notification.Category,
            notification.Priority,
            notification.Status,
            notification.SourceEntity,
            notification.SourceEntityId,
            notification.Archived,
            notification.ReadAt
        });

    public static string? ResolveNavigationPath(string sourceEntity, Guid? sourceEntityId)
    {
        if (string.Equals(sourceEntity, NotificationSourceEntities.RecommendationWorkflow, StringComparison.OrdinalIgnoreCase)
            && sourceEntityId.HasValue)
        {
            return $"/recommendations/workflow/{sourceEntityId.Value}";
        }

        if (string.Equals(sourceEntity, NotificationSourceEntities.Decision, StringComparison.OrdinalIgnoreCase)
            && sourceEntityId.HasValue)
        {
            return $"/decisions/{sourceEntityId.Value}";
        }

        if (string.Equals(sourceEntity, NotificationSourceEntities.CapacityHistory, StringComparison.OrdinalIgnoreCase)
            && sourceEntityId.HasValue)
        {
            return $"/capacity-history/{sourceEntityId.Value}";
        }

        if (string.Equals(sourceEntity, NotificationSourceEntities.Portfolio, StringComparison.OrdinalIgnoreCase)
            && sourceEntityId.HasValue)
        {
            return $"/portfolios/{sourceEntityId.Value}";
        }

        if (string.Equals(sourceEntity, NotificationSourceEntities.AuditEvent, StringComparison.OrdinalIgnoreCase)
            && sourceEntityId.HasValue)
        {
            return $"/audit/{sourceEntityId.Value}";
        }

        if (string.Equals(sourceEntity, NotificationSourceEntities.Company, StringComparison.OrdinalIgnoreCase)
            && sourceEntityId.HasValue)
        {
            return $"/companies/{sourceEntityId.Value}";
        }

        if (string.Equals(sourceEntity, NotificationSourceEntities.PlanningWorkspace, StringComparison.OrdinalIgnoreCase))
        {
            return "/planning-workspace";
        }

        if (string.Equals(sourceEntity, NotificationSourceEntities.RecommendationWorkspace, StringComparison.OrdinalIgnoreCase))
        {
            return sourceEntityId.HasValue
                ? $"/recommendations/{sourceEntityId.Value}"
                : "/recommendation-workspace";
        }

        if (string.Equals(sourceEntity, NotificationSourceEntities.DecisionWorkspace, StringComparison.OrdinalIgnoreCase))
        {
            return sourceEntityId.HasValue
                ? $"/decisions/{sourceEntityId.Value}"
                : "/decision-workspace";
        }

        if (string.Equals(sourceEntity, NotificationSourceEntities.ExecutiveWorkspace, StringComparison.OrdinalIgnoreCase))
        {
            return "/executive-workspace";
        }

        if (string.Equals(sourceEntity, NotificationSourceEntities.CompanyContext, StringComparison.OrdinalIgnoreCase))
        {
            return "/companies";
        }

        return null;
    }
}
