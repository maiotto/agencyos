namespace AgencyOS.Domain.Entities;

public static class RankingDimension
{
    public const string EstimatedCost = "EstimatedCost";

    public const string EstimatedDuration = "EstimatedDuration";

    public const string CapacityUtilization = "CapacityUtilization";

    public const string OperationalRisk = "OperationalRisk";

    public const string HumanResourceUsage = "HumanResourceUsage";

    public const string AiResourceUsage = "AiResourceUsage";

    public const string ExternalResourceUsage = "ExternalResourceUsage";

    public const string AutomationUsage = "AutomationUsage";

    public static readonly IReadOnlyList<string> All =
    [
        EstimatedCost,
        EstimatedDuration,
        CapacityUtilization,
        OperationalRisk,
        HumanResourceUsage,
        AiResourceUsage,
        ExternalResourceUsage,
        AutomationUsage
    ];
}
