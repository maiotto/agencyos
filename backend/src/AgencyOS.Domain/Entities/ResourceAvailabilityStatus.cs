namespace AgencyOS.Domain.Entities;

public static class ResourceAvailabilityStatus
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Active,
            Inactive
        };

    public static bool IsActive(string status) =>
        string.Equals(status, Active, StringComparison.OrdinalIgnoreCase);

    public static bool IsInactive(string status) =>
        string.Equals(status, Inactive, StringComparison.OrdinalIgnoreCase);

    public static bool CanDelete(string status) => IsInactive(status);
}
