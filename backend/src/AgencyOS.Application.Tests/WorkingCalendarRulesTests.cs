using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class WorkingCalendarRulesTests
{
    [Fact]
    public void HasValidPeriod_RejectsEffectiveToEarlierThanEffectiveFrom()
    {
        Assert.False(WorkingCalendarRules.HasValidPeriod(
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 7, 1)));
    }

    [Fact]
    public void HasValidPeriod_AcceptsOpenEndedPeriod()
    {
        Assert.True(WorkingCalendarRules.HasValidPeriod(new DateOnly(2026, 8, 1), null));
    }

    [Fact]
    public void HasAtLeastOneWorkingDay_RejectsEmptyCollection()
    {
        Assert.False(WorkingCalendarRules.HasAtLeastOneWorkingDay([]));
    }

    [Fact]
    public void HasDuplicateWorkingDays_DetectsCaseInsensitiveDuplicates()
    {
        Assert.True(WorkingCalendarRules.HasDuplicateWorkingDays(["Monday", "monday"]));
    }

    [Fact]
    public void PeriodsOverlap_DetectsOpenEndedOverlap()
    {
        Assert.True(WorkingCalendarRules.PeriodsOverlap(
            new DateOnly(2026, 1, 1),
            null,
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 12, 31)));
    }

    [Fact]
    public void PeriodsOverlap_ReturnsFalseWhenPeriodsAreAdjacentWithoutOverlap()
    {
        Assert.False(WorkingCalendarRules.PeriodsOverlap(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 6, 30),
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 12, 31)));
    }

    [Fact]
    public void IsHistorical_WhenEffectiveFromIsTodayOrEarlier()
    {
        var today = new DateOnly(2026, 7, 26);

        Assert.True(WorkingCalendarRules.IsHistorical(today, today));
        Assert.False(WorkingCalendarRules.IsHistorical(today.AddDays(1), today));
    }

    [Fact]
    public void NormalizeWorkingDays_CanonicalizesOrdersAndDeduplicates()
    {
        var normalized = WorkingCalendarRules.NormalizeWorkingDays(
            [" friday ", "Monday", "MONDAY", "Wednesday"]);

        Assert.Equal(new[] { "Monday", "Wednesday", "Friday" }, normalized);
    }

    [Fact]
    public void Create_PersistsInactiveCalendarWithNormalizedDays()
    {
        var calendar = WorkingCalendar.Create(
            Guid.NewGuid(),
            "  Standard Week  ",
            new DateOnly(2026, 8, 1),
            null,
            ["monday", "Friday"],
            DateTimeOffset.UtcNow);

        Assert.Equal(WorkingCalendarStatus.Inactive, calendar.Status);
        Assert.Equal("Standard Week", calendar.Name);
        Assert.Equal(new[] { "Monday", "Friday" }, calendar.WorkingDays);
    }

    [Fact]
    public void Reconfigure_ThrowsWhenCalendarIsHistorical()
    {
        var calendar = WorkingCalendar.Create(
            Guid.NewGuid(),
            "Historical",
            new DateOnly(2026, 1, 1),
            null,
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            calendar.Reconfigure(
                "Changed",
                new DateOnly(2026, 2, 1),
                null,
                WorkingDayNames.DefaultWeekdays,
                new DateOnly(2026, 7, 26),
                DateTimeOffset.UtcNow));

        Assert.Contains("Historical planning", exception.Message);
    }

    [Fact]
    public void EnsureCanDelete_ThrowsWhenActive()
    {
        var calendar = WorkingCalendar.Create(
            Guid.NewGuid(),
            "Future",
            new DateOnly(2026, 12, 1),
            null,
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow);

        calendar.Activate(DateTimeOffset.UtcNow);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            calendar.EnsureCanDelete(new DateOnly(2026, 7, 26)));

        Assert.Contains("inactive", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IsWorkingDay_UsesConfiguredWorkingDaysWithinValidity()
    {
        var calendar = WorkingCalendar.Create(
            Guid.NewGuid(),
            "Weekdays",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31),
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow);

        Assert.True(calendar.IsWorkingDay(new DateOnly(2026, 8, 3)));
        Assert.False(calendar.IsWorkingDay(new DateOnly(2026, 8, 2)));
        Assert.False(calendar.IsWorkingDay(new DateOnly(2026, 9, 1)));
    }
}
