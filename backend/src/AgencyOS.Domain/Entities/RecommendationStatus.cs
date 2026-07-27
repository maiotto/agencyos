namespace AgencyOS.Domain.Entities;

/// <summary>
/// Recommendation persistence status (US-202 / BR-1104).
/// Approval lifecycle belongs to RecommendationWorkflow — not this status.
/// </summary>
public static class RecommendationStatus
{
    public const string Active = "Active";
    public const string Archived = "Archived";

    public static readonly IReadOnlyCollection<string> All =
    [
        Active,
        Archived
    ];

    public static string Canonicalize(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new InvalidOperationException("Recommendation status is mandatory.");
        }

        var trimmed = status.Trim();
        var match = All.FirstOrDefault(candidate =>
            string.Equals(candidate, trimmed, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            throw new InvalidOperationException(
                $"Recommendation status '{trimmed}' is not supported.");
        }

        return match;
    }

    public static bool IsActive(string status) =>
        string.Equals(Canonicalize(status), Active, StringComparison.Ordinal);

    public static bool IsArchived(string status) =>
        string.Equals(Canonicalize(status), Archived, StringComparison.Ordinal);
}
