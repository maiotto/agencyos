namespace AgencyOS.Domain.Entities;

/// <summary>
/// Domain rules for Resource Availability (US-104 / BR-401..BR-410).
/// </summary>
public static class ResourceAvailabilityRules
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

    public static void ValidateOverrideTimes(bool available, TimeOnly? startTime, TimeOnly? endTime)
    {
        if (!available)
        {
            return;
        }

        if (startTime.HasValue != endTime.HasValue)
        {
            throw new InvalidOperationException("Override StartTime and EndTime must both be provided or both omitted.");
        }

        if (startTime.HasValue && endTime.HasValue && startTime.Value >= endTime.Value)
        {
            throw new InvalidOperationException("Override StartTime must be earlier than EndTime.");
        }
    }
}
