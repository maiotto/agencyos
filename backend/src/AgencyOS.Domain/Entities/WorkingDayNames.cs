namespace AgencyOS.Domain.Entities;

public static class WorkingDayNames
{
    public const string Monday = "Monday";
    public const string Tuesday = "Tuesday";
    public const string Wednesday = "Wednesday";
    public const string Thursday = "Thursday";
    public const string Friday = "Friday";
    public const string Saturday = "Saturday";
    public const string Sunday = "Sunday";

    public static readonly IReadOnlyList<string> Ordered =
    [
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday
    ];

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(Ordered, StringComparer.OrdinalIgnoreCase);

    public static readonly IReadOnlyList<string> DefaultWeekdays =
    [
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday
    ];

    public static string Canonicalize(string dayOfWeek)
    {
        var trimmed = dayOfWeek.Trim();
        return Ordered.FirstOrDefault(day =>
            string.Equals(day, trimmed, StringComparison.OrdinalIgnoreCase)) ?? trimmed;
    }

    public static DayOfWeek ToDayOfWeek(string dayOfWeek)
    {
        var canonical = Canonicalize(dayOfWeek);

        return canonical switch
        {
            Monday => DayOfWeek.Monday,
            Tuesday => DayOfWeek.Tuesday,
            Wednesday => DayOfWeek.Wednesday,
            Thursday => DayOfWeek.Thursday,
            Friday => DayOfWeek.Friday,
            Saturday => DayOfWeek.Saturday,
            Sunday => DayOfWeek.Sunday,
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, "Unknown working day.")
        };
    }

    public static string FromDayOfWeek(DayOfWeek dayOfWeek) =>
        dayOfWeek switch
        {
            DayOfWeek.Monday => Monday,
            DayOfWeek.Tuesday => Tuesday,
            DayOfWeek.Wednesday => Wednesday,
            DayOfWeek.Thursday => Thursday,
            DayOfWeek.Friday => Friday,
            DayOfWeek.Saturday => Saturday,
            DayOfWeek.Sunday => Sunday,
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, "Unknown day of week.")
        };

    public static bool IsWorkingDay(DateOnly date, IReadOnlyCollection<string> workingDays)
    {
        var dayName = FromDayOfWeek(date.DayOfWeek);
        return workingDays.Any(day => string.Equals(day, dayName, StringComparison.OrdinalIgnoreCase));
    }
}
