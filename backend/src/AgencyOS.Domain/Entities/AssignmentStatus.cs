namespace AgencyOS.Domain.Entities;

public static class AssignmentStatus
{
    public const string Planned = "Planned";
    public const string Confirmed = "Confirmed";
    public const string InProgress = "In Progress";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Planned,
            Confirmed,
            InProgress,
            Completed,
            Cancelled
        };

    public static readonly IReadOnlySet<string> NonEditable =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Cancelled
        };
}
