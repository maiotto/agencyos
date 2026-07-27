using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class WorkingHoursRulesTests
{
    [Fact]
    public void Create_RequiresWorkingCalendar()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            WorkingHours.Create(
                Guid.Empty,
                "Hours",
                new DateOnly(2099, 1, 1),
                null,
                CreateWeekdayDefinitions(),
                DateTimeOffset.UtcNow));

        Assert.Contains("Working Calendar", exception.Message);
    }

    [Fact]
    public void Create_RequiresAtLeastOneEnabledDay()
    {
        var days = WorkingDayNames.Ordered
            .Select(day => new WorkingHoursDayDefinition { DayOfWeek = day, Enabled = false })
            .ToList();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            WorkingHours.Create(
                Guid.NewGuid(),
                "Hours",
                new DateOnly(2099, 1, 1),
                null,
                days,
                DateTimeOffset.UtcNow));

        Assert.Contains("enabled weekday", exception.Message);
    }

    [Fact]
    public void Create_RejectsStartTimeAfterEndTime()
    {
        var days = new[]
        {
            new WorkingHoursDayDefinition
            {
                DayOfWeek = WorkingDayNames.Monday,
                Enabled = true,
                StartTime = new TimeOnly(18, 0),
                EndTime = new TimeOnly(9, 0)
            }
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            WorkingHours.Create(
                Guid.NewGuid(),
                "Hours",
                new DateOnly(2099, 1, 1),
                null,
                days,
                DateTimeOffset.UtcNow));

        Assert.Contains("StartTime must be earlier", exception.Message);
    }

    [Fact]
    public void Create_RejectsBreakOutsideWorkingPeriod()
    {
        var days = new[]
        {
            new WorkingHoursDayDefinition
            {
                DayOfWeek = WorkingDayNames.Monday,
                Enabled = true,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(18, 0),
                BreakStart = new TimeOnly(8, 0),
                BreakEnd = new TimeOnly(8, 30)
            }
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            WorkingHours.Create(
                Guid.NewGuid(),
                "Hours",
                new DateOnly(2099, 1, 1),
                null,
                days,
                DateTimeOffset.UtcNow));

        Assert.Contains("Break period must be inside", exception.Message);
    }

    [Fact]
    public void CalculateNetHours_SubtractsBreak()
    {
        var net = WorkingHoursRules.CalculateNetHours(
            new TimeOnly(9, 0),
            new TimeOnly(18, 0),
            new TimeOnly(12, 0),
            new TimeOnly(13, 0));

        Assert.Equal(8m, net);
    }

    [Fact]
    public void EnsureCanDelete_ThrowsWhenActive()
    {
        var hours = WorkingHours.Create(
            Guid.NewGuid(),
            "Hours",
            new DateOnly(2099, 1, 1),
            null,
            CreateWeekdayDefinitions(),
            DateTimeOffset.UtcNow);
        hours.Activate(DateTimeOffset.UtcNow);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            hours.EnsureCanDelete(new DateOnly(2026, 7, 26)));

        Assert.Contains("inactive", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetDay_ReturnsEnabledMondaySchedule()
    {
        var hours = WorkingHours.Create(
            Guid.NewGuid(),
            "Hours",
            new DateOnly(2099, 1, 1),
            null,
            CreateWeekdayDefinitions(),
            DateTimeOffset.UtcNow);

        var monday = hours.GetDay(WorkingDayNames.Monday);

        Assert.NotNull(monday);
        Assert.True(monday!.Enabled);
        Assert.Equal(8m, monday.NetHours);
    }

    private static IReadOnlyList<WorkingHoursDayDefinition> CreateWeekdayDefinitions() =>
        WorkingDayNames.DefaultWeekdays
            .Select(day => new WorkingHoursDayDefinition
            {
                DayOfWeek = day,
                Enabled = true,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(18, 0),
                BreakStart = new TimeOnly(12, 0),
                BreakEnd = new TimeOnly(13, 0)
            })
            .ToList();
}
