namespace AgencyOS.Domain.Entities;

public static class ContractStatus
{
    public const string Draft = "Draft";
    public const string Active = "Active";
    public const string Suspended = "Suspended";
    public const string Closed = "Closed";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Draft,
            Active,
            Suspended,
            Closed,
            Cancelled
        };

    public static readonly IReadOnlySet<string> NonEditable =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Closed,
            Cancelled
        };

    public static readonly IReadOnlySet<string> Activatable =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Draft
        };

    public static readonly IReadOnlySet<string> Closable =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Active,
            Suspended
        };

    public static readonly IReadOnlySet<string> Cancellable =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Draft,
            Active,
            Suspended
        };
}
