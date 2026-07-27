using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Application.Tests;

public class ResourceAvailabilityRepositoryTests
{
    [Fact]
    public async Task AddAndGetById_RoundTripsWithChildren()
    {
        await using var context = CreateContext();
        var repository = new ResourceAvailabilityRepository(context);
        var availability = CreateAvailability();

        await repository.AddAsync(availability);
        var loaded = await repository.GetByIdAsync(availability.Id);

        Assert.NotNull(loaded);
        Assert.Equal(availability.Name, loaded!.Name);
        Assert.Equal(7, loaded.WeeklyAvailability.Count);
        Assert.Equal(5, loaded.WeeklyAvailability.Count(day => day.Enabled));
    }

    [Fact]
    public async Task GetActiveOverlappingAsync_ReturnsOverlappingActive()
    {
        await using var context = CreateContext();
        var repository = new ResourceAvailabilityRepository(context);
        var resourceId = Guid.NewGuid();

        var active = CreateAvailability(resourceId);
        active.Activate(DateTimeOffset.UtcNow);
        await repository.AddAsync(active);

        var inactive = CreateAvailability(resourceId, "Inactive");
        await repository.AddAsync(inactive);

        var overlapping = await repository.GetActiveOverlappingAsync(
            resourceId,
            new DateOnly(2099, 6, 1),
            new DateOnly(2099, 6, 30));

        Assert.Single(overlapping);
        Assert.Equal(active.Id, overlapping[0].Id);
    }

    [Fact]
    public async Task GetActiveCoveringDateAsync_ReturnsActiveCoveringDate()
    {
        await using var context = CreateContext();
        var repository = new ResourceAvailabilityRepository(context);
        var resourceId = Guid.NewGuid();

        var availability = CreateAvailability(resourceId);
        availability.Activate(DateTimeOffset.UtcNow);
        await repository.AddAsync(availability);

        var found = await repository.GetActiveCoveringDateAsync(resourceId, new DateOnly(2099, 6, 15));

        Assert.NotNull(found);
        Assert.Equal(availability.Id, found!.Id);
    }

    [Fact]
    public async Task DeleteAsync_RemovesConfiguration()
    {
        await using var context = CreateContext();
        var repository = new ResourceAvailabilityRepository(context);
        var availability = CreateAvailability();
        await repository.AddAsync(availability);

        await repository.DeleteAsync(availability);

        Assert.Null(await repository.GetByIdAsync(availability.Id));
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ResourceAvailability CreateAvailability(
        Guid? resourceId = null,
        string name = "Alice Standard Availability") =>
        ResourceAvailability.Create(
            resourceId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            name,
            new DateOnly(2099, 1, 1),
            new DateOnly(2099, 12, 31),
            WorkingDayNames.Ordered
                .Select(day => new ResourceAvailabilityWeekDayDefinition
                {
                    DayOfWeek = day,
                    Enabled = WorkingDayNames.DefaultWeekdays.Contains(day)
                })
                .ToList(),
            [],
            DateTimeOffset.UtcNow);
}
