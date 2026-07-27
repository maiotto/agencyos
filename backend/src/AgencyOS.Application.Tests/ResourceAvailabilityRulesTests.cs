using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class ResourceAvailabilityRulesTests
{
    [Fact]
    public void HasValidPeriod_RejectsEndBeforeStart()
    {
        Assert.False(ResourceAvailabilityRules.HasValidPeriod(
            new DateOnly(2026, 7, 10),
            new DateOnly(2026, 7, 1)));
    }

    [Fact]
    public void PeriodsOverlap_DetectsOverlappingOpenEndedRanges()
    {
        Assert.True(ResourceAvailabilityRules.PeriodsOverlap(
            new DateOnly(2026, 1, 1),
            null,
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 12, 31)));
    }

    [Fact]
    public void CoversDate_IncludesBoundaries()
    {
        Assert.True(ResourceAvailabilityRules.CoversDate(
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 31),
            new DateOnly(2026, 7, 1)));
        Assert.True(ResourceAvailabilityRules.CoversDate(
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 31),
            new DateOnly(2026, 7, 31)));
        Assert.False(ResourceAvailabilityRules.CoversDate(
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 31),
            new DateOnly(2026, 8, 1)));
    }

    [Fact]
    public void Create_ActivatesWeeklyFlagsAndOverrideResolution()
    {
        var resourceId = Guid.NewGuid();
        var calendarId = Guid.NewGuid();
        var hoursId = Guid.NewGuid();

        var availability = ResourceAvailability.Create(
            resourceId,
            calendarId,
            hoursId,
            "RA",
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 31),
            WorkingDayNames.Ordered.Select(day => new ResourceAvailabilityWeekDayDefinition
            {
                DayOfWeek = day,
                Enabled = day is "Monday" or "Tuesday" or "Wednesday" or "Thursday" or "Friday"
            }).ToList(),
            [
                new ResourceAvailabilityDayOverrideDefinition
                {
                    OverrideDate = new DateOnly(2026, 7, 6),
                    Available = false
                }
            ],
            DateTimeOffset.UtcNow);

        availability.Activate(DateTimeOffset.UtcNow);

        Assert.True(availability.IsAvailableOn(new DateOnly(2026, 7, 1)));
        Assert.False(availability.IsAvailableOn(new DateOnly(2026, 7, 4)));
        Assert.False(availability.IsAvailableOn(new DateOnly(2026, 7, 6)));
    }

    [Fact]
    public void Create_RequiresExecutionResource()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ResourceAvailability.Create(
                Guid.Empty,
                Guid.NewGuid(),
                Guid.NewGuid(),
                "RA",
                new DateOnly(2026, 7, 1),
                null,
                WorkingDayNames.Ordered.Select(day => new ResourceAvailabilityWeekDayDefinition
                {
                    DayOfWeek = day,
                    Enabled = day == "Monday"
                }).ToList(),
                [],
                DateTimeOffset.UtcNow));

        Assert.Contains("Execution Resource is mandatory", ex.Message);
    }
}
