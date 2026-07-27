namespace AgencyOS.Domain.Entities;

/// <summary>
/// Decision implementation status (US-205 / BR-1403).
/// Independent from DecisionStatus.
/// </summary>
public static class DecisionImplementationStatus
{
    public const string NotStarted = "NotStarted";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            NotStarted,
            InProgress,
            Completed,
            Cancelled
        };

    public static bool IsKnown(string status) => All.Contains(status);

    public static bool IsNotStarted(string status) =>
        string.Equals(status, NotStarted, StringComparison.OrdinalIgnoreCase);

    public static bool IsInProgress(string status) =>
        string.Equals(status, InProgress, StringComparison.OrdinalIgnoreCase);

    public static bool IsCompleted(string status) =>
        string.Equals(status, Completed, StringComparison.OrdinalIgnoreCase);

    public static bool IsCancelled(string status) =>
        string.Equals(status, Cancelled, StringComparison.OrdinalIgnoreCase);

    public static bool CanStart(string status) => IsNotStarted(status);

    public static bool CanComplete(string status) => IsInProgress(status);

    public static bool CanCancel(string status) => IsNotStarted(status) || IsInProgress(status);

    public static string Canonicalize(string status)
    {
        var match = All.FirstOrDefault(item =>
            string.Equals(item, status, StringComparison.OrdinalIgnoreCase));

        return match
            ?? throw new InvalidOperationException($"Unknown decision implementation status '{status}'.");
    }
}
