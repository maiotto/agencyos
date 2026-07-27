namespace AgencyOS.Domain.Entities;

/// <summary>
/// Portfolio Planning status (US-109 / BR-906 / BR-907).
/// </summary>
public static class PortfolioStatus
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";

    public static readonly IReadOnlyCollection<string> All = [Active, Inactive];

    public static string Canonicalize(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new InvalidOperationException("Portfolio status is mandatory.");
        }

        var trimmed = status.Trim();
        var match = All.FirstOrDefault(candidate =>
            string.Equals(candidate, trimmed, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            throw new InvalidOperationException($"Portfolio status '{trimmed}' is not supported.");
        }

        return match;
    }

    public static bool IsActive(string status) =>
        string.Equals(Canonicalize(status), Active, StringComparison.Ordinal);

    public static bool IsInactive(string status) =>
        string.Equals(Canonicalize(status), Inactive, StringComparison.Ordinal);

    public static bool CanModify(string status) => IsActive(status);

    public static bool CanDelete(string status) => IsInactive(status);
}
