namespace AgencyOS.Domain.Entities;

/// <summary>
/// Recommendation Approval Workflow statuses (US-201 / BR-1001..BR-1010).
/// </summary>
public static class RecommendationWorkflowStatus
{
    public const string Draft = "Draft";
    public const string PendingApproval = "PendingApproval";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
    public const string Reopened = "Reopened";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Draft,
            PendingApproval,
            Approved,
            Rejected,
            Cancelled,
            Reopened
        };

    public static bool IsKnown(string status) => All.Contains(status);

    public static bool IsDraft(string status) =>
        string.Equals(status, Draft, StringComparison.OrdinalIgnoreCase);

    public static bool IsPendingApproval(string status) =>
        string.Equals(status, PendingApproval, StringComparison.OrdinalIgnoreCase);

    public static bool IsApproved(string status) =>
        string.Equals(status, Approved, StringComparison.OrdinalIgnoreCase);

    public static bool IsRejected(string status) =>
        string.Equals(status, Rejected, StringComparison.OrdinalIgnoreCase);

    public static bool IsCancelled(string status) =>
        string.Equals(status, Cancelled, StringComparison.OrdinalIgnoreCase);

    public static bool IsReopened(string status) =>
        string.Equals(status, Reopened, StringComparison.OrdinalIgnoreCase);

    public static bool CanSubmit(string status) =>
        IsDraft(status) || IsReopened(status);

    public static bool CanApprove(string status) => IsPendingApproval(status);

    public static bool CanReject(string status) => IsPendingApproval(status);

    public static bool CanCancel(string status) =>
        IsDraft(status) || IsPendingApproval(status) || IsReopened(status);

    public static bool CanReopen(string status) => IsRejected(status);

    public static bool IsImmutable(string status) => IsApproved(status);

    public static string Canonicalize(string status)
    {
        var match = All.FirstOrDefault(item =>
            string.Equals(item, status, StringComparison.OrdinalIgnoreCase));

        return match ?? throw new InvalidOperationException($"Unknown recommendation workflow status '{status}'.");
    }

    public static bool ValidateTransition(string fromStatus, string toStatus)
    {
        if (string.Equals(fromStatus, toStatus, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (IsApproved(fromStatus) || IsCancelled(fromStatus))
        {
            return false;
        }

        return (IsDraft(fromStatus) && IsPendingApproval(toStatus))
            || (IsReopened(fromStatus) && IsPendingApproval(toStatus))
            || (IsPendingApproval(fromStatus) && IsApproved(toStatus))
            || (IsPendingApproval(fromStatus) && IsRejected(toStatus))
            || (IsDraft(fromStatus) && IsCancelled(toStatus))
            || (IsPendingApproval(fromStatus) && IsCancelled(toStatus))
            || (IsReopened(fromStatus) && IsCancelled(toStatus))
            || (IsRejected(fromStatus) && IsReopened(toStatus));
    }
}
