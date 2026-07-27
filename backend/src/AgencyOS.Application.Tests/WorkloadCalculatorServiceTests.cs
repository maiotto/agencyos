using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Mappings;
using AgencyOS.Application.Services;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class WorkloadCalculatorServiceTests
{
    private readonly Mock<IExecutionResourceRepository> _executionResourceRepository = new();
    private readonly Mock<IAssignmentRepository> _assignmentRepository = new();
    private readonly Mock<IResourceAvailabilityRepository> _resourceAvailabilityRepository = new();
    private readonly Mock<IWorkingCalendarRepository> _workingCalendarRepository = new();
    private readonly Mock<IHolidayRepository> _holidayRepository = new();
    private readonly Mock<IWorkloadHistoryService> _workloadHistoryService = new();
    private readonly Mock<ILogger<WorkloadCalculatorService>> _logger = new();

    private WorkloadCalculatorService CreateService()
    {
        _resourceAvailabilityRepository
            .Setup(repository => repository.GetActiveCoveringPeriodAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ResourceAvailability>());

        _workloadHistoryService
            .Setup(service => service.PersistCompletedCalculationAsync(
                It.IsAny<WorkloadHistoryPersistModel>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _workloadHistoryService
            .Setup(service => service.PersistCompletedCalculationsAsync(
                It.IsAny<IReadOnlyList<WorkloadHistoryPersistModel>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new WorkloadCalculatorService(
            _executionResourceRepository.Object,
            _assignmentRepository.Object,
            _resourceAvailabilityRepository.Object,
            _workingCalendarRepository.Object,
            _holidayRepository.Object,
            _workloadHistoryService.Object,
            _logger.Object);
    }

    [Fact]
    public async Task GetAllAsync_CalculatesWorkloadOnlyForActiveResources()
    {
        var resourceId = Guid.NewGuid();
        var inactiveResourceId = Guid.NewGuid();
        var parameters = new WorkloadQueryParameters
        {
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7)
        };

        _executionResourceRepository
            .Setup(repository => repository.GetAllAsync(
                It.Is<ExecutionResourceQueryParameters>(query => query.Status == ExecutionResourceStatus.Active),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExecutionResource>
            {
                CreateResource(resourceId, "RES-001", 40m)
            });

        _assignmentRepository
            .Setup(repository => repository.GetForCapacityCalculationAsync(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Assignment>
            {
                CreateAssignment(resourceId, 10m, AssignmentStatus.Planned),
                CreateAssignment(inactiveResourceId, 100m, AssignmentStatus.Planned)
            });

        var service = CreateService();
        var result = await service.GetAllAsync(parameters);

        Assert.Single(result);
        Assert.Equal(resourceId, result[0].ExecutionResourceId);
        Assert.Equal(10m, result[0].TotalPlannedHours);
        Assert.Equal(1, result[0].AssignmentCount);
        Assert.Equal(10m, result[0].AverageHoursPerAssignment);
        Assert.Equal(25m, result[0].WorkloadPercentage);
        Assert.Single(result[0].AssignmentDistribution);

        _workloadHistoryService.Verify(
            service => service.PersistCompletedCalculationsAsync(
                It.Is<IReadOnlyList<WorkloadHistoryPersistModel>>(
                    payload => payload.Count == 1
                        && payload[0].Workload.ExecutionResourceId == resourceId
                        && payload[0].CompanyId == WorkloadCalculatorService.DefaultCompanyId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSummaryAsync_DoesNotPersistHistory()
    {
        var resourceId = Guid.NewGuid();
        var parameters = new WorkloadQueryParameters
        {
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7)
        };

        _executionResourceRepository
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<ExecutionResourceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExecutionResource> { CreateResource(resourceId, "RES-001", 40m) });

        _assignmentRepository
            .Setup(repository => repository.GetForCapacityCalculationAsync(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Assignment>());

        var service = CreateService();
        await service.GetSummaryAsync(parameters);

        _workloadHistoryService.Verify(
            service => service.PersistCompletedCalculationsAsync(
                It.IsAny<IReadOnlyList<WorkloadHistoryPersistModel>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _workloadHistoryService.Verify(
            service => service.PersistCompletedCalculationAsync(
                It.IsAny<WorkloadHistoryPersistModel>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByResourceIdAsync_ThrowsNotFoundWhenResourceIsInactive()
    {
        var resourceId = Guid.NewGuid();
        var parameters = new WorkloadQueryParameters
        {
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7)
        };

        _executionResourceRepository
            .Setup(repository => repository.GetByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResource(resourceId, "RES-002", 40m, ExecutionResourceStatus.Inactive));

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetByResourceIdAsync(resourceId, parameters));
    }

    [Fact]
    public async Task GetSummaryAsync_AggregatesWorkloadAcrossActiveResources()
    {
        var firstResourceId = Guid.NewGuid();
        var secondResourceId = Guid.NewGuid();
        var parameters = new WorkloadQueryParameters
        {
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7)
        };

        _executionResourceRepository
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<ExecutionResourceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExecutionResource>
            {
                CreateResource(firstResourceId, "RES-001", 40m),
                CreateResource(secondResourceId, "RES-002", 20m)
            });

        _assignmentRepository
            .Setup(repository => repository.GetForCapacityCalculationAsync(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Assignment>
            {
                CreateAssignment(firstResourceId, 10m, AssignmentStatus.Confirmed),
                CreateAssignment(secondResourceId, 5m, AssignmentStatus.InProgress)
            });

        var service = CreateService();
        var summary = await service.GetSummaryAsync(parameters);

        Assert.Equal(2, summary.ActiveResourceCount);
        Assert.Equal(15m, summary.TotalPlannedHours);
        Assert.Equal(2, summary.TotalAssignmentCount);
        Assert.Equal(7.5m, summary.AverageHoursPerAssignment);
        Assert.Equal(25m, summary.OverallWorkloadPercentage);
    }

    [Fact]
    public void WorkloadQueryParametersValidator_RequiresPlanningPeriod()
    {
        var validator = new WorkloadQueryParametersValidator();
        var result = validator.Validate(new WorkloadQueryParameters());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(WorkloadQueryParameters.PeriodStartDate));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(WorkloadQueryParameters.PeriodEndDate));
    }

    private static ExecutionResource CreateResource(
        Guid id,
        string code,
        decimal capacityHoursPerWeek,
        string status = ExecutionResourceStatus.Active)
    {
        return new ExecutionResource
        {
            Id = id,
            Code = code,
            Name = $"Resource {code}",
            ResourceType = "Internal Human",
            Status = status,
            CapacityHoursPerWeek = capacityHoursPerWeek,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    private static Assignment CreateAssignment(Guid resourceId, decimal plannedHours, string status)
    {
        return new Assignment
        {
            Id = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            ExecutionResourceId = resourceId,
            AssignmentRole = AssignmentRole.Responsible,
            PlannedHours = plannedHours,
            PlannedStartDate = new DateOnly(2026, 7, 1),
            PlannedEndDate = new DateOnly(2026, 7, 7),
            AllocationPercentage = 100,
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
