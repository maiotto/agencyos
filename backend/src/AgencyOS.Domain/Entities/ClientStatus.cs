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
}
