namespace AgencyOS.Domain.Entities;

public static class LeadStatus
{
    public const string Prospect = "Prospect";
    public const string Qualified = "Qualified";
    public const string Proposal = "Proposal";
    public const string Negotiation = "Negotiation";
    public const string Won = "Won";
    public const string Lost = "Lost";
    public const string Converted = "Converted";
    public const string Archived = "Archived";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Prospect,
            Qualified,
            Proposal,
            Negotiation,
            Won,
            Lost,
            Converted,
            Archived
        };

    public static readonly IReadOnlySet<string> Convertible =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Prospect,
            Qualified,
            Proposal,
            Negotiation,
            Won
        };

    public static readonly IReadOnlySet<string> Inactive =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Archived,
            Converted
        };

    public static bool IsActive(string status) => !Inactive.Contains(status);
}
