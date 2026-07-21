using AgencyOS.Application.Services;

namespace AgencyOS.Application.Tests;

public class CapacityCalculationTests
{
    [Fact]
    public void CalculateTotalCapacityHours_UsesWeeklyCapacityProportionalToPeriodDays()
    {
        var totalCapacity = CapacityCalculation.CalculateTotalCapacityHours(40m, 7);

        Assert.Equal(40m, totalCapacity);
    }

    [Fact]
    public void CalculateAvailableHours_DoesNotBecomeNegative()
    {
        var availableHours = CapacityCalculation.CalculateAvailableHours(40m, 50m);

        Assert.Equal(0m, availableHours);
    }

    [Fact]
    public void CalculateUtilizationPercentage_ReturnsZeroWhenCapacityIsZero()
    {
        var utilization = CapacityCalculation.CalculateUtilizationPercentage(0m, 10m);

        Assert.Equal(0m, utilization);
    }

    [Fact]
    public void CalculateUtilizationPercentage_RoundsToTwoDecimals()
    {
        var utilization = CapacityCalculation.CalculateUtilizationPercentage(40m, 10m);

        Assert.Equal(25m, utilization);
    }

    [Fact]
    public void GetInclusivePeriodDays_IncludesBothBoundaryDates()
    {
        var periodDays = CapacityCalculation.GetInclusivePeriodDays(
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 7));

        Assert.Equal(7, periodDays);
    }
}
