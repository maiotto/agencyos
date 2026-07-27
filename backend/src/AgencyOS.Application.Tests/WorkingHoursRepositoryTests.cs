using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Application.Tests;

public class WorkingHoursRepositoryTests
{
    [Fact]
    public async Task AddAndGetById_RoundTripsWithDays()
    {
        await using var context = CreateContext();
        var calendar = await SeedCalendarAsync(context);
        var repository = new WorkingHoursRepository(context);
        var hours = CreateHours(calendar.Id);

        await repository.AddAsync(hours);
        var loaded = await repository.GetByIdAsync(hours.Id);

        Assert.NotNull(loaded);
        Assert.Equal(hours.Name, loaded!.Name);
        Assert.Equal(7, loaded.Days.Count);
        Assert.Equal(5, loaded.Days.Count(day => day.Enabled));
    }

    [Fact]
    public async Task GetActiveOverlappingAsync_ReturnsOverlappingActive()
    {
        await using var context = CreateContext();
        var calendar = await SeedCalendarAsync(context);
        var repository = new WorkingHoursRepository(context);

        var active = CreateHours(calendar.Id);
        active.Activate(DateTimeOffset.UtcNow);
        await repository.AddAsync(active);

        var inactive = WorkingHours.Create(
            calendar.Id,
            "Inactive",
            new DateOnly(2099, 1, 1),
            new DateOnly(2099, 12, 31),
            CreateWeekdayDefinitions(),
            DateTimeOffset.UtcNow);
        await repository.AddAsync(inactive);

        var overlapping = await repository.GetActiveOverlappingAsync(
            calendar.Id,
            new DateOnly(2099, 6, 1),
            new DateOnly(2099, 6, 30));

        Assert.Single(overlapping);
        Assert.Equal(active.Id, overlapping[0].Id);
    }

    [Fact]
    public async Task DeleteAsync_RemovesConfiguration()
    {
        await using var context = CreateContext();
        var calendar = await SeedCalendarAsync(context);
        var repository = new WorkingHoursRepository(context);
        var hours = CreateHours(calendar.Id);
        await repository.AddAsync(hours);

        await repository.DeleteAsync(hours);

        Assert.Null(await repository.GetByIdAsync(hours.Id));
    }

    private static async Task<WorkingCalendar> SeedCalendarAsync(ApplicationDbContext context)
    {
        var calendar = WorkingCalendar.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            "Calendar",
            new DateOnly(2099, 1, 1),
            null,
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow);

        context.WorkingCalendars.Add(calendar);
        await context.SaveChangesAsync();
        return calendar;
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static WorkingHours CreateHours(Guid calendarId) =>
        WorkingHours.Create(
            calendarId,
            "Standard Office Hours",
            new DateOnly(2099, 1, 1),
            new DateOnly(2099, 12, 31),
            CreateWeekdayDefinitions(),
            DateTimeOffset.UtcNow);

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
