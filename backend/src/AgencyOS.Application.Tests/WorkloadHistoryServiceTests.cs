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

public class WorkloadHistoryServiceTests
{
    [Fact]
    public async Task PersistAndQuery_RoundTripsImmutableHistory()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var model = CreatePersistModel(Guid.NewGuid());

        await service.PersistCompletedCalculationAsync(model);

        var results = await service.QueryAsync(new WorkloadHistoryQueryParameters
        {
            ExecutionResourceId = model.Workload.ExecutionResourceId,
            CompanyId = model.CompanyId,
            CalculationVersion = WorkloadHistoryVersions.Current
        });

        Assert.Single(results);
        Assert.Equal(model.Workload.TotalPlannedHours, results[0].AllocatedHours);
        Assert.Equal(model.CapacityHours, results[0].CapacityHours);
        Assert.Equal(WorkloadHistoryVersions.Current, results[0].CalculationVersion);
        Assert.NotEmpty(results[0].AssignmentDistribution);
    }

    [Fact]
    public async Task PersistCompletedCalculations_CreatesNewVersionEachTime()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var resourceId = Guid.NewGuid();
        var model = CreatePersistModel(resourceId);

        await service.PersistCompletedCalculationAsync(model);
        await service.PersistCompletedCalculationAsync(model);

        var results = await service.GetByExecutionResourceIdAsync(resourceId);
        Assert.Equal(2, results.Count);
        Assert.NotEqual(results[0].HistoryId, results[1].HistoryId);
    }

    [Fact]
    public async Task CompareAsync_ReturnsDeltas()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var resourceId = Guid.NewGuid();

        await service.PersistCompletedCalculationAsync(CreatePersistModel(resourceId, allocated: 24, capacity: 40));
        await service.PersistCompletedCalculationAsync(CreatePersistModel(resourceId, allocated: 32, capacity: 40));

        var histories = await service.GetByExecutionResourceIdAsync(resourceId);
        var older = histories.OrderBy(item => item.CalculationDate).First();
        var newer = histories.OrderBy(item => item.CalculationDate).Last();

        var comparison = await service.CompareAsync(new WorkloadHistoryCompareQueryParameters
        {
            LeftHistoryId = older.HistoryId,
            RightHistoryId = newer.HistoryId
        });

        Assert.Equal(8, comparison.AllocatedHoursDelta);
    }

    [Fact]
    public async Task AggregateAndTrends_ReturnAnalyticalViews()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var companyId = Guid.NewGuid();

        await service.PersistCompletedCalculationAsync(CreatePersistModel(Guid.NewGuid(), companyId: companyId, allocated: 10));
        await service.PersistCompletedCalculationAsync(CreatePersistModel(Guid.NewGuid(), companyId: companyId, allocated: 20));

        var parameters = new WorkloadHistoryQueryParameters { CompanyId = companyId };
        var aggregate = await service.AggregateAsync(parameters);
        var trends = await service.TrendsAsync(parameters);

        Assert.Equal(2, aggregate.RecordCount);
        Assert.Equal(30, aggregate.TotalAllocatedHours);
        Assert.Equal(2, trends.PointCount);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFound()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public void CompareValidator_RejectsSameIds()
    {
        var validator = new WorkloadHistoryCompareQueryParametersValidator();
        var result = validator.Validate(new WorkloadHistoryCompareQueryParameters
        {
            LeftHistoryId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
            RightHistoryId = Guid.Parse("11111111-1111-4111-8111-111111111111")
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Mapping_PreservesAssignmentDistributionInSnapshot()
    {
        var model = CreatePersistModel(Guid.NewGuid());
        var history = WorkloadHistoryMappings.CreateHistory(model, DateTimeOffset.UtcNow);
        var response = WorkloadHistoryMappings.ToResponse(history);

        Assert.Single(response.AssignmentDistribution);
        Assert.Equal(16, response.AssignmentDistribution[0].PlannedHours);
    }

    private static WorkloadHistoryService CreateService(ApplicationDbContext context) =>
        new(new WorkloadHistoryRepository(context), new AgencyOS.Application.Audit.NoOpAuditService(), NullLogger<WorkloadHistoryService>.Instance);

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static WorkloadHistoryPersistModel CreatePersistModel(
        Guid resourceId,
        Guid? companyId = null,
        decimal allocated = 24,
        decimal capacity = 40)
    {
        return new WorkloadHistoryPersistModel
        {
            Workload = new WorkloadResponse
            {
                ExecutionResourceId = resourceId,
                ExecutionResourceCode = "RES-001",
                ExecutionResourceName = "Consultant",
                PeriodStartDate = new DateOnly(2026, 7, 1),
                PeriodEndDate = new DateOnly(2026, 7, 7),
                TotalPlannedHours = allocated,
                AssignmentCount = 1,
                AverageHoursPerAssignment = allocated,
                WorkloadPercentage = capacity == 0
                    ? 0
                    : Math.Round((allocated / capacity) * 100, 2),
                AssignmentDistribution =
                [
                    new WorkloadAssignmentDistributionItem
                    {
                        AssignmentId = Guid.NewGuid(),
                        TaskId = Guid.NewGuid(),
                        AssignmentRole = "Responsible",
                        PlannedHours = 16,
                        PlannedStartDate = new DateOnly(2026, 7, 1),
                        PlannedEndDate = new DateOnly(2026, 7, 4),
                        Status = "Planned"
                    }
                ]
            },
            CompanyId = companyId ?? Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            CapacityHours = capacity,
            WorkingDays = 5,
            HolidayDays = 0,
            AvailableDays = 5
        };
    }
}

public class WorkloadHistoryRepositoryTests
{
    [Fact]
    public async Task QueryAsync_FiltersByCompanyPeriodAndVersion()
    {
        await using var context = CreateContext();
        var repository = new WorkloadHistoryRepository(context);
        var companyId = Guid.NewGuid();
        var otherCompanyId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();

        await repository.AddAsync(CreateHistory(resourceId, companyId, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 7)));
        await repository.AddAsync(CreateHistory(resourceId, otherCompanyId, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 7)));
        await repository.AddAsync(CreateHistory(resourceId, companyId, new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 7)));

        var results = await repository.QueryAsync(new WorkloadHistoryQueryParameters
        {
            CompanyId = companyId,
            PeriodStart = new DateOnly(2026, 7, 1),
            PeriodEnd = new DateOnly(2026, 7, 31),
            CalculationVersion = WorkloadHistoryVersions.Current
        });

        Assert.Single(results);
        Assert.Equal(companyId, results[0].CompanyId);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static WorkloadHistory CreateHistory(
        Guid resourceId,
        Guid companyId,
        DateOnly periodStart,
        DateOnly periodEnd)
    {
        return WorkloadHistory.Create(
            resourceId,
            companyId,
            periodStart,
            periodEnd,
            allocatedHours: 24,
            capacityHours: 40,
            workloadPercentage: 60,
            workingDays: 5,
            holidayDays: 0,
            availableDays: 5,
            calculationVersion: WorkloadHistoryVersions.Current,
            operationalInputsJson: """{"assignmentDistribution":[]}""",
            calculationDate: DateTimeOffset.UtcNow);
    }
}
