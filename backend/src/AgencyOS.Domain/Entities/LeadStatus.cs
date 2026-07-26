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
            Won
        };

    public static readonly IReadOnlySet<string> Inactive =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Archived,
            Converted
        };

    private static readonly Dictionary<string, string> ForwardTransitions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [Prospect] = Qualified,
            [Qualified] = Proposal,
            [Proposal] = Negotiation,
            [Negotiation] = Won
        };

    public static bool IsActive(string status) => !Inactive.Contains(status);

    /// <summary>
    /// Validates Lead status changes applied through update flows.
    /// Won → Converted is excluded; conversion is performed only by the convert use case.
    /// </summary>
    public static bool CanTransition(string currentStatus, string newStatus)
    {
        if (string.Equals(currentStatus, newStatus, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(currentStatus, Converted, StringComparison.OrdinalIgnoreCase)
            || string.Equals(currentStatus, Lost, StringComparison.OrdinalIgnoreCase)
            || string.Equals(currentStatus, Archived, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (ForwardTransitions.TryGetValue(currentStatus, out var nextStatus)
            && string.Equals(nextStatus, newStatus, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(newStatus, Lost, StringComparison.OrdinalIgnoreCase)
            && string.Equals(currentStatus, Won, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(newStatus, Archived, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}
