using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Application.Tests;

public class HolidayRepositoryTests
{
    [Fact]
    public async Task AddAndGetById_RoundTripsHoliday()
    {
        await using var context = CreateContext();
        var repository = new HolidayRepository(context);
        var holiday = CreateFutureHoliday();

        await repository.AddAsync(holiday);
        var loaded = await repository.GetByIdAsync(holiday.Id);

        Assert.NotNull(loaded);
        Assert.Equal(holiday.Name, loaded!.Name);
        Assert.Equal(HolidayTypes.Company, loaded.HolidayType);
    }

    [Fact]
    public async Task ExistsWithSameScopeAsync_DetectsDuplicate()
    {
        await using var context = CreateContext();
        var repository = new HolidayRepository(context);
        var holiday = CreateFutureHoliday();
        await repository.AddAsync(holiday);

        var exists = await repository.ExistsWithSameScopeAsync(
            holiday.HolidayType,
            holiday.CompanyId,
            holiday.StateCode,
            holiday.City,
            holiday.HolidayDate,
            holiday.Recurring);

        Assert.True(exists);
    }

    [Fact]
    public async Task GetActiveForCompanyOnDateAsync_ReturnsMatchingActiveHoliday()
    {
        await using var context = CreateContext();
        var repository = new HolidayRepository(context);
        var holiday = CreateFutureHoliday();
        holiday.Activate(DateTimeOffset.UtcNow);
        await repository.AddAsync(holiday);

        var inactive = Holiday.Create(
            holiday.CompanyId,
            "Other",
            null,
            HolidayTypes.Company,
            holiday.HolidayDate.AddDays(1),
            null,
            null,
            false,
            DateTimeOffset.UtcNow);
        await repository.AddAsync(inactive);

        var found = await repository.GetActiveForCompanyOnDateAsync(
            holiday.CompanyId!.Value,
            holiday.HolidayDate);

        Assert.Single(found);
        Assert.Equal(holiday.Id, found[0].Id);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByNameAndType()
    {
        await using var context = CreateContext();
        var repository = new HolidayRepository(context);

        await repository.AddAsync(CreateFutureHoliday());
        await repository.AddAsync(Holiday.Create(
            null,
            "Independence Day",
            null,
            HolidayTypes.National,
            new DateOnly(2099, 9, 7),
            null,
            null,
            true,
            DateTimeOffset.UtcNow));

        var results = await repository.GetAllAsync(new HolidayQueryParameters
        {
            HolidayType = HolidayTypes.National,
            Name = "Independence"
        });

        Assert.Single(results);
        Assert.Equal("Independence Day", results[0].Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesHoliday()
    {
        await using var context = CreateContext();
        var repository = new HolidayRepository(context);
        var holiday = CreateFutureHoliday();
        await repository.AddAsync(holiday);

        await repository.DeleteAsync(holiday);

        Assert.Null(await repository.GetByIdAsync(holiday.Id));
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static Holiday CreateFutureHoliday() =>
        Holiday.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            "Foundation Day",
            "Company holiday",
            HolidayTypes.Company,
            new DateOnly(2099, 11, 15),
            null,
            null,
            false,
            DateTimeOffset.UtcNow);
}
