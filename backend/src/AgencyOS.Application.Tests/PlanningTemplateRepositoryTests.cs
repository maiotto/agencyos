using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Application.Tests;

public class PlanningTemplateRepositoryTests
{
    private static readonly Guid CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

    [Fact]
    public async Task AddAndGetById_RoundTrips()
    {
        await using var context = CreateContext();
        var (calendar, hours) = await SeedCalendarAndHoursAsync(context);
        var repository = new PlanningTemplateRepository(context);

        var template = PlanningTemplate.Create(
            CompanyId,
            "Standard Weekly",
            "Desc",
            calendar.Id,
            hours.Id,
            ResourceAvailabilityStrategies.RequireActiveConfiguration,
            7,
            0,
            PlanningTemplateCapacityRules.Default,
            null,
            DateTimeOffset.UtcNow);

        await repository.AddAsync(template);
        var loaded = await repository.GetByIdAsync(template.Id);

        Assert.NotNull(loaded);
        Assert.Equal(template.Name, loaded!.Name);
        Assert.Equal(calendar.Id, loaded.WorkingCalendarId);
    }

    [Fact]
    public async Task ExistsByCompanyAndName_RespectsUniqueness()
    {
        await using var context = CreateContext();
        var (calendar, hours) = await SeedCalendarAndHoursAsync(context);
        var repository = new PlanningTemplateRepository(context);

        await repository.AddAsync(PlanningTemplate.Create(
            CompanyId,
            "Standard Weekly",
            null,
            calendar.Id,
            hours.Id,
            ResourceAvailabilityStrategies.RequireActiveConfiguration,
            7,
            0,
            PlanningTemplateCapacityRules.Default,
            null,
            DateTimeOffset.UtcNow));

        Assert.True(await repository.ExistsByCompanyAndNameAsync(CompanyId, "standard weekly"));
        Assert.False(await repository.ExistsByCompanyAndNameAsync(Guid.NewGuid(), "Standard Weekly"));
    }

    [Fact]
    public async Task Query_FiltersByStatusAndSearch()
    {
        await using var context = CreateContext();
        var (calendar, hours) = await SeedCalendarAndHoursAsync(context);
        var repository = new PlanningTemplateRepository(context);

        var active = PlanningTemplate.Create(
            CompanyId,
            "Active Template",
            "Primary",
            calendar.Id,
            hours.Id,
            ResourceAvailabilityStrategies.RequireActiveConfiguration,
            7,
            0,
            PlanningTemplateCapacityRules.Default,
            null,
            DateTimeOffset.UtcNow);
        active.Activate(DateTimeOffset.UtcNow);
        await repository.AddAsync(active);

        await repository.AddAsync(PlanningTemplate.Create(
            CompanyId,
            "Other",
            null,
            calendar.Id,
            hours.Id,
            ResourceAvailabilityStrategies.RequireActiveConfiguration,
            7,
            0,
            PlanningTemplateCapacityRules.Default,
            null,
            DateTimeOffset.UtcNow));

        var results = await repository.GetAllAsync(new PlanningTemplateQueryParameters
        {
            CompanyId = CompanyId,
            Status = PlanningTemplateStatus.Active,
            Search = "Primary"
        });

        Assert.Single(results);
        Assert.Equal(active.Id, results[0].Id);
    }

    private static async Task<(WorkingCalendar Calendar, WorkingHours Hours)> SeedCalendarAndHoursAsync(
        ApplicationDbContext context)
    {
        var calendar = WorkingCalendar.Create(
            CompanyId,
            "Calendar",
            new DateOnly(2099, 1, 1),
            null,
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow);
        calendar.Activate(DateTimeOffset.UtcNow);
        context.WorkingCalendars.Add(calendar);

        var hours = WorkingHours.Create(
            calendar.Id,
            "Hours",
            new DateOnly(2099, 1, 1),
            null,
            WorkingDayNames.All.Select(day => new WorkingHoursDayDefinition
            {
                DayOfWeek = day,
                Enabled = WorkingDayNames.DefaultWeekdays.Contains(day),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(18, 0),
                BreakStart = new TimeOnly(12, 0),
                BreakEnd = new TimeOnly(13, 0)
            }).ToList(),
            DateTimeOffset.UtcNow);
        hours.Activate(DateTimeOffset.UtcNow);
        context.WorkingHours.Add(hours);
        await context.SaveChangesAsync();
        return (calendar, hours);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
