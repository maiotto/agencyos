namespace AgencyOS.Domain.Entities;

/// <summary>
/// Portfolio health bands (US-109).
/// </summary>
public static class PortfolioHealth
{
    public const string Unknown = "Unknown";
    public const string Underutilized = "Underutilized";
    public const string Healthy = "Healthy";
    public const string AtRisk = "AtRisk";
    public const string Overloaded = "Overloaded";

    public static readonly IReadOnlyCollection<string> All =
    [
        Unknown,
        Underutilized,
        Healthy,
        AtRisk,
        Overloaded
    ];

    public static string Canonicalize(string health)
    {
        if (string.IsNullOrWhiteSpace(health))
        {
            return Unknown;
        }

        var trimmed = health.Trim();
        var match = All.FirstOrDefault(candidate =>
            string.Equals(candidate, trimmed, StringComparison.OrdinalIgnoreCase));

        return match ?? Unknown;
    }

    /// <summary>
    /// Deterministic health from utilization and workload percentages (BR-912).
    /// </summary>
    public static string Calculate(
        decimal utilizationPercentage,
        decimal workloadPercentage,
        decimal? warningPercentage)
    {
        var peak = Math.Max(utilizationPercentage, workloadPercentage);
        var warning = warningPercentage ?? 85m;

        if (peak > 100m)
        {
            return Overloaded;
        }

        if (peak >= warning)
        {
            return AtRisk;
        }

        if (peak < 40m)
        {
            return Underutilized;
        }

        return Healthy;
    }
}
