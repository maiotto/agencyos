using AgencyOS.Application.DTOs;
using AgencyOS.Application.Mappings;
using AgencyOS.Application.Services;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using AgencyOS.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgencyOS.Application.Tests;

public class CapacityHistoryServiceTests
{
    [Fact]
    public async Task PersistAndQuery_RoundTripsImmutableHistory()
    {
        await using var context = CreateContext();
        var repository = new CapacityHistoryRepository(context);
        var service = new CapacityHistoryService(repository, new AgencyOS.Application.Audit.NoOpAuditService(), NullLogger<CapacityHistoryService>.Instance);

        var capacity = CreateCapacityResponse(Guid.NewGuid());
        var companyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

        await service.PersistCompletedCalculationAsync(capacity, companyId);

        var results = await service.QueryAsync(new CapacityHistoryQueryParameters
        {
            ExecutionResourceId = capacity.ExecutionResourceId,
            CompanyId = companyId,
            CalculationVersion = CapacityHistoryVersions.Current
        });

        Assert.Single(results);
        Assert.Equal(capacity.TotalCapacityHours, results[0].CapacityHours);
        Assert.Equal(companyId, results[0].CompanyId);
        Assert.Equal(CapacityHistoryVersions.Current, results[0].CalculationVersion);
        Assert.NotEmpty(results[0].OperationalDays);
    }

    [Fact]
    public async Task PersistCompletedCalculations_CreatesNewVersionEachTime()
    {
        await using var context = CreateContext();
        var repository = new CapacityHistoryRepository(context);
        var service = new CapacityHistoryService(repository, new AgencyOS.Application.Audit.NoOpAuditService(), NullLogger<CapacityHistoryService>.Instance);

        var resourceId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var capacity = CreateCapacityResponse(resourceId);

        await service.PersistCompletedCalculationAsync(capacity, companyId);
        await service.PersistCompletedCalculationAsync(capacity, companyId);

        var results = await service.GetByExecutionResourceIdAsync(resourceId);
        Assert.Equal(2, results.Count);
        Assert.NotEqual(results[0].HistoryId, results[1].HistoryId);
    }

    [Fact]
    public async Task CompareAsync_ReturnsDeltas()
    {
        await using var context = CreateContext();
        var repository = new CapacityHistoryRepository(context);
        var service = new CapacityHistoryService(repository, new AgencyOS.Application.Audit.NoOpAuditService(), NullLogger<CapacityHistoryService>.Instance);

        var resourceId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var leftCapacity = CreateCapacityResponse(resourceId, capacityHours: 40, availableHours: 30);
        var rightCapacity = CreateCapacityResponse(resourceId, capacityHours: 32, availableHours: 20);

        await service.PersistCompletedCalculationAsync(leftCapacity, companyId);
        await service.PersistCompletedCalculationAsync(rightCapacity, companyId);

        var histories = await service.GetByExecutionResourceIdAsync(resourceId);
        var older = histories.OrderBy(item => item.CalculationDate).First();
        var newer = histories.OrderBy(item => item.CalculationDate).Last();

        var comparison = await service.CompareAsync(new CapacityHistoryCompareQueryParameters
        {
            LeftHistoryId = older.HistoryId,
            RightHistoryId = newer.HistoryId
        });

        Assert.Equal(-8, comparison.CapacityHoursDelta);
        Assert.Equal(-10, comparison.AvailableHoursDelta);
    }

