namespace AgencyOS.Application.DTOs;

public class NotificationResponse
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string SourceEntity { get; set; } = string.Empty;

    public Guid? SourceEntityId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ReadAt { get; set; }

    public bool Archived { get; set; }

    /// <summary>Frontend navigation path derived from SourceEntity / SourceEntityId (BR-2907).</summary>
    public string? NavigationPath { get; set; }
}

public class NotificationQueryParameters
{
    public Guid? CompanyId { get; set; }

    public string? UserId { get; set; }

    public string? Category { get; set; }

    public string? Priority { get; set; }

    public string? Status { get; set; }

    public bool? Archived { get; set; }

    public string? SourceEntity { get; set; }

    public Guid? SourceEntityId { get; set; }

    public DateTimeOffset? CreatedFrom { get; set; }

    public DateTimeOffset? CreatedTo { get; set; }

    public string? Search { get; set; }

    public string? OrderBy { get; set; }

    public string? OrderDirection { get; set; }
}

public class NotificationGenerationRequest
{
    public Guid? CompanyId { get; set; }

    public string? UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string SourceEntity { get; set; } = string.Empty;

    public Guid? SourceEntityId { get; set; }
}

public class NotificationUnreadCountResponse
{
    public int UnreadCount { get; set; }
}
