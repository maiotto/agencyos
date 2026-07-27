namespace AgencyOS.Domain.Entities;

/// <summary>
/// Resource Availability strategy referenced by Planning Templates (US-108).
/// Stores strategy identity only — does not duplicate RA operational rows.
/// </summary>
public static class ResourceAvailabilityStrategies
{
    /// <summary>
    /// Capacity/Workload planning requires Active Resource Availability covering the period.
    /// </summary>
    public const string RequireActiveConfiguration = "RequireActiveConfiguration";

    public static readonly IReadOnlyCollection<string> All =
    [
        RequireActiveConfiguration
    ];

    public static bool IsKnown(string strategy) =>
        All.Any(item => string.Equals(item, strategy, StringComparison.OrdinalIgnoreCase));

    public static string Canonicalize(string strategy)
    {
        var match = All.FirstOrDefault(item =>
            string.Equals(item, strategy, StringComparison.OrdinalIgnoreCase));

        return match
            ?? throw new InvalidOperationException(
                $"Unknown Resource Availability strategy '{strategy}'.");
    }
}
