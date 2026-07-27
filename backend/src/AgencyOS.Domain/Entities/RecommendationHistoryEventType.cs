namespace AgencyOS.Domain.Entities;

/// <summary>
/// Recommendation History event types (US-203).
/// </summary>
public static class RecommendationHistoryEventType
{
    public const string VersionCreated = "VersionCreated";
    public const string Archived = "Archived";
    public const string Restored = "Restored";
    public const string WorkflowTransition = "WorkflowTransition";

    public static readonly IReadOnlyCollection<string> All =
    [
        VersionCreated,
        Archived,
        Restored,
        WorkflowTransition
    ];

    public static string Canonicalize(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new InvalidOperationException("History event type is mandatory.");
        }

        var trimmed = eventType.Trim();
        var match = All.FirstOrDefault(candidate =>
            string.Equals(candidate, trimmed, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            throw new InvalidOperationException($"History event type '{trimmed}' is not supported.");
        }

        return match;
    }
}
