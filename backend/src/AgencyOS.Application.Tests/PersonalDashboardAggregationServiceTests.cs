using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Moq;

namespace AgencyOS.Application.Tests;

public class PersonalDashboardAggregationServiceTests
{
    private readonly Mock<IAssignmentRepository> _assignmentRepository = new();
    private readonly Mock<ITaskRepository> _taskRepository = new();
    private readonly Mock<IMissionRepository> _missionRepository = new();
    private readonly Mock<IRecommendationWorkflowService> _recommendationWorkflowService = new();
    private readonly Mock<IRecommendationService> _recommendationService = new();
    private readonly Mock<IDecisionService> _decisionService = new();
    private readonly Mock<ICapacityCalculatorService> _capacityCalculatorService = new();
    private readonly Mock<IWorkloadCalculatorService> _workloadCalculatorService = new();

    private PersonalDashboardAggregationService CreateService() =>
        new(
            _assignmentRepository.Object,
            _taskRepository.Object,
            _missionRepository.Object,
            _recommendationWorkflowService.Object,
            _recommendationService.Object,
            _decisionService.Object,
            _capacityCalculatorService.Object,
            _workloadCalculatorService.Object);

    private static Assignment CreateAssignment(Guid taskId, Guid resourceId, string status) => new()
    {
        Id = Guid.NewGuid(),
        TaskId = taskId,
        ExecutionResourceId = resourceId,
        Status = status,
        PlannedHours = 10m,
        PlannedStartDate = new DateOnly(2026, 1, 1),
        PlannedEndDate = new DateOnly(2026, 1, 31),
        AllocationPercentage = 50
    };

    private static MissionTask CreateTask(Guid id, Guid missionId, string? statusCode, DateOnly? plannedEnd = null) => new()
    {
        Id = id,
        MissionId = missionId,
        Code = $"T-{id:N}"[..8],
        Name = "Task " + id,
        Priority = TaskPriority.Medium,
        EstimatedHours = 5m,
        PlannedEnd = plannedEnd,
        Status = statusCode is null ? null : new TaskStatusLookup { Id = Guid.NewGuid(), Code = statusCode, Name = statusCode }
    };

    private static Mission CreateMission(Guid id) => new()
    {
        Id = id,
        Code = "M-" + id.ToString("N")[..6],
        Name = "Mission " + id,
        ClientContractId = Guid.NewGuid(),
        MissionTypeId = Guid.NewGuid(),
        MissionStatusId = Guid.NewGuid(),
        Priority = "NORMAL"
    };

