namespace AgencyOS.Domain.Entities;

/// <summary>
/// Immutable Audit Event aggregate (US-206 / BR-1501..BR-1510).
/// Create-only. No Update. No Delete.
/// </summary>
public class AuditEvent
{
    public Guid Id { get; private set; }

    public string EntityType { get; private set; } = string.Empty;

    public Guid EntityId { get; private set; }

    public string? EntityVersion { get; private set; }

    public string EventType { get; private set; } = string.Empty;

    public string Action { get; private set; } = string.Empty;

    public Guid? CompanyId { get; private set; }

    public string UserId { get; private set; } = string.Empty;

    public string UserName { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; private set; }

    public string Source { get; private set; } = string.Empty;

    public Guid? CorrelationId { get; private set; }

    public string? SessionId { get; private set; }

    public string? RequestId { get; private set; }

    public string? PreviousState { get; private set; }

    public string? CurrentState { get; private set; }

    public string? Metadata { get; private set; }

    private AuditEvent()
    {
    }

    public static AuditEvent Create(
        string entityType,
        Guid entityId,
        string? entityVersion,
        string eventType,
        string action,
        Guid? companyId,
        string userId,
        string userName,
        DateTimeOffset occurredAt,
        string source,
        Guid? correlationId,
        string? sessionId,
        string? requestId,
        string? previousState,
        string? currentState,
        string? metadata)
    {
        if (string.IsNullOrWhiteSpace(entityType))
        {
            throw new InvalidOperationException("EntityType is mandatory.");
        }

        if (entityId == Guid.Empty)
        {
            throw new InvalidOperationException("EntityId is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new InvalidOperationException("EventType is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            throw new InvalidOperationException("Action is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("UserId is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new InvalidOperationException("UserName is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(source))
        {
            throw new InvalidOperationException("Source is mandatory.");
        }

        return new AuditEvent
        {
            Id = Guid.NewGuid(),
            EntityType = entityType.Trim(),
            EntityId = entityId,
            EntityVersion = string.IsNullOrWhiteSpace(entityVersion) ? null : entityVersion.Trim(),
            EventType = eventType.Trim(),
            Action = action.Trim(),
            CompanyId = companyId == Guid.Empty ? null : companyId,
            UserId = userId.Trim(),
            UserName = userName.Trim(),
            OccurredAt = occurredAt,
            Source = source.Trim(),
            CorrelationId = correlationId == Guid.Empty ? null : correlationId,
            SessionId = string.IsNullOrWhiteSpace(sessionId) ? null : sessionId.Trim(),
            RequestId = string.IsNullOrWhiteSpace(requestId) ? null : requestId.Trim(),
            PreviousState = NormalizeJson(previousState),
            CurrentState = NormalizeJson(currentState),
            Metadata = NormalizeJson(metadata)
        };
    }

    private static string? NormalizeJson(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public static class AuditEntityTypes
{
    public const string Recommendation = "Recommendation";
    public const string RecommendationWorkflow = "RecommendationWorkflow";
    public const string Decision = "Decision";
    public const string PlanningTemplate = "PlanningTemplate";
    public const string Portfolio = "Portfolio";
    public const string CapacityHistory = "CapacityHistory";
    public const string WorkloadHistory = "WorkloadHistory";
    public const string AIRecommendation = "AIRecommendation";
    public const string Explainability = "Explainability";
    public const string ExecutiveRecommendationSummary = "ExecutiveRecommendationSummary";
    public const string CompanyDecisionProfile = "CompanyDecisionProfile";
    public const string Company = "Company";
    public const string CrossPortfolioPlan = "CrossPortfolioPlan";
    public const string Notification = "Notification";
    public const string PersonalProductivityDashboard = "PersonalProductivityDashboard";
}

public static class AuditEventTypes
{
    public const string Created = "Created";
    public const string VersionCreated = "VersionCreated";
    public const string Archived = "Archived";
    public const string Restored = "Restored";
    public const string WorkflowTransition = "WorkflowTransition";
    public const string StatusChanged = "StatusChanged";
    public const string OutcomeRecorded = "OutcomeRecorded";
    public const string Applied = "Applied";
    public const string Activated = "Activated";
    public const string Deactivated = "Deactivated";
    public const string Calculated = "Calculated";
    public const string Executed = "Executed";
    public const string Simulated = "Simulated";
}

public static class AuditSources
{
    public const string Api = "Api";
    public const string DecisionEngine = "DecisionEngine";
    public const string System = "System";
}
