namespace AgencyOS.Domain.Entities;

/// <summary>
/// Immutable Decision timeline entry (US-205 / BR-1404).
/// </summary>
public class DecisionTimelineEntry
{
    public Guid Id { get; private set; }

    public Guid DecisionId { get; private set; }

    public string EventType { get; private set; } = string.Empty;

    public string FromDecisionStatus { get; private set; } = string.Empty;

    public string ToDecisionStatus { get; private set; } = string.Empty;

    public string FromImplementationStatus { get; private set; } = string.Empty;

    public string ToImplementationStatus { get; private set; } = string.Empty;

    public string Actor { get; private set; } = string.Empty;

    public string? Comment { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    private DecisionTimelineEntry()
    {
    }

    internal static DecisionTimelineEntry Create(
        Guid decisionId,
        string eventType,
        string fromDecisionStatus,
        string toDecisionStatus,
        string fromImplementationStatus,
        string toImplementationStatus,
        string actor,
        string? comment,
        DateTimeOffset occurredAt)
    {
        if (decisionId == Guid.Empty)
        {
            throw new InvalidOperationException("DecisionId is mandatory for timeline entries.");
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new InvalidOperationException("EventType is mandatory for timeline entries.");
        }

        if (string.IsNullOrWhiteSpace(actor))
        {
            throw new InvalidOperationException("Actor is mandatory for timeline entries.");
        }

        return new DecisionTimelineEntry
        {
            Id = Guid.NewGuid(),
            DecisionId = decisionId,
            EventType = eventType.Trim(),
            FromDecisionStatus = DecisionStatus.Canonicalize(fromDecisionStatus),
            ToDecisionStatus = DecisionStatus.Canonicalize(toDecisionStatus),
            FromImplementationStatus = DecisionImplementationStatus.Canonicalize(fromImplementationStatus),
            ToImplementationStatus = DecisionImplementationStatus.Canonicalize(toImplementationStatus),
            Actor = actor.Trim(),
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            OccurredAt = occurredAt
        };
    }
}

public static class DecisionTimelineEventType
{
    public const string Created = "Created";
    public const string StartImplementation = "StartImplementation";
    public const string Complete = "Complete";
    public const string Cancel = "Cancel";
    public const string RecordOutcome = "RecordOutcome";
}
