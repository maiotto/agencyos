using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class CapacityCalculatorServiceTests
{
    private readonly Mock<IExecutionResourceRepository> _executionResourceRepository = new();
    private readonly Mock<IAssignmentRepository> _assignmentRepository = new();
    private readonly Mock<ILogger<CapacityCalculatorService>> _logger = new();

    private CapacityCalculatorService CreateService()
    {
        return new CapacityCalculatorService(
            _executionResourceRepository.Object,
            _assignmentRepository.Object,
            _logger.Object);
    }

    [Fact]
    public async Task GetAllAsync_CalculatesCapacityOnlyForActiveResources()
    {
        var resourceId = Guid.NewGuid();
        var inactiveResourceId = Guid.NewGuid();
        var parameters = new CapacityQueryParameters
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
        Assert.Equal(40m, result[0].TotalCapacityHours);
        Assert.Equal(10m, result[0].AllocatedHours);
        Assert.Equal(30m, result[0].AvailableHours);
        Assert.Equal(30m, result[0].RemainingCapacityHours);
        Assert.Equal(25m, result[0].UtilizationPercentage);
    }

    [Fact]
    public async Task GetByResourceIdAsync_ThrowsNotFoundWhenResourceIsInactive()
    {
        var resourceId = Guid.NewGuid();
        var parameters = new CapacityQueryParameters
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
    public async Task GetSummaryAsync_AggregatesCapacityAcrossActiveResources()
    {
        var firstResourceId = Guid.NewGuid();
        var secondResourceId = Guid.NewGuid();
        var parameters = new CapacityQueryParameters
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
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Assignment>
            {
                CreateAssignment(firstResourceId, 10m, AssignmentStatus.Confirmed),
                CreateAssignment(secondResourceId, 5m, AssignmentStatus.InProgress)
            });

        var service = CreateService();
        var summary = await service.GetSummaryAsync(parameters);

        Assert.Equal(2, summary.ActiveResourceCount);
        Assert.Equal(60m, summary.TotalCapacityHours);
        Assert.Equal(15m, summary.TotalAllocatedHours);
        Assert.Equal(45m, summary.TotalAvailableHours);
        Assert.Equal(45m, summary.TotalRemainingCapacityHours);
        Assert.Equal(25m, summary.OverallUtilizationPercentage);
    }

    [Fact]
    public void CapacityQueryParametersValidator_RequiresPlanningPeriod()
    {
        var validator = new CapacityQueryParametersValidator();
        var result = validator.Validate(new CapacityQueryParameters());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CapacityQueryParameters.PeriodStartDate));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CapacityQueryParameters.PeriodEndDate));
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
