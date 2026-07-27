namespace AgencyOS.Domain.Entities;

/// <summary>
/// Decision lifecycle status (US-205 / BR-1401..BR-1407).
/// Independent from ImplementationStatus (BR-1403).
/// </summary>
public static class DecisionStatus
{
    public const string Created = "Created";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Created,
            InProgress,
            Completed,
            Cancelled
        };

    public static bool IsKnown(string status) => All.Contains(status);

    public static bool IsCreated(string status) =>
        string.Equals(status, Created, StringComparison.OrdinalIgnoreCase);

    public static bool IsInProgress(string status) =>
        string.Equals(status, InProgress, StringComparison.OrdinalIgnoreCase);

    public static bool IsCompleted(string status) =>
        string.Equals(status, Completed, StringComparison.OrdinalIgnoreCase);

    public static bool IsCancelled(string status) =>
        string.Equals(status, Cancelled, StringComparison.OrdinalIgnoreCase);

    public static bool CanStartImplementation(string status) => IsCreated(status);

    public static bool CanComplete(string status) => IsInProgress(status);

    public static bool CanCancel(string status) => IsCreated(status) || IsInProgress(status);

    public static string Canonicalize(string status)
    {
        var match = All.FirstOrDefault(item =>
            string.Equals(item, status, StringComparison.OrdinalIgnoreCase));

        return match ?? throw new InvalidOperationException($"Unknown decision status '{status}'.");
    }
}
