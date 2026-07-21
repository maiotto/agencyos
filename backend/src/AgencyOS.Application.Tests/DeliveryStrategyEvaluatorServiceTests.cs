using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class DeliveryStrategyEvaluatorServiceTests
{
    private readonly Mock<IDeliveryStrategyBuilderService> _deliveryStrategyBuilderService = new();
    private readonly Mock<ICapacityCalculatorService> _capacityCalculatorService = new();
    private readonly Mock<IWorkloadCalculatorService> _workloadCalculatorService = new();
    private readonly Mock<IAvailabilityEngineService> _availabilityEngineService = new();
    private readonly Mock<IAllocationConflictDetectionService> _allocationConflictDetectionService = new();
    private readonly Mock<ILogger<DeliveryStrategyEvaluatorService>> _logger = new();

    private DeliveryStrategyEvaluatorService CreateService()
    {
        return new DeliveryStrategyEvaluatorService(
            _deliveryStrategyBuilderService.Object,
            _capacityCalculatorService.Object,
            _workloadCalculatorService.Object,
            _availabilityEngineService.Object,
            _allocationConflictDetectionService.Object,
            _logger.Object);
    }

    [Fact]
    public async Task EvaluateAsync_EvaluatesEveryGeneratedStrategy()
    {
        var contractId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var request = new EvaluateDeliveryStrategyRequest
        {
            ContractId = contractId,
            MissionId = missionId,
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 31)
        };

        _deliveryStrategyBuilderService
            .Setup(service => service.BuildAsync(
                It.IsAny<BuildDeliveryStrategyRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BuildDeliveryStrategyResponse
            {
                ContractId = contractId,
                MissionId = missionId,
                GeneratedStrategyCount = 1,
                Strategies =
                [
                    new DeliveryStrategyResponse
                    {
                        StrategyId = Guid.NewGuid(),
                        StrategyName = "Internal Human",
                        EstimatedHours = 16m,
                        EstimatedCost = 1360m,
                        AssignedResources =
                        [
                            new DeliveryStrategyTaskAssignmentResponse
                            {
                                ExecutionResourceId = resourceId,
                                ResourceType = ExecutionResourceType.InternalHuman,
                                PlannedHours = 16m
                            }
                        ]
                    }
                ]
            });

        _capacityCalculatorService
            .Setup(service => service.GetAllAsync(
                It.IsAny<CapacityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CapacityResponse>
            {
                new()
                {
                    ExecutionResourceId = resourceId,
                    TotalCapacityHours = 160m,
                    AvailableHours = 160m
                }
            });

        _workloadCalculatorService
            .Setup(service => service.GetAllAsync(
                It.IsAny<WorkloadQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WorkloadResponse>
            {
                new()
                {
                    ExecutionResourceId = resourceId,
                    TotalPlannedHours = 0m
                }
            });

        _availabilityEngineService
            .Setup(service => service.GetAllAsync(
                It.IsAny<AvailabilityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AvailabilityResponse>
            {
                new()
                {
                    ExecutionResourceId = resourceId,
                    AvailableHours = 160m
                }
            });

        _allocationConflictDetectionService
            .Setup(service => service.GetAllAsync(
                It.IsAny<AllocationConflictQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AllocationConflictResponse>());

        var service = CreateService();
        var response = await service.EvaluateAsync(request);

        Assert.Equal(1, response.EvaluatedStrategyCount);
        Assert.Single(response.EvaluatedStrategies);
        Assert.Equal("Internal Human", response.EvaluatedStrategies[0].Strategy.StrategyName);
        Assert.Equal(1360m, response.EvaluatedStrategies[0].EvaluationMetrics.EstimatedCost);
        Assert.Equal(16m, response.EvaluatedStrategies[0].EvaluationMetrics.EstimatedDurationHours);
        Assert.Equal(100m, response.EvaluatedStrategies[0].EvaluationMetrics.HumanUtilizationPercentage);
    }

    [Fact]
    public async Task EvaluateAsync_ThrowsBusinessRuleWhenNoStrategiesGenerated()
    {
        var request = new EvaluateDeliveryStrategyRequest
        {
            ContractId = Guid.NewGuid(),
            MissionId = Guid.NewGuid(),
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 31)
        };

        _deliveryStrategyBuilderService
            .Setup(service => service.BuildAsync(
                It.IsAny<BuildDeliveryStrategyRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BuildDeliveryStrategyResponse
            {
                GeneratedStrategyCount = 0,
                Strategies = []
            });

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.EvaluateAsync(request));
    }

    [Fact]
    public async Task EvaluateAsync_PropagatesNotFoundFromBuilder()
    {
        var request = new EvaluateDeliveryStrategyRequest
        {
            ContractId = Guid.NewGuid(),
            MissionId = Guid.NewGuid(),
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 31)
        };

        _deliveryStrategyBuilderService
            .Setup(service => service.BuildAsync(
                It.IsAny<BuildDeliveryStrategyRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Contract not found."));

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.EvaluateAsync(request));
    }
}
