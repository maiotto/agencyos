using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

/// <summary>
/// Deterministic capacity math driven by operational calendar configuration (US-105 / BR-501..BR-509).
/// </summary>
public static class CapacityCalculation
{
    public static int GetInclusivePeriodDays(DateOnly periodStartDate, DateOnly periodEndDate)
    {
        return periodEndDate.DayNumber - periodStartDate.DayNumber + 1;
    }

    public static IEnumerable<DateOnly> EnumerateDates(DateOnly periodStartDate, DateOnly periodEndDate)
    {
        for (var date = periodStartDate; date <= periodEndDate; date = date.AddDays(1))
        {
            yield return date;
        }
    }

    /// <summary>
    /// Resolves planned net hours for an operational day from working-hours schedule and optional RA override.
    /// Returns null when the day is not productive.
    /// </summary>
    public static decimal? ResolvePlannedNetHours(
        WorkingHoursDay? scheduleDay,
        ResourceAvailabilityDayOverride? dayOverride)
    {
        if (dayOverride is not null && !dayOverride.Available)
        {
            return null;
        }

        if (dayOverride is { Available: true, StartTime: not null, EndTime: not null })
        {
            return WorkingHoursRules.CalculateNetHours(
                dayOverride.StartTime.Value,
                dayOverride.EndTime.Value,
                breakStart: null,
                breakEnd: null);
        }

        if (scheduleDay is null || !scheduleDay.Enabled)
        {
            return null;
        }

        return scheduleDay.NetHours;
    }

    public static decimal SumPlannedCapacityHours(IEnumerable<decimal> dailyPlannedHours) =>
        dailyPlannedHours.Sum();

    public static decimal CalculateAvailableHours(decimal totalCapacityHours, decimal allocatedHours)
    {
        return Math.Max(0, totalCapacityHours - allocatedHours);
    }

    public static decimal CalculateUtilizationPercentage(decimal totalCapacityHours, decimal allocatedHours)
    {
        if (totalCapacityHours <= 0)
        {
            return 0;
        }

        return decimal.Round((allocatedHours / totalCapacityHours) * 100, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Prorates weekly hours across calendar days for Workload percentage only.
    /// Capacity Engine (US-105) must use operational planned hours instead.
    /// </summary>
    public static decimal ProrateWeeklyHours(decimal capacityHoursPerWeek, int periodDays)
    {
        return capacityHoursPerWeek * (periodDays / 7.0m);
    }
}
