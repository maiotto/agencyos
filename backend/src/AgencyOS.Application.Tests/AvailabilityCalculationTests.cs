using AgencyOS.Application.DTOs;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class AvailabilityCalculationTests
{
    private static readonly IReadOnlyList<string> Weekdays = WorkingDayNames.DefaultWeekdays;

    [Fact]
    public void IsWorkingDay_ReturnsTrueForConfiguredWeekdays()
    {
        Assert.True(AvailabilityCalculation.IsWorkingDay(new DateOnly(2026, 7, 6), Weekdays));
    }

    [Fact]
    public void IsWorkingDay_ReturnsFalseForWeekends()
    {
        Assert.False(AvailabilityCalculation.IsWorkingDay(new DateOnly(2026, 7, 4), Weekdays));
    }

    [Fact]
    public void IsWorkingDay_ThrowsWhenConfigurationMissing()
    {
        Assert.Throws<InvalidOperationException>(() =>
            AvailabilityCalculation.IsWorkingDay(new DateOnly(2026, 7, 6), Array.Empty<string>()));
    }

    [Fact]
    public void GetWorkingDaysInPeriod_ExcludesWeekends()
    {
        var workingDays = AvailabilityCalculation.GetWorkingDaysInPeriod(
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 7),
            Weekdays);

        Assert.Equal(5, workingDays.Count);
    }

    [Fact]
    public void CalculateAvailabilityPercentage_CapsAtOneHundred()
    {
        var percentage = AvailabilityCalculation.CalculateAvailabilityPercentage(40m, 50m);

        Assert.Equal(100m, percentage);
    }

    [Fact]
    public void CapAvailableHours_DoesNotExceedCapacity()
    {
        var availableHours = AvailabilityCalculation.CapAvailableHours(50m, 40m);

        Assert.Equal(40m, availableHours);
    }

    [Fact]
    public void BuildAvailableTimeSlots_SplitsSlotsWhenWorkingDaysAreNotContiguous()
    {
        var assignments = new List<WorkloadAssignmentDistributionItem>
        {
            new()
            {
                AssignmentId = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                AssignmentRole = "Responsible",
                PlannedHours = 8m,
                PlannedStartDate = new DateOnly(2026, 7, 6),
                PlannedEndDate = new DateOnly(2026, 7, 6),
                Status = "Planned"
            }
        };

        var dailyCapacity = CreateDailyCapacity(
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 10),
            8m);

        var timeSlots = AvailabilityCalculation.BuildAvailableTimeSlots(
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 10),
            dailyCapacity,
            assignments);

        Assert.Equal(2, timeSlots.Count);
        Assert.Equal(new DateOnly(2026, 7, 1), timeSlots[0].StartDate);
        Assert.Equal(new DateOnly(2026, 7, 3), timeSlots[0].EndDate);
        Assert.Equal(new DateOnly(2026, 7, 7), timeSlots[1].StartDate);
        Assert.Equal(new DateOnly(2026, 7, 10), timeSlots[1].EndDate);
    }

    [Fact]
    public void FindNextAvailableDate_ReturnsFirstWorkingDayWithCapacity()
    {
        var assignments = new List<WorkloadAssignmentDistributionItem>
        {
            new()
            {
                AssignmentId = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                AssignmentRole = "Responsible",
                PlannedHours = 8m,
                PlannedStartDate = new DateOnly(2026, 7, 1),
                PlannedEndDate = new DateOnly(2026, 7, 1),
                Status = "Planned"
            }
        };

        var dailyCapacity = CreateDailyCapacity(
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 7),
            8m);

        var nextAvailableDate = AvailabilityCalculation.FindNextAvailableDate(
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 7),
            dailyCapacity,
            assignments);

        Assert.Equal(new DateOnly(2026, 7, 2), nextAvailableDate);
    }

    private static Dictionary<DateOnly, decimal> CreateDailyCapacity(
        DateOnly start,
        DateOnly end,
        decimal hoursPerDay)
    {
        var result = new Dictionary<DateOnly, decimal>();

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (WorkingDayNames.IsWorkingDay(date, Weekdays))
            {
                result[date] = hoursPerDay;
            }
        }

        return result;
    }
}
