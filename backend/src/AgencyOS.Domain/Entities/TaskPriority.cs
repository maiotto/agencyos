namespace AgencyOS.Domain.Entities;

public static class TaskPriority
{
    public const string Critical = "Critical";
    public const string High = "High";
    public const string Medium = "Medium";
    public const string Low = "Low";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Critical,
            High,
            Medium,
            Low
        };
}