    [Fact]
    public async Task AggregateAsync_SumsFilteredHistory()
    {
        await using var context = CreateContext();
        var repository = new CapacityHistoryRepository(context);
        var service = new CapacityHistoryService(repository, new AgencyOS.Application.Audit.NoOpAuditService(), NullLogger<CapacityHistoryService>.Instance);
        var companyId = Guid.NewGuid();

        await service.PersistCompletedCalculationAsync(CreateCapacityResponse(Guid.NewGuid(), 40), companyId);
        await service.PersistCompletedCalculationAsync(CreateCapacityResponse(Guid.NewGuid(), 20), companyId);

        var aggregate = await service.AggregateAsync(new CapacityHistoryQueryParameters
        {
            CompanyId = companyId
        });

        Assert.Equal(2, aggregate.RecordCount);
        Assert.Equal(60, aggregate.TotalCapacityHours);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFound()
    {
        await using var context = CreateContext();
        var service = new CapacityHistoryService(new CapacityHistoryRepository(context), new AgencyOS.Application.Audit.NoOpAuditService(), NullLogger<CapacityHistoryService>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public void CompareValidator_RejectsSameIds()
    {
        var validator = new CapacityHistoryCompareQueryParametersValidator();
        var result = validator.Validate(new CapacityHistoryCompareQueryParameters
        {
            LeftHistoryId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
            RightHistoryId = Guid.Parse("11111111-1111-4111-8111-111111111111")
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Mapping_PreservesOperationalDaysInSnapshot()
    {
        var capacity = CreateCapacityResponse(Guid.NewGuid());
        var history = CapacityHistoryMappings.CreateHistory(
            capacity,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
        var response = CapacityHistoryMappings.ToResponse(history);

        Assert.Single(response.OperationalDays);
        Assert.Equal(new DateOnly(2026, 7, 1), response.OperationalDays[0].Date);
        Assert.Equal(8, response.OperationalDays[0].PlannedCapacityHours);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static CapacityResponse CreateCapacityResponse(
        Guid resourceId,
        decimal capacityHours = 40,
        decimal availableHours = 30)
    {
        return new CapacityResponse
        {
            ExecutionResourceId = resourceId,
            ExecutionResourceCode = "RES-001",
            ExecutionResourceName = "Consultant",
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7),
            OperationalDayCount = 5,
            HolidayImpactDayCount = 0,
            ResourceAvailabilityExcludedDayCount = 0,
            ConfiguredWorkingHoursTotal = capacityHours,
            TotalCapacityHours = capacityHours,
            AllocatedHours = capacityHours - availableHours,
            AvailableHours = availableHours,
            UtilizationPercentage = capacityHours == 0
                ? 0
                : Math.Round(((capacityHours - availableHours) / capacityHours) * 100, 2),
            RemainingCapacityHours = availableHours,
            OperationalDays =
            [
                new CapacityDayBreakdownResponse
                {
                    Date = new DateOnly(2026, 7, 1),
                    IsOperationalDay = true,
                    IsCalendarWorkingWeekday = true,
                    IsHoliday = false,
                    IsResourceAvailable = true,
                    PlannedCapacityHours = 8
                }
            ]
        };
    }
}

public class CapacityHistoryRepositoryTests
{
    [Fact]
    public async Task QueryAsync_FiltersByCompanyPeriodAndVersion()
    {
        await using var context = CreateContext();
        var repository = new CapacityHistoryRepository(context);
        var companyId = Guid.NewGuid();
        var otherCompanyId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();

        await repository.AddAsync(CreateHistory(resourceId, companyId, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 7)));
        await repository.AddAsync(CreateHistory(resourceId, otherCompanyId, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 7)));
        await repository.AddAsync(CreateHistory(resourceId, companyId, new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 7)));

        var results = await repository.QueryAsync(new CapacityHistoryQueryParameters
        {
            CompanyId = companyId,
            PeriodStart = new DateOnly(2026, 7, 1),
            PeriodEnd = new DateOnly(2026, 7, 31),
            CalculationVersion = CapacityHistoryVersions.Current
        });

        Assert.Single(results);
        Assert.Equal(companyId, results[0].CompanyId);
        Assert.Equal(new DateOnly(2026, 7, 1), results[0].PeriodStart);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static CapacityHistory CreateHistory(
        Guid resourceId,
        Guid companyId,
        DateOnly periodStart,
        DateOnly periodEnd)
    {
        return CapacityHistory.Create(
            resourceId,
            companyId,
            periodStart,
            periodEnd,
            workingDays: 5,
            holidayDays: 0,
            availableDays: 5,
            configuredHours: 40,
            availableHours: 30,
            capacityHours: 40,
            allocatedHours: 10,
            utilizationPercentage: 25,
            calculationVersion: CapacityHistoryVersions.Current,
            operationalInputsJson: """{"operationalDays":[]}""",
            calculationDate: DateTimeOffset.UtcNow);
    }
}
