namespace AgencyOS.Application.Services;

public static class WorkloadCalculation
{
    public static decimal CalculateAverageHoursPerAssignment(decimal totalPlannedHours, int assignmentCount)
    {
        if (assignmentCount <= 0)
        {
            return 0;
        }

        return decimal.Round(
            totalPlannedHours / assignmentCount,
            2,
            MidpointRounding.AwayFromZero);
    }

    public static decimal CalculateWorkloadPercentage(decimal totalCapacityHours, decimal totalPlannedHours)
    {
        var workloadPercentage = CapacityCalculation.CalculateUtilizationPercentage(
            totalCapacityHours,
            totalPlannedHours);

        return Math.Max(0, workloadPercentage);
    }
}
