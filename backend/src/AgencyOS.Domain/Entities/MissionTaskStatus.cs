namespace AgencyOS.Domain.Entities;

public static class MissionTaskStatus
{
    public const string Draft = "Draft";
    public const string Planned = "Planned";
    public const string InProgress = "In Progress";
    public const string Blocked = "Blocked";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Draft,
            Planned,
            InProgress,
            Blocked,
            Completed,
            Cancelled
        };

    public static readonly IReadOnlySet<string> NonEditable =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Completed
        };
}
