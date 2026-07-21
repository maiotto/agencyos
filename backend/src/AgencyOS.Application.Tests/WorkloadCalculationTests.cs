using AgencyOS.Application.Services;

namespace AgencyOS.Application.Tests;

public class WorkloadCalculationTests
{
    [Fact]
    public void CalculateAverageHoursPerAssignment_ReturnsZeroWhenNoAssignments()
    {
        var average = WorkloadCalculation.CalculateAverageHoursPerAssignment(40m, 0);

        Assert.Equal(0m, average);
    }

    [Fact]
    public void CalculateAverageHoursPerAssignment_RoundsToTwoDecimals()
    {
        var average = WorkloadCalculation.CalculateAverageHoursPerAssignment(10m, 3);

        Assert.Equal(3.33m, average);
    }

    [Fact]
    public void CalculateWorkloadPercentage_ReturnsZeroWhenCapacityIsZero()
    {
        var workloadPercentage = WorkloadCalculation.CalculateWorkloadPercentage(0m, 10m);

        Assert.Equal(0m, workloadPercentage);
    }

    [Fact]
    public void CalculateWorkloadPercentage_DoesNotBecomeNegative()
    {
        var workloadPercentage = WorkloadCalculation.CalculateWorkloadPercentage(40m, 10m);

        Assert.Equal(25m, workloadPercentage);
    }
}
