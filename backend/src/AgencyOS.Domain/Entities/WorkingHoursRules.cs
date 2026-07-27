namespace AgencyOS.Domain.Entities;

/// <summary>
/// Domain rules for Standard Working Hours (US-103 / BR-301..BR-309).
/// </summary>
public static class WorkingHoursRules
{
    public static bool HasValidPeriod(DateOnly effectiveFrom, DateOnly? effectiveTo) =>
        !effectiveTo.HasValue || effectiveTo.Value >= effectiveFrom;

    public static bool IsHistorical(DateOnly effectiveFrom, DateOnly asOfDate) =>
        effectiveFrom <= asOfDate;

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

    public static bool CoversDate(DateOnly effectiveFrom, DateOnly? effectiveTo, DateOnly date)
    {
        if (date < effectiveFrom)
        {
            return false;
        }

        return !effectiveTo.HasValue || date <= effectiveTo.Value;
    }

    public static void ValidateDaySchedule(
        bool enabled,
        TimeOnly? startTime,
        TimeOnly? endTime,
        TimeOnly? breakStart,
        TimeOnly? breakEnd)
    {
        if (!enabled)
        {
            return;
        }

        if (startTime is null || endTime is null)
        {
            throw new InvalidOperationException("Enabled weekdays require StartTime and EndTime.");
        }

        if (startTime.Value >= endTime.Value)
        {
            throw new InvalidOperationException("StartTime must be earlier than EndTime.");
        }

        if (breakStart.HasValue != breakEnd.HasValue)
        {
            throw new InvalidOperationException("If BreakStart exists, BreakEnd is mandatory.");
        }

        if (breakStart.HasValue && breakEnd.HasValue)
        {
            if (breakStart.Value >= breakEnd.Value)
            {
                throw new InvalidOperationException("BreakStart must be earlier than BreakEnd.");
            }

            if (breakStart.Value < startTime.Value || breakEnd.Value > endTime.Value)
            {
                throw new InvalidOperationException("Break period must be inside working period.");
            }
        }
    }

    public static decimal CalculateNetHours(
        TimeOnly startTime,
        TimeOnly endTime,
        TimeOnly? breakStart,
        TimeOnly? breakEnd)
    {
        var totalMinutes = (endTime.ToTimeSpan() - startTime.ToTimeSpan()).TotalMinutes;

        if (breakStart.HasValue && breakEnd.HasValue)
        {
            totalMinutes -= (breakEnd.Value.ToTimeSpan() - breakStart.Value.ToTimeSpan()).TotalMinutes;
        }

        return Math.Round((decimal)(totalMinutes / 60d), 2, MidpointRounding.AwayFromZero);
    }
}
