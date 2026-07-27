using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepository = new();
    private readonly Mock<IMissionRepository> _missionRepository = new();
    private readonly Mock<ILogger<TaskService>> _logger = new();

    private static readonly Guid PlannedStatusId = Guid.Parse("11111111-1111-4111-8221-000000000002");
    private static readonly Guid CompletedStatusId = Guid.Parse("11111111-1111-4111-8221-000000000004");
    private static readonly Guid TaskTypeId = Guid.Parse("11111111-1111-4111-8211-000000000001");

    private TaskService CreateService() =>
        new(_taskRepository.Object, _missionRepository.Object, _logger.Object);

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundWhenTaskMissing()
    {
        var taskId = Guid.NewGuid();
        _taskRepository
            .Setup(repository => repository.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MissionTask?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(taskId));
    }

    [Fact]
    public async Task CreateAsync_PersistsTaskWhenMissionExists()
    {
        var missionId = Guid.NewGuid();
        MissionTask? persisted = null;

        SetupMission(missionId);
        SetupTypeAndStatus();
        SetupUniqueCode();

        _taskRepository
            .Setup(repository => repository.AddAsync(It.IsAny<MissionTask>(), It.IsAny<CancellationToken>()))
            .Callback<MissionTask, CancellationToken>((task, _) => persisted = task)
            .ReturnsAsync((MissionTask task, CancellationToken _) => task);

        _taskRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => CreateTask(id, missionId, PlannedStatusId));

        var service = CreateService();
        var result = await service.CreateAsync(CreateValidRequest(missionId));

        Assert.NotNull(persisted);
        Assert.Equal(missionId, persisted!.MissionId);
        Assert.Equal("TSK-001", persisted.Code);
        Assert.Equal("Discovery Workshop", result.Name);
    }

    [Fact]
    public async Task CreateAsync_NormalizesWhitespaceDescriptionToNull()
    {
        var missionId = Guid.NewGuid();
        MissionTask? persisted = null;

        SetupMission(missionId);
        SetupTypeAndStatus();
        SetupUniqueCode();

        _taskRepository
            .Setup(repository => repository.AddAsync(It.IsAny<MissionTask>(), It.IsAny<CancellationToken>()))
            .Callback<MissionTask, CancellationToken>((task, _) => persisted = task)
            .ReturnsAsync((MissionTask task, CancellationToken _) => task);

        _taskRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => CreateTask(id, missionId, PlannedStatusId));

        var request = CreateValidRequest(missionId);
        request.Description = "   ";

        var service = CreateService();
        await service.CreateAsync(request);

        Assert.NotNull(persisted);
        Assert.Null(persisted!.Description);
    }

    [Fact]
    public async Task CreateAsync_ThrowsNotFoundWhenMissionMissing()
    {
        var missionId = Guid.NewGuid();
        _missionRepository
            .Setup(repository => repository.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mission?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateAsync(CreateValidRequest(missionId)));
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflictWhenTaskCodeExistsForMission()
    {
        var missionId = Guid.NewGuid();
        SetupMission(missionId);
        SetupTypeAndStatus();

        _taskRepository
            .Setup(repository => repository.ExistsWithCodeForMissionAsync(
                missionId,
                "TSK-001",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(CreateValidRequest(missionId)));

        Assert.Contains("TSK-001", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsNotFoundWhenTaskTypeMissing()
    {
        var missionId = Guid.NewGuid();
        SetupMission(missionId);

        _taskRepository
            .Setup(repository => repository.GetTypeByIdAsync(TaskTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskTypeLookup?)null);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateAsync(CreateValidRequest(missionId)));

        Assert.Contains(TaskTypeId.ToString(), exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsNotFoundWhenTaskStatusMissing()
    {
        var missionId = Guid.NewGuid();
        SetupMission(missionId);

        _taskRepository
            .Setup(repository => repository.GetTypeByIdAsync(TaskTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskTypeLookup
            {
                Id = TaskTypeId,
                Code = "DELIVERY",
                Name = "Delivery"
            });

        _taskRepository
            .Setup(repository => repository.GetStatusByIdAsync(PlannedStatusId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskStatusLookup?)null);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateAsync(CreateValidRequest(missionId)));

        Assert.Contains(PlannedStatusId.ToString(), exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_RejectsCompletedTask()
    {
        var taskId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var task = CreateTask(taskId, missionId, CompletedStatusId);
        task.Status = new TaskStatusLookup
        {
            Id = CompletedStatusId,
            Code = MissionTaskStatus.Completed,
            Name = "Completed"
        };

        _taskRepository
            .Setup(repository => repository.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.UpdateAsync(taskId, CreateValidUpdateRequest()));

        Assert.Equal("Completed Tasks cannot be edited.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_RejectsSettingCompletedViaPut()
    {
        var taskId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var task = CreateTask(taskId, missionId, PlannedStatusId);
        task.Status = new TaskStatusLookup
        {
            Id = PlannedStatusId,
            Code = MissionTaskStatus.Planned,
            Name = "Planned"
        };

        _taskRepository
            .Setup(repository => repository.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        SetupTypeAndStatus();
        SetupUniqueCode(missionId, taskId);

        _taskRepository
            .Setup(repository => repository.GetStatusByIdAsync(CompletedStatusId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskStatusLookup
            {
                Id = CompletedStatusId,
                Code = MissionTaskStatus.Completed,
                Name = "Completed"
            });

        var request = CreateValidUpdateRequest();
        request.TaskStatusId = CompletedStatusId;

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.UpdateAsync(taskId, request));

        Assert.Contains("dedicated task complete endpoint", exception.Message);
    }

    [Fact]
    public async Task CompleteAsync_SetsCompletedStatus()
    {
        var taskId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var task = CreateTask(taskId, missionId, PlannedStatusId);
        task.Status = new TaskStatusLookup
        {
            Id = PlannedStatusId,
            Code = MissionTaskStatus.Planned,
            Name = "Planned"
        };

        _taskRepository
            .Setup(repository => repository.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        _taskRepository
            .Setup(repository => repository.GetStatusByCodeAsync(
                MissionTaskStatus.Completed,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskStatusLookup
            {
                Id = CompletedStatusId,
                Code = MissionTaskStatus.Completed,
                Name = "Completed"
            });

        _taskRepository
            .Setup(repository => repository.UpdateAsync(task, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var service = CreateService();
        await service.CompleteAsync(taskId);

        Assert.Equal(CompletedStatusId, task.TaskStatusId);
        Assert.NotNull(task.ActualEnd);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTask()
    {
        var taskId = Guid.NewGuid();
        var task = CreateTask(taskId, Guid.NewGuid(), PlannedStatusId);

        _taskRepository
            .Setup(repository => repository.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        _taskRepository
            .Setup(repository => repository.DeleteAsync(task, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();
        await service.DeleteAsync(taskId);

        _taskRepository.Verify(
            repository => repository.DeleteAsync(task, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void SetupMission(Guid missionId)
    {
        _missionRepository
            .Setup(repository => repository.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mission
            {
                Id = missionId,
                ClientContractId = Guid.NewGuid(),
                Code = "MSN-001",
                Name = "Mission",
                MissionTypeId = Guid.NewGuid(),
                MissionStatusId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
    }

    private void SetupTypeAndStatus()
    {
        _taskRepository
            .Setup(repository => repository.GetTypeByIdAsync(TaskTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskTypeLookup
            {
                Id = TaskTypeId,
                Code = "DELIVERY",
                Name = "Delivery"
            });

        _taskRepository
            .Setup(repository => repository.GetStatusByIdAsync(PlannedStatusId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskStatusLookup
            {
                Id = PlannedStatusId,
                Code = MissionTaskStatus.Planned,
                Name = "Planned"
            });
    }

    private void SetupUniqueCode(Guid? missionId = null, Guid? excludeTaskId = null)
    {
        if (missionId.HasValue)
        {
            _taskRepository
                .Setup(repository => repository.ExistsWithCodeForMissionAsync(
                    missionId.Value,
                    It.IsAny<string>(),
                    excludeTaskId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            return;
        }

        _taskRepository
            .Setup(repository => repository.ExistsWithCodeForMissionAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private static CreateTaskRequest CreateValidRequest(Guid missionId)
    {
        return new CreateTaskRequest
        {
            MissionId = missionId,
            Code = "TSK-001",
            Name = "Discovery Workshop",
            Description = "Kickoff discovery",
            TaskTypeId = TaskTypeId,
            TaskStatusId = PlannedStatusId,
            Priority = TaskPriority.High,
            EstimatedHours = 16,
            PlannedStartDate = new DateOnly(2026, 1, 5),
            PlannedEndDate = new DateOnly(2026, 1, 9)
        };
    }

    private static UpdateTaskRequest CreateValidUpdateRequest()
    {
        return new UpdateTaskRequest
        {
            Code = "TSK-001",
            Name = "Discovery Workshop",
            Description = "Kickoff discovery",
            TaskTypeId = TaskTypeId,
            TaskStatusId = PlannedStatusId,
            Priority = TaskPriority.High,
            EstimatedHours = 16,
            PlannedStartDate = new DateOnly(2026, 1, 5),
            PlannedEndDate = new DateOnly(2026, 1, 9)
        };
    }

    private static MissionTask CreateTask(Guid id, Guid missionId, Guid statusId)
    {
        return new MissionTask
        {
            Id = id,
            MissionId = missionId,
            Code = "TSK-001",
            Name = "Discovery Workshop",
            TaskTypeId = TaskTypeId,
            TaskStatusId = statusId,
            Priority = TaskPriority.High,
            EstimatedHours = 16,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Status = new TaskStatusLookup
            {
                Id = statusId,
                Code = statusId == CompletedStatusId ? MissionTaskStatus.Completed : MissionTaskStatus.Planned,
                Name = statusId == CompletedStatusId ? "Completed" : "Planned"
            }
        };
    }
}
