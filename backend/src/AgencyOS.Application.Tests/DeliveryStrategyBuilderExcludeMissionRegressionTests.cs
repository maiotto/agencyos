using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class DeliveryStrategyBuilderExcludeMissionRegressionTests
{
    [Fact]
    public async Task BuildAsync_MinimalScenarioWithExistingAssignment_GeneratesAtLeastOneStrategy()
    {
        var contractId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var periodStartDate = new DateOnly(2026, 7, 1);
        var periodEndDate = new DateOnly(2026, 7, 7);
        const decimal plannedHours = 16m;

        var contractRepository = new Mock<IClientContractRepository>();
        var missionRepository = new Mock<IMissionRepository>();
        var taskRepository = new Mock<ITaskRepository>();
        var executionResourceRepository = new Mock<IExecutionResourceRepository>();
        var assignmentRepository = new Mock<IAssignmentRepository>();
        var capacityLogger = new Mock<ILogger<CapacityCalculatorService>>();
        var workloadLogger = new Mock<ILogger<WorkloadCalculatorService>>();
        var availabilityLogger = new Mock<ILogger<AvailabilityEngineService>>();
        var conflictLogger = new Mock<ILogger<AllocationConflictDetectionService>>();
        var builderLogger = new Mock<ILogger<DeliveryStrategyBuilderService>>();

        var resource = new ExecutionResource
        {
            Id = resourceId,
            Code = "RES-001",
            Name = "Senior Designer",
            ResourceType = ExecutionResourceType.InternalHuman,
            Status = ExecutionResourceStatus.Active,
            CapacityHoursPerWeek = plannedHours,
            CostRate = 85m
        };

        var existingAssignment = new Assignment
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            ExecutionResourceId = resourceId,
            AssignmentRole = AssignmentRole.Responsible,
            PlannedHours = plannedHours,
            PlannedStartDate = periodStartDate,
            PlannedEndDate = periodEndDate,
            AllocationPercentage = 100,
            Status = AssignmentStatus.Planned
        };

        contractRepository
            .Setup(repository => repository.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClientContract
            {
                Id = contractId,
                BillingModel = ContractType.TimeAndMaterial
            });

        missionRepository
            .Setup(repository => repository.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mission
            {
                Id = missionId,
                ClientContractId = contractId
            });

        taskRepository
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
                    EstimatedHours = plannedHours
                }
            });

        executionResourceRepository
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<ExecutionResourceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExecutionResource> { resource });

        assignmentRepository
            .Setup(repository => repository.GetForCapacityCalculationAsync(
                periodStartDate,
                periodEndDate,
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .Returns((
                DateOnly _,
                DateOnly __,
                Guid? ___,
                Guid? excludeMissionId,
                CancellationToken ____) =>
                Task.FromResult<IReadOnlyList<Assignment>>(
                    excludeMissionId == missionId
                        ? Array.Empty<Assignment>()
                        : new List<Assignment> { existingAssignment }));

        var capacityCalculatorService = new CapacityCalculatorService(
            executionResourceRepository.Object,
            assignmentRepository.Object,
            capacityLogger.Object);
        var workloadCalculatorService = new WorkloadCalculatorService(
            executionResourceRepository.Object,
            assignmentRepository.Object,
            workloadLogger.Object);
        var availabilityEngineService = new AvailabilityEngineService(
            capacityCalculatorService,
            workloadCalculatorService,
            availabilityLogger.Object);
        var allocationConflictDetectionService = new AllocationConflictDetectionService(
            capacityCalculatorService,
            workloadCalculatorService,
            availabilityEngineService,
            conflictLogger.Object);

        var builder = new DeliveryStrategyBuilderService(
            contractRepository.Object,
            missionRepository.Object,
            taskRepository.Object,
            executionResourceRepository.Object,
            capacityCalculatorService,
            workloadCalculatorService,
            availabilityEngineService,
            allocationConflictDetectionService,
            builderLogger.Object);

        var response = await builder.BuildAsync(new BuildDeliveryStrategyRequest
        {
            ContractId = contractId,
            MissionId = missionId,
            PeriodStartDate = periodStartDate,
            PeriodEndDate = periodEndDate
        });

        Assert.True(response.GeneratedStrategyCount >= 1);
        Assert.NotEmpty(response.Strategies);
        Assert.Contains(
            response.Strategies,
            strategy => strategy.EstimatedHours == plannedHours
                && strategy.AssignedResources.Count == 1
                && strategy.AssignedResources[0].ExecutionResourceId == resourceId);

        assignmentRepository.Verify(
            repository => repository.GetForCapacityCalculationAsync(
                periodStartDate,
                periodEndDate,
                null,
                missionId,
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void IsOperationallyValid_RejectsDoubleCountedMissionAssignment_ButAcceptsWhenExcluded()
    {
        var resourceId = Guid.NewGuid();
        const decimal plannedHours = 16m;

        var capacitiesByResourceId = new Dictionary<Guid, CapacityResponse>
        {
            [resourceId] = new()
            {
                ExecutionResourceId = resourceId,
                TotalCapacityHours = plannedHours,
                AvailableHours = 0m
            }
        };
        var workloadsByResourceId = new Dictionary<Guid, WorkloadResponse>
        {
            [resourceId] = new()
            {
                ExecutionResourceId = resourceId,
                TotalPlannedHours = plannedHours
            }
        };
        var availabilitiesByResourceId = new Dictionary<Guid, AvailabilityResponse>
        {
            [resourceId] = new()
            {
                ExecutionResourceId = resourceId,
                AvailableHours = 0m
            }
        };
        var conflictsByResourceId =
            new Dictionary<Guid, IReadOnlyList<AllocationConflictResponse>>();

        var assignments = new List<DeliveryStrategyTaskAssignmentCandidate>
        {
            new()
            {
                TaskId = Guid.NewGuid(),
                ExecutionResourceId = resourceId,
                ExecutionResourceCode = "RES-001",
                PlannedHours = plannedHours,
                ResourceType = ExecutionResourceType.InternalHuman
            }
        };

        var isValidWithDoubleCount = DeliveryStrategyGeneration.IsOperationallyValid(
            assignments,
            capacitiesByResourceId,
            workloadsByResourceId,
            availabilitiesByResourceId,
            conflictsByResourceId);

        capacitiesByResourceId[resourceId].AvailableHours = plannedHours;
        workloadsByResourceId[resourceId].TotalPlannedHours = 0m;
        availabilitiesByResourceId[resourceId].AvailableHours = plannedHours;

        var isValidWhenMissionExcluded = DeliveryStrategyGeneration.IsOperationallyValid(
            assignments,
            capacitiesByResourceId,
            workloadsByResourceId,
            availabilitiesByResourceId,
            conflictsByResourceId);

        Assert.False(isValidWithDoubleCount);
        Assert.True(isValidWhenMissionExcluded);
    }
}
