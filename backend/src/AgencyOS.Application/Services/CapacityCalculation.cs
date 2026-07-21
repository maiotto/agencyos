namespace AgencyOS.Application.Services;

public static class CapacityCalculation
{
    public static int GetInclusivePeriodDays(DateOnly periodStartDate, DateOnly periodEndDate)
    {
        return periodEndDate.DayNumber - periodStartDate.DayNumber + 1;
    }

    public static decimal CalculateTotalCapacityHours(decimal capacityHoursPerWeek, int periodDays)
    {
        return capacityHoursPerWeek * (periodDays / 7.0m);
    }

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
}
