namespace AgencyOS.Domain.Entities;

/// <summary>
/// Domain rules for Company Working Calendar (US-101 / BR-101..BR-109).
/// </summary>
public static class WorkingCalendarRules
{
    public static bool HasValidPeriod(DateOnly effectiveFrom, DateOnly? effectiveTo) =>
        !effectiveTo.HasValue || effectiveTo.Value >= effectiveFrom;

    public static bool HasAtLeastOneWorkingDay(IReadOnlyCollection<string>? workingDays) =>
        workingDays is { Count: > 0 };

    public static bool HasDuplicateWorkingDays(IReadOnlyCollection<string> workingDays)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var day in workingDays)
        {
            if (string.IsNullOrWhiteSpace(day))
            {
                continue;
            }

            if (!seen.Add(day.Trim()))
            {
                return true;
            }
        }

        return false;
    }

    public static bool PeriodsOverlap(
        DateOnly effectiveFromA,
        DateOnly? effectiveToA,
        DateOnly effectiveFromB,
        DateOnly? effectiveToB)
    {
        var endA = effectiveToA ?? DateOnly.MaxValue;
        var endB = effectiveToB ?? DateOnly.MaxValue;

        return effectiveFromA <= endB && effectiveFromB <= endA;
    }

    /// <summary>
    /// BR-109: calendars whose validity has already started must not be structurally modified.
    /// </summary>
    public static bool IsHistorical(DateOnly effectiveFrom, DateOnly asOfDate) =>
        effectiveFrom <= asOfDate;

    public static bool CoversDate(DateOnly effectiveFrom, DateOnly? effectiveTo, DateOnly date)
    {
        if (date < effectiveFrom)
        {
            return false;
        }

        return !effectiveTo.HasValue || date <= effectiveTo.Value;
    }

    public static IReadOnlyList<string> NormalizeWorkingDays(IReadOnlyCollection<string> workingDays)
    {
        return workingDays
            .Where(day => !string.IsNullOrWhiteSpace(day))
            .Select(WorkingDayNames.Canonicalize)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(day => Array.FindIndex(
                WorkingDayNames.Ordered.ToArray(),
                ordered => string.Equals(ordered, day, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
    }
}