    [Fact]
    public async Task GetTasksAsync_ReturnsEmpty_WhenExecutionResourceIdIsNull()
    {
        var service = CreateService();

        var tasks = await service.GetTasksAsync(null);

        Assert.Empty(tasks);
        _assignmentRepository.Verify(
            repository => repository.GetAllAsync(It.IsAny<AssignmentQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMissionsAsync_ReturnsEmpty_WhenExecutionResourceIdIsNull()
    {
        var service = CreateService();

        var missions = await service.GetMissionsAsync(null);

        Assert.Empty(missions);
    }

    [Fact]
    public async Task GetTasksAsync_ExcludesCompletedAndCancelledAssignments()
    {
        var resourceId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var activeTaskId = Guid.NewGuid();
        var completedTaskId = Guid.NewGuid();
        var cancelledTaskId = Guid.NewGuid();

        _assignmentRepository
            .Setup(repository => repository.GetAllAsync(
                It.Is<AssignmentQueryParameters>(parameters => parameters.ExecutionResourceId == resourceId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Assignment>
            {
                CreateAssignment(activeTaskId, resourceId, AssignmentStatus.InProgress),
                CreateAssignment(completedTaskId, resourceId, AssignmentStatus.Completed),
                CreateAssignment(cancelledTaskId, resourceId, AssignmentStatus.Cancelled)
            });

        _taskRepository
            .Setup(repository => repository.GetByIdAsync(activeTaskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTask(activeTaskId, missionId, MissionTaskStatus.InProgress));

        _missionRepository
            .Setup(repository => repository.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMission(missionId));

        var service = CreateService();
        var tasks = await service.GetTasksAsync(resourceId);

        Assert.Single(tasks);
        Assert.Equal(activeTaskId, tasks[0].Id);
        _taskRepository.Verify(
            repository => repository.GetByIdAsync(completedTaskId, It.IsAny<CancellationToken>()),
            Times.Never);
        _taskRepository.Verify(
            repository => repository.GetByIdAsync(cancelledTaskId, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetTasksAsync_ExcludesTasksWithCompletedOrCancelledOwnStatus()
    {
        var resourceId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var completedTaskId = Guid.NewGuid();
        var activeTaskId = Guid.NewGuid();

        _assignmentRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<AssignmentQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Assignment>
            {
                CreateAssignment(completedTaskId, resourceId, AssignmentStatus.Confirmed),
                CreateAssignment(activeTaskId, resourceId, AssignmentStatus.Confirmed)
            });

        _taskRepository
            .Setup(repository => repository.GetByIdAsync(completedTaskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTask(completedTaskId, missionId, MissionTaskStatus.Completed));
        _taskRepository
            .Setup(repository => repository.GetByIdAsync(activeTaskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTask(activeTaskId, missionId, MissionTaskStatus.Planned));

        _missionRepository
            .Setup(repository => repository.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMission(missionId));

        var service = CreateService();
        var tasks = await service.GetTasksAsync(resourceId);

        Assert.Single(tasks);
        Assert.Equal(activeTaskId, tasks[0].Id);
    }

    [Fact]
    public async Task GetTasksAsync_MarksOverdueTasks()
    {
        var resourceId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-5);

        _assignmentRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<AssignmentQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Assignment> { CreateAssignment(taskId, resourceId, AssignmentStatus.InProgress) });

        _taskRepository
            .Setup(repository => repository.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTask(taskId, missionId, MissionTaskStatus.InProgress, pastDate));

        _missionRepository
            .Setup(repository => repository.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMission(missionId));

        var service = CreateService();
        var tasks = await service.GetTasksAsync(resourceId);

        Assert.Single(tasks);
        Assert.True(tasks[0].IsOverdue);
    }

    [Fact]
    public async Task GetMissionsAsync_ComputesActiveTaskCountPerMission()
    {
        var resourceId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var taskOneId = Guid.NewGuid();
        var taskTwoId = Guid.NewGuid();

        _assignmentRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<AssignmentQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Assignment>
            {
                CreateAssignment(taskOneId, resourceId, AssignmentStatus.InProgress),
                CreateAssignment(taskTwoId, resourceId, AssignmentStatus.Planned)
            });

        _taskRepository
            .Setup(repository => repository.GetByIdAsync(taskOneId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTask(taskOneId, missionId, MissionTaskStatus.InProgress));
        _taskRepository
            .Setup(repository => repository.GetByIdAsync(taskTwoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTask(taskTwoId, missionId, MissionTaskStatus.Planned));

        _missionRepository
            .Setup(repository => repository.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMission(missionId));

        var service = CreateService();
        var missions = await service.GetMissionsAsync(resourceId);

        Assert.Single(missions);
        Assert.Equal(2, missions[0].ActiveTaskCount);
    }

    [Fact]
    public async Task GetRecommendationsAsync_ReturnsCompanyScopedPendingWorkflows()
    {
        var companyId = Guid.NewGuid();
        var otherCompanyId = Guid.NewGuid();

        _recommendationWorkflowService
            .Setup(service => service.GetAllAsync(
                It.Is<RecommendationWorkflowQueryParameters>(parameters =>
                    parameters.Status == RecommendationWorkflowStatus.PendingApproval),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RecommendationWorkflowResponse>
            {
                new() { Id = Guid.NewGuid(), CompanyId = companyId, Status = RecommendationWorkflowStatus.PendingApproval, CreatedBy = "alice" },
                new() { Id = Guid.NewGuid(), CompanyId = otherCompanyId, Status = RecommendationWorkflowStatus.PendingApproval, CreatedBy = "bob" }
            });

        var service = CreateService();
        var recommendations = await service.GetRecommendationsAsync(companyId, "system");

        Assert.Single(recommendations);
        _recommendationService.Verify(
            service => service.GetByCompanyIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<RecommendationQueryParameters>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetRecommendationsAsync_UnionsWorkflowsForRecommendationsGeneratedByUser()
    {
        var companyId = Guid.NewGuid();
        var userId = "alice";
        var recommendationId = Guid.NewGuid();
        var draftWorkflowId = Guid.NewGuid();

        _recommendationWorkflowService
            .SetupSequence(service => service.GetAllAsync(It.IsAny<RecommendationWorkflowQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RecommendationWorkflowResponse>())
            .ReturnsAsync(new List<RecommendationWorkflowResponse>
            {
                new()
                {
                    Id = draftWorkflowId,
                    CompanyId = companyId,
                    RecommendationId = recommendationId,
                    Status = RecommendationWorkflowStatus.Draft,
                    CreatedBy = userId
                }
            });

        _recommendationService
            .Setup(service => service.GetByCompanyIdAsync(
                companyId,
                It.Is<RecommendationQueryParameters>(parameters => parameters.GeneratedBy == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RecommendationResponse> { new() { Id = recommendationId } });

        var service = CreateService();
        var recommendations = await service.GetRecommendationsAsync(companyId, userId);

        Assert.Single(recommendations);
        Assert.Equal(draftWorkflowId, recommendations[0].Id);
    }

    [Fact]
    public async Task GetDecisionsAsync_FiltersToCreatedOrInProgress()
    {
        var companyId = Guid.NewGuid();

        _decisionService
            .Setup(service => service.FilterAsync(
                It.Is<DecisionQueryParameters>(parameters => parameters.CompanyId == companyId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DecisionResponse>
            {
                new() { Id = Guid.NewGuid(), DecisionStatus = DecisionStatus.Created, CreatedBy = "alice" },
                new() { Id = Guid.NewGuid(), DecisionStatus = DecisionStatus.InProgress, CreatedBy = "bob" },
                new() { Id = Guid.NewGuid(), DecisionStatus = DecisionStatus.Completed, CreatedBy = "alice" },
                new() { Id = Guid.NewGuid(), DecisionStatus = DecisionStatus.Cancelled, CreatedBy = "alice" }
            });

        var service = CreateService();
        var decisions = await service.GetDecisionsAsync(companyId, "system");

        Assert.Equal(2, decisions.Count);
        Assert.All(decisions, decision => Assert.Contains(
            decision.DecisionStatus,
            new[] { DecisionStatus.Created, DecisionStatus.InProgress }));
    }

    [Fact]
    public async Task GetDecisionsAsync_PrefersCreatedByMatchingUser()
    {
        var companyId = Guid.NewGuid();

        _decisionService
            .Setup(service => service.FilterAsync(It.IsAny<DecisionQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DecisionResponse>
            {
                new() { Id = Guid.NewGuid(), DecisionStatus = DecisionStatus.Created, CreatedBy = "alice" },
                new() { Id = Guid.NewGuid(), DecisionStatus = DecisionStatus.InProgress, CreatedBy = "bob" }
            });

        var service = CreateService();
        var decisions = await service.GetDecisionsAsync(companyId, "alice");

        Assert.Single(decisions);
        Assert.Equal("alice", decisions[0].CreatedBy);
    }

    [Fact]
    public async Task GetDecisionsAsync_FallsBackToCompanyPending_WhenUserHasNoOwnEntries()
    {
        var companyId = Guid.NewGuid();

        _decisionService
            .Setup(service => service.FilterAsync(It.IsAny<DecisionQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DecisionResponse>
            {
                new() { Id = Guid.NewGuid(), DecisionStatus = DecisionStatus.Created, CreatedBy = "bob" }
            });

        var service = CreateService();
        var decisions = await service.GetDecisionsAsync(companyId, "alice");

        Assert.Single(decisions);
        Assert.Equal("bob", decisions[0].CreatedBy);
    }

    [Fact]
    public async Task GetCapacityAsync_ReturnsEmptySummary_WhenExecutionResourceIdIsNull()
    {
        var service = CreateService();

        var capacity = await service.GetCapacityAsync(null, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

        Assert.False(capacity.HasData);
        _capacityCalculatorService.Verify(
            calculator => calculator.GetByResourceIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CapacityQueryParameters>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCapacityAsync_ReturnsEmptySummary_WhenCalculatorThrowsNotFound()
    {
        var resourceId = Guid.NewGuid();
        _capacityCalculatorService
            .Setup(calculator => calculator.GetByResourceIdAsync(
                resourceId,
                It.IsAny<CapacityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("not found"));

        var service = CreateService();
        var capacity = await service.GetCapacityAsync(resourceId, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

        Assert.False(capacity.HasData);
    }

    [Fact]
    public async Task GetCapacityAsync_ReturnsData_WhenResourceResolved()
    {
        var resourceId = Guid.NewGuid();
        _capacityCalculatorService
            .Setup(calculator => calculator.GetByResourceIdAsync(
                resourceId,
                It.IsAny<CapacityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CapacityResponse
            {
                ExecutionResourceId = resourceId,
                PeriodStartDate = new DateOnly(2026, 1, 1),
                PeriodEndDate = new DateOnly(2026, 1, 31),
                TotalCapacityHours = 100m,
                AllocatedHours = 40m,
                AvailableHours = 60m,
                UtilizationPercentage = 40m
            });

        var service = CreateService();
        var capacity = await service.GetCapacityAsync(resourceId, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

        Assert.True(capacity.HasData);
        Assert.Equal(40m, capacity.UtilizationPercentage);
    }

    [Fact]
    public async Task GetWorkloadAsync_ReturnsEmptySummary_WhenExecutionResourceIdIsNull()
    {
        var service = CreateService();

        var workload = await service.GetWorkloadAsync(null, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

        Assert.False(workload.HasData);
    }

    [Fact]
    public async Task GetWorkloadAsync_ReturnsData_WhenResourceResolved()
    {
        var resourceId = Guid.NewGuid();
        _workloadCalculatorService
            .Setup(calculator => calculator.GetByResourceIdAsync(
                resourceId,
                It.IsAny<WorkloadQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkloadResponse
            {
                ExecutionResourceId = resourceId,
                PeriodStartDate = new DateOnly(2026, 1, 1),
                PeriodEndDate = new DateOnly(2026, 1, 31),
                TotalPlannedHours = 30m,
                AssignmentCount = 3,
                WorkloadPercentage = 55m
            });

        var service = CreateService();
        var workload = await service.GetWorkloadAsync(resourceId, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

        Assert.True(workload.HasData);
        Assert.Equal(55m, workload.WorkloadPercentage);
    }
}
