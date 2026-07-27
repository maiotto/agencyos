using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Application.Validators;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class AllocationConflictDetectionServiceTests
{
    private readonly Mock<ICapacityCalculatorService> _capacityCalculatorService = new();
    private readonly Mock<IWorkloadCalculatorService> _workloadCalculatorService = new();
    private readonly Mock<IAvailabilityEngineService> _availabilityEngineService = new();
    private readonly Mock<ILogger<AllocationConflictDetectionService>> _logger = new();

    private AllocationConflictDetectionService CreateService()
    {
        return new AllocationConflictDetectionService(
            _capacityCalculatorService.Object,
            _workloadCalculatorService.Object,
            _availabilityEngineService.Object,
            _logger.Object);
    }

    [Fact]
    public async Task GetAllAsync_DetectsConflictsUsingCapacityWorkloadAndAvailability()
    {
        var resourceId = Guid.NewGuid();
        var parameters = new AllocationConflictQueryParameters
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
                CreateCapacityResponse(resourceId, "RES-001", 20m)
            });

        _workloadCalculatorService
            .Setup(service => service.GetAllAsync(
                It.IsAny<WorkloadQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WorkloadResponse>
            {
                CreateWorkloadResponse(resourceId, "RES-001", 30m)
            });

        _availabilityEngineService
            .Setup(service => service.GetAllAsync(
                It.IsAny<AvailabilityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AvailabilityResponse>
            {
                CreateAvailabilityResponse(resourceId, "RES-001", 0m, 30m)
            });

        var service = CreateService();
        var conflicts = await service.GetAllAsync(parameters);

        Assert.NotEmpty(conflicts);
        Assert.Contains(
            conflicts,
            conflict => conflict.ConflictType == AllocationConflictCalculation.ConflictType.CapacityExceeded);
    }

    [Fact]
    public async Task GetByResourceIdAsync_ThrowsNotFoundWhenCapacityServiceThrows()
    {
        var resourceId = Guid.NewGuid();
        var parameters = new AllocationConflictQueryParameters
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
    public async Task GetSummaryAsync_AggregatesConflictCounts()
    {
        var firstResourceId = Guid.NewGuid();
        var secondResourceId = Guid.NewGuid();
        var parameters = new AllocationConflictQueryParameters
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
                CreateCapacityResponse(firstResourceId, "RES-001", 20m),
                CreateCapacityResponse(secondResourceId, "RES-002", 40m)
            });

        _workloadCalculatorService
            .Setup(service => service.GetAllAsync(
                It.IsAny<WorkloadQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WorkloadResponse>
            {
                CreateWorkloadResponse(firstResourceId, "RES-001", 30m),
                CreateWorkloadResponse(
                    secondResourceId,
                    "RES-002",
                    10m,
                    new DateOnly(2026, 7, 1),
                    new DateOnly(2026, 7, 3))
            });

        _availabilityEngineService
            .Setup(service => service.GetAllAsync(
                It.IsAny<AvailabilityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AvailabilityResponse>
            {
                CreateAvailabilityResponse(firstResourceId, "RES-001", 0m, 30m),
                CreateAvailabilityResponse(secondResourceId, "RES-002", 30m, 10m)
            });

        var service = CreateService();
        var summary = await service.GetSummaryAsync(parameters);

        Assert.Equal(2, summary.ActiveResourceCount);
        Assert.True(summary.TotalConflictCount > 0);
        Assert.Equal(1, summary.ResourcesWithConflicts);
    }

    [Fact]
    public void AllocationConflictQueryParametersValidator_RequiresPlanningPeriod()
    {
        var validator = new AllocationConflictQueryParametersValidator();
        var result = validator.Validate(new AllocationConflictQueryParameters());

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(AllocationConflictQueryParameters.PeriodStartDate));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(AllocationConflictQueryParameters.PeriodEndDate));
    }

    private static CapacityResponse CreateCapacityResponse(
        Guid resourceId,
        string code,
        decimal totalCapacityHours)
    {
        var operationalDays = new List<CapacityDayBreakdownResponse>();
        for (var date = new DateOnly(2026, 7, 1); date <= new DateOnly(2026, 7, 7); date = date.AddDays(1))
        {
            var isOperational = date.DayOfWeek is >= DayOfWeek.Monday and <= DayOfWeek.Friday;
            operationalDays.Add(new CapacityDayBreakdownResponse
            {
                Date = date,
                IsOperationalDay = isOperational,
                IsCalendarWorkingWeekday = isOperational,
                IsHoliday = false,
                IsResourceAvailable = isOperational,
                PlannedCapacityHours = isOperational ? 8m : 0m
            });
        }

        return new CapacityResponse
        {
            ExecutionResourceId = resourceId,
            ExecutionResourceCode = code,
            ExecutionResourceName = $"Resource {code}",
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7),
            OperationalDayCount = operationalDays.Count(day => day.IsOperationalDay),
            TotalCapacityHours = totalCapacityHours,
            AllocatedHours = totalCapacityHours,
            AvailableHours = 0m,
            UtilizationPercentage = 100m,
            RemainingCapacityHours = 0m,
            OperationalDays = operationalDays
        };
    }

    private static WorkloadResponse CreateWorkloadResponse(
        Guid resourceId,
        string code,
        decimal totalPlannedHours,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        var plannedStartDate = startDate ?? new DateOnly(2026, 7, 1);
        var plannedEndDate = endDate ?? new DateOnly(2026, 7, 7);

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
            WorkloadPercentage = 150m,
            AssignmentDistribution = new List<WorkloadAssignmentDistributionItem>
            {
                new()
                {
                    AssignmentId = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    AssignmentRole = "Responsible",
                    PlannedHours = totalPlannedHours,
                    PlannedStartDate = plannedStartDate,
                    PlannedEndDate = plannedEndDate,
                    Status = "Planned"
                }
            }
        };
    }

    private static AvailabilityResponse CreateAvailabilityResponse(
        Guid resourceId,
        string code,
        decimal availableHours,
        decimal occupiedHours)
    {
        return new AvailabilityResponse
        {
            ExecutionResourceId = resourceId,
            ExecutionResourceCode = code,
            ExecutionResourceName = $"Resource {code}",
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7),
            AvailableHours = availableHours,
            OccupiedHours = occupiedHours,
            AvailabilityPercentage = 0m,
            NextAvailableDate = null,
            AvailableTimeSlots = Array.Empty<AvailabilityTimeSlotResponse>()
        };
    }
}
