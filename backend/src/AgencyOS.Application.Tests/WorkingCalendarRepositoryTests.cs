using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Application.Tests;

public class WorkingCalendarRepositoryTests
{
    [Fact]
    public async Task AddAndGetById_RoundTripsCalendar()
    {
        await using var context = CreateContext();
        var repository = new WorkingCalendarRepository(context);
        var calendar = CreateCalendar();

        await repository.AddAsync(calendar);
        var loaded = await repository.GetByIdAsync(calendar.Id);

        Assert.NotNull(loaded);
        Assert.Equal(calendar.Name, loaded!.Name);
        Assert.Equal(calendar.WorkingDays, loaded.WorkingDays);
        Assert.Equal(WorkingCalendarStatus.Inactive, loaded.Status);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        await using var context = CreateContext();
        var repository = new WorkingCalendarRepository(context);
        var calendar = CreateCalendar();
        await repository.AddAsync(calendar);

        calendar.Reconfigure(
            "Updated Name",
            calendar.EffectiveFrom,
            calendar.EffectiveTo,
            ["Monday", "Tuesday"],
            new DateOnly(2026, 7, 26),
            DateTimeOffset.UtcNow);

        await repository.UpdateAsync(calendar);
        var loaded = await repository.GetByIdAsync(calendar.Id);

        Assert.Equal("Updated Name", loaded!.Name);
        Assert.Equal(new[] { "Monday", "Tuesday" }, loaded.WorkingDays);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCalendar()
    {
        await using var context = CreateContext();
        var repository = new WorkingCalendarRepository(context);
        var calendar = CreateCalendar();
        await repository.AddAsync(calendar);

        await repository.DeleteAsync(calendar);
        var loaded = await repository.GetByIdAsync(calendar.Id);

        Assert.Null(loaded);
    }

    [Fact]
    public async Task GetActiveOverlappingAsync_ReturnsOverlappingActiveCalendars()
    {
        await using var context = CreateContext();
        var repository = new WorkingCalendarRepository(context);
        var companyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

        var active = WorkingCalendar.Create(
            companyId,
            "Active",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 12, 31),
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow);
        active.Activate(DateTimeOffset.UtcNow);

        var inactive = WorkingCalendar.Create(
            companyId,
            "Inactive",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 12, 31),
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow);

        await repository.AddAsync(active);
        await repository.AddAsync(inactive);

        var overlapping = await repository.GetActiveOverlappingAsync(
            companyId,
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 10, 1));

        Assert.Single(overlapping);
        Assert.Equal(active.Id, overlapping[0].Id);
    }

    [Fact]
    public async Task GetActiveCoveringDateAsync_ReturnsActiveCalendarForDate()
    {
        await using var context = CreateContext();
        var repository = new WorkingCalendarRepository(context);
        var companyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

        var calendar = WorkingCalendar.Create(
            companyId,
            "Covering",
            new DateOnly(2026, 1, 1),
            null,
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow);
        calendar.Activate(DateTimeOffset.UtcNow);
        await repository.AddAsync(calendar);

        var found = await repository.GetActiveCoveringDateAsync(companyId, new DateOnly(2026, 7, 26));

        Assert.NotNull(found);
        Assert.Equal(calendar.Id, found!.Id);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByCompanyAndName()
    {
        await using var context = CreateContext();
        var repository = new WorkingCalendarRepository(context);
        var companyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

        await repository.AddAsync(WorkingCalendar.Create(
            companyId,
            "Standard Agency Week",
            new DateOnly(2099, 1, 1),
            null,
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow));

        await repository.AddAsync(WorkingCalendar.Create(
            Guid.NewGuid(),
            "Other Company Week",
            new DateOnly(2099, 1, 1),
            null,
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow));

        var results = await repository.GetAllAsync(new WorkingCalendarQueryParameters
        {
            CompanyId = companyId,
            Name = "Agency"
        });

        Assert.Single(results);
        Assert.Equal("Standard Agency Week", results[0].Name);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static WorkingCalendar CreateCalendar()
    {
        return WorkingCalendar.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            "Standard Agency Week",
            new DateOnly(2099, 1, 1),
            new DateOnly(2099, 12, 31),
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow);
    }
}
