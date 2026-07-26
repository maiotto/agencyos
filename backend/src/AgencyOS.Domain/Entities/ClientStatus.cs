namespace AgencyOS.Domain.Entities;

public static class ClientStatus
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Active,
            Inactive
        };

    /// <summary>
    /// Returns the canonical ClientStatus constant for a valid status value.
    /// </summary>
    public static string Normalize(string status)
    {
        if (string.Equals(status, Active, StringComparison.OrdinalIgnoreCase))
        {
            return Active;
        }

        if (string.Equals(status, Inactive, StringComparison.OrdinalIgnoreCase))
        {
            return Inactive;
        }

        throw new ArgumentOutOfRangeException(nameof(status), status, "Status must be a valid Client status.");
    }
}
