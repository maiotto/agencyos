using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class AssignmentServiceTests
{
    private readonly Mock<IAssignmentRepository> _assignmentRepository = new();
    private readonly Mock<ITaskRepository> _taskRepository = new();
    private readonly Mock<IExecutionResourceRepository> _executionResourceRepository = new();
    private readonly Mock<ILogger<AssignmentService>> _logger = new();

    private AssignmentService CreateService() =>
        new(
            _assignmentRepository.Object,
            _taskRepository.Object,
            _executionResourceRepository.Object,
            _logger.Object);

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundWhenAssignmentMissing()
    {
        var assignmentId = Guid.NewGuid();
        _assignmentRepository
            .Setup(repository => repository.GetByIdAsync(assignmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Assignment?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(assignmentId));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedAssignments()
    {
        var assignment = CreateAssignment(Guid.NewGuid());

        _assignmentRepository
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<AssignmentQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { assignment });

        var service = CreateService();
        var result = await service.GetAllAsync(new AssignmentQueryParameters());

        Assert.Single(result);
        Assert.Equal(assignment.TaskId, result[0].TaskId);
        Assert.Equal(assignment.ExecutionResourceId, result[0].ExecutionResourceId);
    }

    [Fact]
    public async Task CreateAsync_PersistsAssignment()
    {
        Assignment? persisted = null;
        var request = CreateValidCreateRequest();

        SetupTask(request.TaskId);
        SetupActiveResource(request.ExecutionResourceId);
        SetupAdd(assignment => persisted = assignment);

        var service = CreateService();
        var result = await service.CreateAsync(request);

        Assert.NotNull(persisted);
        Assert.Equal(request.TaskId, persisted!.TaskId);
        Assert.Equal(request.ExecutionResourceId, persisted.ExecutionResourceId);
        Assert.Equal(AssignmentRole.Responsible, persisted.AssignmentRole);
        Assert.Equal(AssignmentStatus.Planned, persisted.Status);
        Assert.Equal(request.TaskId, result.TaskId);
    }

    [Fact]
    public async Task CreateAsync_PersistsCanonicalRoleAndStatus()
    {
        Assignment? persisted = null;
        var request = CreateValidCreateRequest();
        request.AssignmentRole = "  responsible  ";
        request.Status = "pLANNED";

        SetupTask(request.TaskId);
        SetupActiveResource(request.ExecutionResourceId);
        SetupAdd(assignment => persisted = assignment);

        var service = CreateService();
        await service.CreateAsync(request);

        Assert.NotNull(persisted);
        Assert.Equal(AssignmentRole.Responsible, persisted!.AssignmentRole);
        Assert.Equal(AssignmentStatus.Planned, persisted.Status);
    }

    [Fact]
    public async Task CreateAsync_NormalizesWhitespaceNotesToNull()
    {
        Assignment? persisted = null;
        var request = CreateValidCreateRequest();
        request.Notes = "   ";

        SetupTask(request.TaskId);
        SetupActiveResource(request.ExecutionResourceId);
        SetupAdd(assignment => persisted = assignment);

        var service = CreateService();
        await service.CreateAsync(request);

        Assert.NotNull(persisted);
        Assert.Null(persisted!.Notes);
    }

    [Fact]
    public async Task CreateAsync_ThrowsNotFoundWhenTaskMissing()
    {
        var request = CreateValidCreateRequest();
        _taskRepository
            .Setup(repository => repository.GetByIdAsync(request.TaskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MissionTask?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_ThrowsNotFoundWhenExecutionResourceMissing()
    {
        var request = CreateValidCreateRequest();
        SetupTask(request.TaskId);

        _executionResourceRepository
            .Setup(repository => repository.GetByIdAsync(
                request.ExecutionResourceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExecutionResource?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_ThrowsBusinessRuleWhenExecutionResourceInactive()
    {
        var request = CreateValidCreateRequest();
        SetupTask(request.TaskId);

        _executionResourceRepository
            .Setup(repository => repository.GetByIdAsync(
                request.ExecutionResourceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecutionResource
            {
                Id = request.ExecutionResourceId,
                Code = "RES-001",
                Name = "Resource",
                ResourceType = ExecutionResourceType.InternalHuman,
                Status = ExecutionResourceStatus.Inactive,
                CapacityHoursPerWeek = 40,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.CreateAsync(request));

        Assert.Equal("Only Active Execution Resources can receive assignments.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundWhenAssignmentMissing()
    {
        var assignmentId = Guid.NewGuid();
        _assignmentRepository
            .Setup(repository => repository.GetByIdAsync(assignmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Assignment?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(assignmentId, CreateValidUpdateRequest()));
    }

    [Fact]
    public async Task UpdateAsync_RejectsCancelledAssignment()
    {
        var assignmentId = Guid.NewGuid();
        var assignment = CreateAssignment(assignmentId);
        assignment.Status = AssignmentStatus.Cancelled;

        _assignmentRepository
            .Setup(repository => repository.GetByIdAsync(assignmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.UpdateAsync(assignmentId, CreateValidUpdateRequest()));

        Assert.Equal("Cancelled Assignments cannot be edited.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_RejectsSettingCancelledViaPut()
    {
        var assignmentId = Guid.NewGuid();
        var assignment = CreateAssignment(assignmentId);

        _assignmentRepository
            .Setup(repository => repository.GetByIdAsync(assignmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        var request = CreateValidUpdateRequest();
        request.Status = AssignmentStatus.Cancelled;

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.UpdateAsync(assignmentId, request));

        Assert.Contains("dedicated assignment cancel endpoints", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_AppliesRequestedValues()
    {
        var assignmentId = Guid.NewGuid();
        var assignment = CreateAssignment(assignmentId);

        _assignmentRepository
            .Setup(repository => repository.GetByIdAsync(assignmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        _assignmentRepository
            .Setup(repository => repository.UpdateAsync(
                It.IsAny<Assignment>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Assignment updated, CancellationToken _) => updated);

        var request = CreateValidUpdateRequest();
        request.PlannedHours = 40;
        request.Status = AssignmentStatus.Confirmed;

        var service = CreateService();
        var result = await service.UpdateAsync(assignmentId, request);

        Assert.Equal(40, assignment.PlannedHours);
        Assert.Equal(AssignmentStatus.Confirmed, assignment.Status);
        Assert.Equal(40, result.PlannedHours);
    }

    [Fact]
    public async Task CancelAsync_SetsCancelledStatus()
    {
        var assignmentId = Guid.NewGuid();
        var assignment = CreateAssignment(assignmentId);

        _assignmentRepository
            .Setup(repository => repository.GetByIdAsync(assignmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        _assignmentRepository
            .Setup(repository => repository.UpdateAsync(assignment, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        var service = CreateService();
        var result = await service.CancelAsync(assignmentId);

        Assert.Equal(AssignmentStatus.Cancelled, assignment.Status);
        Assert.Equal(AssignmentStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task CancelAsync_IsIdempotentWhenAlreadyCancelled()
    {
        var assignmentId = Guid.NewGuid();
        var assignment = CreateAssignment(assignmentId);
        assignment.Status = AssignmentStatus.Cancelled;

        _assignmentRepository
            .Setup(repository => repository.GetByIdAsync(assignmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        var service = CreateService();
        await service.CancelAsync(assignmentId);

        _assignmentRepository.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<Assignment>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupTask(Guid taskId)
    {
        _taskRepository
            .Setup(repository => repository.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MissionTask
            {
                Id = taskId,
                MissionId = Guid.NewGuid(),
                Code = "TSK-001",
                Name = "Discovery Workshop",
                TaskTypeId = Guid.NewGuid(),
                TaskStatusId = Guid.NewGuid(),
                Priority = TaskPriority.High,
                EstimatedHours = 16,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
    }

    private void SetupActiveResource(Guid executionResourceId)
    {
        _executionResourceRepository
            .Setup(repository => repository.GetByIdAsync(
                executionResourceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecutionResource
            {
                Id = executionResourceId,
                Code = "RES-001",
                Name = "Senior Delivery Consultant",
                ResourceType = ExecutionResourceType.InternalHuman,
                Status = ExecutionResourceStatus.Active,
                CapacityHoursPerWeek = 40,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
    }

    private void SetupAdd(Action<Assignment> capture)
    {
        _assignmentRepository
            .Setup(repository => repository.AddAsync(
                It.IsAny<Assignment>(),
                It.IsAny<CancellationToken>()))
            .Callback<Assignment, CancellationToken>((assignment, _) => capture(assignment))
            .ReturnsAsync((Assignment assignment, CancellationToken _) => assignment);
    }

    private static CreateAssignmentRequest CreateValidCreateRequest()
    {
        return new CreateAssignmentRequest
        {
            TaskId = Guid.NewGuid(),
            ExecutionResourceId = Guid.NewGuid(),
            AssignmentRole = AssignmentRole.Responsible,
            PlannedHours = 24,
            PlannedStartDate = new DateOnly(2026, 1, 5),
            PlannedEndDate = new DateOnly(2026, 1, 9),
            AllocationPercentage = 50,
            Status = AssignmentStatus.Planned,
            Notes = "Primary delivery allocation"
        };
    }

    private static UpdateAssignmentRequest CreateValidUpdateRequest()
    {
        return new UpdateAssignmentRequest
        {
            AssignmentRole = AssignmentRole.Responsible,
            PlannedHours = 24,
            PlannedStartDate = new DateOnly(2026, 1, 5),
            PlannedEndDate = new DateOnly(2026, 1, 9),
            AllocationPercentage = 50,
            Status = AssignmentStatus.Planned,
            Notes = "Primary delivery allocation"
        };
    }

    private static Assignment CreateAssignment(Guid id)
    {
        var now = DateTimeOffset.UtcNow;

        return new Assignment
        {
            Id = id,
            TaskId = Guid.NewGuid(),
            ExecutionResourceId = Guid.NewGuid(),
            AssignmentRole = AssignmentRole.Responsible,
            PlannedHours = 24,
            PlannedStartDate = new DateOnly(2026, 1, 5),
            PlannedEndDate = new DateOnly(2026, 1, 9),
            AllocationPercentage = 50,
            Status = AssignmentStatus.Planned,
            Notes = "Primary delivery allocation",
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
