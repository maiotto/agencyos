namespace AgencyOS.Domain.Entities;

public static class ExecutionResourceStatus
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Active,
            Inactive
        };

    public static bool CanReceiveAssignments(string status) =>
        string.Equals(status, Active, StringComparison.OrdinalIgnoreCase);
}
