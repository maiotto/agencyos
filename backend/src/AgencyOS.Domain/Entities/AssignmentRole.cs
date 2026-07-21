namespace AgencyOS.Domain.Entities;

public static class AssignmentRole
{
    public const string Responsible = "Responsible";
    public const string Reviewer = "Reviewer";
    public const string Approver = "Approver";
    public const string Contributor = "Contributor";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Responsible,
            Reviewer,
            Approver,
            Contributor
        };
}
