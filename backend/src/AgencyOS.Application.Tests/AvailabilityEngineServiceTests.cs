using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Application.Validators;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class AvailabilityEngineServiceTests
{
    private readonly Mock<ICapacityCalculatorService> _capacityCalculatorService = new();
    private readonly Mock<IWorkloadCalculatorService> _workloadCalculatorService = new();
    private readonly Mock<ILogger<AvailabilityEngineService>> _logger = new();

    private AvailabilityEngineService CreateService()
    {
        return new AvailabilityEngineService(
            _capacityCalculatorService.Object,
            _workloadCalculatorService.Object,
            _logger.Object);
    }

    [Fact]
    public async Task GetAllAsync_CalculatesAvailabilityUsingCapacityAndWorkload()
    {
        var resourceId = Guid.NewGuid();
        var parameters = new AvailabilityQueryParameters
        {
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7)
        };

        _capacityCalculatorService
            .Setup(service => service.GetAllAsync(
                It.Is<CapacityQueryParameters>(query =>
                    query.PeriodStartDate == parameters.PeriodStartDate
                    && query.PeriodEndDate == parameters.PeriodEndDate),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CapacityResponse>
            {
                CreateCapacityResponse(resourceId, "RES-001", 40m, 10m, 30m)
            });

        _workloadCalculatorService
            .Setup(service => service.GetAllAsync(
                It.Is<WorkloadQueryParameters>(query =>
                    query.PeriodStartDate == parameters.PeriodStartDate
                    && query.PeriodEndDate == parameters.PeriodEndDate),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WorkloadResponse>
            {
                CreateWorkloadResponse(resourceId, "RES-001", 10m)
            });

        var service = CreateService();
        var result = await service.GetAllAsync(parameters);

        Assert.Single(result);
        Assert.Equal(resourceId, result[0].ExecutionResourceId);
        Assert.Equal(30m, result[0].AvailableHours);
        Assert.Equal(10m, result[0].OccupiedHours);
        Assert.Equal(75m, result[0].AvailabilityPercentage);
        Assert.NotNull(result[0].NextAvailableDate);
        Assert.NotEmpty(result[0].AvailableTimeSlots);
    }

    [Fact]
    public async Task GetByResourceIdAsync_ThrowsNotFoundWhenCapacityServiceThrows()
    {
        var resourceId = Guid.NewGuid();
        var parameters = new AvailabilityQueryParameters
        {
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7)
        };

        _capacityCalculatorService
            .Setup(service => service.GetByResourceIdAsync(
                resourceId,
                It.IsAny<CapacityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Execution Resource with id '{resourceId}' was not found."));

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetByResourceIdAsync(resourceId, parameters));
    }

    [Fact]
    public async Task GetSummaryAsync_AggregatesAvailabilityAcrossResources()
    {
        var firstResourceId = Guid.NewGuid();
        var secondResourceId = Guid.NewGuid();
        var parameters = new AvailabilityQueryParameters
        {
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7)
        };

        _capacityCalculatorService
            .Setup(service => service.GetAllAsync(
                It.IsAny<CapacityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CapacityResponse>
            {
                CreateCapacityResponse(firstResourceId, "RES-001", 40m, 10m, 30m),
                CreateCapacityResponse(secondResourceId, "RES-002", 20m, 5m, 15m)
            });

        _workloadCalculatorService
            .Setup(service => service.GetAllAsync(
                It.IsAny<WorkloadQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WorkloadResponse>
            {
                CreateWorkloadResponse(firstResourceId, "RES-001", 10m),
                CreateWorkloadResponse(secondResourceId, "RES-002", 5m)
            });

        var service = CreateService();
        var summary = await service.GetSummaryAsync(parameters);

        Assert.Equal(2, summary.ActiveResourceCount);
        Assert.Equal(45m, summary.TotalAvailableHours);
        Assert.Equal(15m, summary.TotalOccupiedHours);
        Assert.Equal(75m, summary.OverallAvailabilityPercentage);
        Assert.Equal(2, summary.ResourcesWithAvailability);
    }

    [Fact]
    public void AvailabilityQueryParametersValidator_RequiresPlanningPeriod()
    {
        var validator = new AvailabilityQueryParametersValidator();
        var result = validator.Validate(new AvailabilityQueryParameters());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AvailabilityQueryParameters.PeriodStartDate));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AvailabilityQueryParameters.PeriodEndDate));
    }

    private static CapacityResponse CreateCapacityResponse(
        Guid resourceId,
        string code,
        decimal totalCapacityHours,
        decimal allocatedHours,
        decimal availableHours)
    {
        return new CapacityResponse
        {
            ExecutionResourceId = resourceId,
            ExecutionResourceCode = code,
            ExecutionResourceName = $"Resource {code}",
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7),
            TotalCapacityHours = totalCapacityHours,
            AllocatedHours = allocatedHours,
            AvailableHours = availableHours,
            UtilizationPercentage = 25m,
            RemainingCapacityHours = availableHours
        };
    }

    private static WorkloadResponse CreateWorkloadResponse(
        Guid resourceId,
        string code,
        decimal totalPlannedHours)
    {
        return new WorkloadResponse
        {
            ExecutionResourceId = resourceId,
            ExecutionResourceCode = code,
            ExecutionResourceName = $"Resource {code}",
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7),
            TotalPlannedHours = totalPlannedHours,
            AssignmentCount = 1,
            AverageHoursPerAssignment = totalPlannedHours,
            WorkloadPercentage = 25m,
            AssignmentDistribution = new List<WorkloadAssignmentDistributionItem>
            {
                new()
                {
                    AssignmentId = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    AssignmentRole = "Responsible",
                    PlannedHours = totalPlannedHours,
                    PlannedStartDate = new DateOnly(2026, 7, 1),
                    PlannedEndDate = new DateOnly(2026, 7, 7),
                    Status = "Planned"
                }
            }
        };
    }
}
