namespace AgencyOS.Domain.Entities;

public static class ContractType
{
    public const string FixedPrice = "Fixed Price";
    public const string TimeAndMaterial = "Time & Material";
    public const string MonthlyRetainer = "Monthly Retainer";
    public const string ManagedService = "Managed Service";
    public const string Subscription = "Subscription";
    public const string Other = "Other";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            FixedPrice,
            TimeAndMaterial,
            MonthlyRetainer,
            ManagedService,
            Subscription,
            Other
        };
}
