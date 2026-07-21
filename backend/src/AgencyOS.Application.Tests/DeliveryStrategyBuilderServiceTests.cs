using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class DeliveryStrategyBuilderServiceTests
{
    private readonly Mock<IClientContractRepository> _contractRepository = new();
    private readonly Mock<IMissionRepository> _missionRepository = new();
    private readonly Mock<ITaskRepository> _taskRepository = new();
    private readonly Mock<IExecutionResourceRepository> _executionResourceRepository = new();
    private readonly Mock<ICapacityCalculatorService> _capacityCalculatorService = new();
    private readonly Mock<IWorkloadCalculatorService> _workloadCalculatorService = new();
    private readonly Mock<IAvailabilityEngineService> _availabilityEngineService = new();
    private readonly Mock<IAllocationConflictDetectionService> _allocationConflictDetectionService = new();
    private readonly Mock<ILogger<DeliveryStrategyBuilderService>> _logger = new();

    private DeliveryStrategyBuilderService CreateService()
    {
        return new DeliveryStrategyBuilderService(
            _contractRepository.Object,
            _missionRepository.Object,
            _taskRepository.Object,
            _executionResourceRepository.Object,
            _capacityCalculatorService.Object,
            _workloadCalculatorService.Object,
            _availabilityEngineService.Object,
            _allocationConflictDetectionService.Object,
            _logger.Object);
    }

    [Fact]
    public async Task BuildAsync_GeneratesCandidateStrategiesUsingOperationalInputs()
    {
        var contractId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var request = new BuildDeliveryStrategyRequest
        {
            ContractId = contractId,
            MissionId = missionId,
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 31)
        };

        _contractRepository
            .Setup(repository => repository.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClientContract
            {
                Id = contractId,
                BillingModel = ContractType.TimeAndMaterial
            });

        _missionRepository
            .Setup(repository => repository.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mission
            {
                Id = missionId,
                ClientContractId = contractId
            });

        _taskRepository
            .Setup(repository => repository.GetAllAsync(
                It.Is<TaskQueryParameters>(parameters => parameters.MissionId == missionId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MissionTask>
            {
                new()
                {
                    Id = taskId,
                    MissionId = missionId,
                    Code = "TSK-001",
                    Name = "Design Homepage",
                    EstimatedHours = 16m
                }
            });

        _executionResourceRepository
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<ExecutionResourceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExecutionResource>
            {
                new()
                {
                    Id = resourceId,
                    Code = "RES-001",
                    Name = "Senior Designer",
                    ResourceType = ExecutionResourceType.InternalHuman,
                    Status = ExecutionResourceStatus.Active,
                    CostRate = 85m
                }
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
                    ExecutionResourceCode = "RES-001",
                    ExecutionResourceName = "Senior Designer",
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
        var response = await service.BuildAsync(request);

        Assert.Equal(1, response.GeneratedStrategyCount);
        Assert.Single(response.Strategies);
        Assert.Equal("Internal Human", response.Strategies[0].StrategyName);
        Assert.Equal(16m, response.Strategies[0].EstimatedHours);
        Assert.Equal(1360m, response.Strategies[0].EstimatedCost);
    }

    [Fact]
    public async Task BuildAsync_ThrowsNotFoundWhenContractDoesNotExist()
    {
        var request = new BuildDeliveryStrategyRequest
        {
            ContractId = Guid.NewGuid(),
            MissionId = Guid.NewGuid(),
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 31)
        };

        _contractRepository
            .Setup(repository => repository.GetByIdAsync(request.ContractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClientContract?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.BuildAsync(request));
    }

    [Fact]
    public async Task BuildAsync_ThrowsBusinessRuleWhenMissionHasNoTasks()
    {
        var contractId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var request = new BuildDeliveryStrategyRequest
        {
            ContractId = contractId,
            MissionId = missionId,
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 31)
        };

        _contractRepository
            .Setup(repository => repository.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClientContract { Id = contractId });

        _missionRepository
            .Setup(repository => repository.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mission { Id = missionId, ClientContractId = contractId });

        _taskRepository
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<TaskQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MissionTask>());

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.BuildAsync(request));
    }
}
