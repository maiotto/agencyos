namespace AgencyOS.Domain.Entities;

public static class HolidayTypes
{
    public const string National = "National";
    public const string State = "State";
    public const string Municipal = "Municipal";
    public const string Company = "Company";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            National,
            State,
            Municipal,
            Company
        };

    public static string Canonicalize(string holidayType)
    {
        var trimmed = holidayType.Trim();
        return All.FirstOrDefault(type =>
            string.Equals(type, trimmed, StringComparison.OrdinalIgnoreCase)) ?? trimmed;
    }

    public static bool IsNational(string holidayType) =>
        string.Equals(holidayType, National, StringComparison.OrdinalIgnoreCase);

    public static bool IsState(string holidayType) =>
        string.Equals(holidayType, State, StringComparison.OrdinalIgnoreCase);

    public static bool IsMunicipal(string holidayType) =>
        string.Equals(holidayType, Municipal, StringComparison.OrdinalIgnoreCase);

    public static bool IsCompany(string holidayType) =>
        string.Equals(holidayType, Company, StringComparison.OrdinalIgnoreCase);
}
