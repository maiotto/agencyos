namespace AgencyOS.Domain.Entities;

/// <summary>
/// In-application Notification aggregate (US-506 / BR-2901..BR-2910).
/// Informational only. Never modifies business data. Immutable except read/archive status.
/// </summary>
public class Notification
{
    public Guid Id { get; private set; }

    public Guid CompanyId { get; private set; }

    public string UserId { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public string Category { get; private set; } = string.Empty;

    public string Priority { get; private set; } = string.Empty;

    public string Status { get; private set; } = string.Empty;

    public string SourceEntity { get; private set; } = string.Empty;

    public Guid? SourceEntityId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ReadAt { get; private set; }

    public bool Archived { get; private set; }

    public bool IsUnread => NotificationStatus.IsUnread(Status);

    public bool IsRead => NotificationStatus.IsRead(Status);

    private Notification()
    {
    }

    public static Notification Create(
        Guid companyId,
        string userId,
        string title,
        string message,
        string category,
        string priority,
        string sourceEntity,
        Guid? sourceEntityId,
        DateTimeOffset createdAt)
    {
        ValidateInputs(companyId, userId, title, message, category, priority, sourceEntity, sourceEntityId);

        return new Notification
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            UserId = userId.Trim(),
            Title = title.Trim(),
            Message = message.Trim(),
            Category = category.Trim(),
            Priority = priority.Trim(),
            Status = NotificationStatus.Unread,
            SourceEntity = sourceEntity.Trim(),
            SourceEntityId = sourceEntityId,
            CreatedAt = createdAt,
            ReadAt = null,
            Archived = false
        };
    }

    public void MarkRead(DateTimeOffset readAt)
    {
        EnsureNotArchived();

        if (IsRead)
        {
            return;
        }

        Status = NotificationStatus.Read;
        ReadAt = readAt;
    }

    public void MarkUnread()
    {
        EnsureNotArchived();

        if (IsUnread)
        {
            return;
        }

        Status = NotificationStatus.Unread;
        ReadAt = null;
    }

    public void Archive()
    {
        if (Archived)
        {
            throw new InvalidOperationException("Notification is already archived.");
        }

        Archived = true;
    }

    public bool Validate()
    {
        try
        {
            ValidateInputs(CompanyId, UserId, Title, Message, Category, Priority, SourceEntity, SourceEntityId);
            return NotificationStatus.IsKnown(Status);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private void EnsureNotArchived()
    {
        if (Archived)
        {
            throw new InvalidOperationException("Archived notifications cannot change read status.");
        }
    }

    private static void ValidateInputs(
        Guid companyId,
        string userId,
        string title,
        string message,
        string category,
        string priority,
        string sourceEntity,
        Guid? sourceEntityId)
    {
        if (companyId == Guid.Empty)
        {
            throw new InvalidOperationException("CompanyId is mandatory (BR-2906).");
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("UserId is mandatory (BR-2901).");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException("Title is mandatory.");
        }

        if (title.Trim().Length > 200)
        {
            throw new InvalidOperationException("Title cannot exceed 200 characters.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new InvalidOperationException("Message is mandatory.");
        }

        if (message.Trim().Length > 2000)
        {
            throw new InvalidOperationException("Message cannot exceed 2000 characters.");
        }

        if (!NotificationCategory.IsKnown(category))
        {
            throw new InvalidOperationException(
                $"Category must be one of: {string.Join(", ", NotificationCategory.All)}.");
        }

        if (!NotificationPriority.IsKnown(priority))
        {
            throw new InvalidOperationException(
                $"Priority must be one of: {string.Join(", ", NotificationPriority.All)}.");
        }

        if (!NotificationSourceEntities.IsKnown(sourceEntity))
        {
            throw new InvalidOperationException(
                $"SourceEntity must be one of: {string.Join(", ", NotificationSourceEntities.All)}.");
        }

        if (sourceEntityId.HasValue && sourceEntityId.Value == Guid.Empty)
        {
            throw new InvalidOperationException("SourceEntityId cannot be an empty GUID.");
        }
    }
}

public static class NotificationStatus
{
    public const string Unread = "Unread";
    public const string Read = "Read";

    public static readonly IReadOnlyList<string> All =
        new List<string> { Unread, Read };

    public static bool IsKnown(string status) =>
        !string.IsNullOrWhiteSpace(status)
        && All.Any(item => string.Equals(item, status.Trim(), StringComparison.OrdinalIgnoreCase));

    public static bool IsUnread(string status) =>
        string.Equals(status, Unread, StringComparison.OrdinalIgnoreCase);

    public static bool IsRead(string status) =>
        string.Equals(status, Read, StringComparison.OrdinalIgnoreCase);
}

public static class NotificationPriority
{
    public const string Low = "Low";
    public const string Medium = "Medium";
    public const string High = "High";
    public const string Critical = "Critical";

    public static readonly IReadOnlyList<string> All =
        new List<string> { Low, Medium, High, Critical };

    public static bool IsKnown(string priority) =>
        !string.IsNullOrWhiteSpace(priority)
        && All.Any(item => string.Equals(item, priority.Trim(), StringComparison.OrdinalIgnoreCase));
}

public static class NotificationCategory
{
    public const string Recommendation = "Recommendation";
    public const string Decision = "Decision";
    public const string Capacity = "Capacity";
    public const string Portfolio = "Portfolio";
    public const string Audit = "Audit";
    public const string Planning = "Planning";
    public const string Executive = "Executive";
    public const string Company = "Company";
    public const string System = "System";

    public static readonly IReadOnlyList<string> All =
        new List<string>
        {
            Recommendation,
            Decision,
            Capacity,
            Portfolio,
            Audit,
            Planning,
            Executive,
            Company,
            System
        };

    public static bool IsKnown(string category) =>
        !string.IsNullOrWhiteSpace(category)
        && All.Any(item => string.Equals(item, category.Trim(), StringComparison.OrdinalIgnoreCase));
}

public static class NotificationSourceEntities
{
    public const string RecommendationWorkflow = "RecommendationWorkflow";
    public const string Decision = "Decision";
    public const string CapacityHistory = "CapacityHistory";
    public const string Portfolio = "Portfolio";
    public const string AuditEvent = "AuditEvent";
    public const string PlanningWorkspace = "PlanningWorkspace";
    public const string RecommendationWorkspace = "RecommendationWorkspace";
    public const string DecisionWorkspace = "DecisionWorkspace";
    public const string ExecutiveWorkspace = "ExecutiveWorkspace";
    public const string Company = "Company";
    public const string CompanyContext = "CompanyContext";
    public const string System = "System";

    public static readonly IReadOnlyList<string> All =
        new List<string>
        {
            RecommendationWorkflow,
            Decision,
            CapacityHistory,
            Portfolio,
            AuditEvent,
            PlanningWorkspace,
            RecommendationWorkspace,
            DecisionWorkspace,
            ExecutiveWorkspace,
            Company,
            CompanyContext,
            System
        };

    public static bool IsKnown(string sourceEntity) =>
        !string.IsNullOrWhiteSpace(sourceEntity)
        && All.Any(item => string.Equals(item, sourceEntity.Trim(), StringComparison.OrdinalIgnoreCase));
}
