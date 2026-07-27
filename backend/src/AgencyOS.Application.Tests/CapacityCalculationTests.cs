using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class CapacityCalculationTests
{
    [Fact]
    public void ResolvePlannedNetHours_UsesScheduleNetHoursWhenAvailable()
    {
        var schedule = CreateScheduleDay(
            enabled: true,
            new TimeOnly(9, 0),
            new TimeOnly(18, 0),
            new TimeOnly(12, 0),
            new TimeOnly(13, 0));

        var planned = CapacityCalculation.ResolvePlannedNetHours(schedule, dayOverride: null);

        Assert.Equal(8m, planned);
    }

    [Fact]
    public void ResolvePlannedNetHours_UsesOverrideTimesWhenProvided()
    {
        var schedule = CreateScheduleDay(
            enabled: true,
            new TimeOnly(9, 0),
            new TimeOnly(18, 0),
            null,
            null);

        var dayOverride = CreateOverride(
            new DateOnly(2026, 7, 1),
            available: true,
            new TimeOnly(9, 0),
            new TimeOnly(13, 0),
            notes: "Half day");

        var planned = CapacityCalculation.ResolvePlannedNetHours(schedule, dayOverride);

        Assert.Equal(4m, planned);
    }

    [Fact]
    public void ResolvePlannedNetHours_ReturnsNullWhenOverrideUnavailable()
    {
        var schedule = CreateScheduleDay(
            enabled: true,
            new TimeOnly(9, 0),
            new TimeOnly(17, 0),
            null,
            null);

        var dayOverride = CreateOverride(
            new DateOnly(2026, 7, 1),
            available: false,
            null,
            null,
            notes: "Unavailable");

        var planned = CapacityCalculation.ResolvePlannedNetHours(schedule, dayOverride);

        Assert.Null(planned);
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

    [Fact]
    public void EnumerateDates_IsDeterministic()
    {
        var dates = CapacityCalculation.EnumerateDates(
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 3)).ToList();

        Assert.Equal(
            new[]
            {
                new DateOnly(2026, 7, 1),
                new DateOnly(2026, 7, 2),
                new DateOnly(2026, 7, 3)
            },
            dates);
    }

    private static WorkingHoursDay CreateScheduleDay(
        bool enabled,
        TimeOnly? startTime,
        TimeOnly? endTime,
        TimeOnly? breakStart,
        TimeOnly? breakEnd)
    {
        var hours = WorkingHours.Create(
            Guid.NewGuid(),
            "Hours",
            new DateOnly(2026, 1, 1),
            null,
            [
                new WorkingHoursDayDefinition
                {
                    DayOfWeek = WorkingDayNames.Monday,
                    Enabled = enabled,
                    StartTime = startTime,
                    EndTime = endTime,
                    BreakStart = breakStart,
                    BreakEnd = breakEnd
                }
            ],
            DateTimeOffset.UtcNow);

        return hours.GetDay(WorkingDayNames.Monday)!;
    }

    private static ResourceAvailabilityDayOverride CreateOverride(
        DateOnly overrideDate,
        bool available,
        TimeOnly? startTime,
        TimeOnly? endTime,
        string? notes)
    {
        var availability = ResourceAvailability.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Availability",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            WorkingDayNames.Ordered
                .Select(day => new ResourceAvailabilityWeekDayDefinition
                {
                    DayOfWeek = day,
                    Enabled = true
                })
                .ToList(),
            [
                new ResourceAvailabilityDayOverrideDefinition
                {
                    OverrideDate = overrideDate,
                    Available = available,
                    StartTime = startTime,
                    EndTime = endTime,
                    Notes = notes
                }
            ],
            DateTimeOffset.UtcNow);

        return availability.GetOverride(overrideDate)!;
    }
}
