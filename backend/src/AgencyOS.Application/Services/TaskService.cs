using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IMissionRepository _missionRepository;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        ITaskRepository taskRepository,
        IMissionRepository missionRepository,
        ILogger<TaskService> logger)
    {
        _taskRepository = taskRepository;
        _missionRepository = missionRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TaskResponse>> GetAllAsync(
        TaskQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetAllAsync(parameters, cancellationToken);
        return tasks.Select(MapToResponse).ToList();
    }

    public async Task<TaskResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await GetTaskOrThrowAsync(id, cancellationToken);
        return MapToResponse(task);
    }

    public async Task<TaskResponse> CreateAsync(
        CreateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureMissionExistsAsync(request.MissionId, cancellationToken);
        await EnsureTaskTypeExistsAsync(request.TaskTypeId, cancellationToken);
        await EnsureTaskStatusExistsAsync(request.TaskStatusId, cancellationToken);
        await EnsureTaskCodeIsUniqueForMissionAsync(request.MissionId, request.Code, null, cancellationToken);

        var now = DateTimeOffset.UtcNow;

        var task = new MissionTask
        {
            Id = Guid.NewGuid(),
            MissionId = request.MissionId,
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Description = NormalizeOptionalText(request.Description),
            TaskTypeId = request.TaskTypeId,
            TaskStatusId = request.TaskStatusId,
            Priority = request.Priority.Trim(),
            EstimatedHours = request.EstimatedHours,
            PlannedStart = request.PlannedStartDate,
            PlannedEnd = request.PlannedEndDate,
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _taskRepository.AddAsync(task, cancellationToken);

        _logger.LogInformation(
            "Task Created: {TaskId} ({TaskCode}) for Mission {MissionId}",
            created.Id,
            created.Code,
            created.MissionId);

        return MapToResponse(await GetTaskOrThrowAsync(created.Id, cancellationToken));
    }

    public async Task<TaskResponse> UpdateAsync(
        Guid id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await GetTaskOrThrowAsync(id, cancellationToken);

        EnsureTaskCanBeEdited(task);
        await EnsureTaskTypeExistsAsync(request.TaskTypeId, cancellationToken);
        await EnsureTaskStatusExistsAsync(request.TaskStatusId, cancellationToken);
        await EnsureTaskCodeIsUniqueForMissionAsync(task.MissionId, request.Code, id, cancellationToken);
        await EnsureStatusTransitionIsValidAsync(task, request.TaskStatusId, cancellationToken);

        task.Code = request.Code.Trim();
        task.Name = request.Name.Trim();
        task.Description = NormalizeOptionalText(request.Description);
        task.TaskTypeId = request.TaskTypeId;
        task.TaskStatusId = request.TaskStatusId;
        task.Priority = request.Priority.Trim();
        task.EstimatedHours = request.EstimatedHours;
        task.PlannedStart = request.PlannedStartDate;
        task.PlannedEnd = request.PlannedEndDate;
        task.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _taskRepository.UpdateAsync(task, cancellationToken);

        _logger.LogInformation(
            "Task Updated: {TaskId} ({TaskCode})",
            updated.Id,
            updated.Code);

        return MapToResponse(await GetTaskOrThrowAsync(updated.Id, cancellationToken));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await GetTaskOrThrowAsync(id, cancellationToken);

        await _taskRepository.DeleteAsync(task, cancellationToken);

        _logger.LogInformation(
            "Task Deleted: {TaskId} ({TaskCode})",
            task.Id,
            task.Code);
    }

    public async Task<TaskResponse> CompleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await GetTaskOrThrowAsync(id, cancellationToken);
        var completedStatus = await _taskRepository.GetStatusByCodeAsync(MissionTaskStatus.Completed, cancellationToken);

        if (completedStatus is null)
        {
            throw new NotFoundException($"Task status '{MissionTaskStatus.Completed}' was not found.");
        }

        if (task.TaskStatusId == completedStatus.Id)
        {
            return MapToResponse(task);
        }

        task.TaskStatusId = completedStatus.Id;
        task.ActualEnd ??= DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        task.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _taskRepository.UpdateAsync(task, cancellationToken);

        _logger.LogInformation(
            "Task Completed: {TaskId} ({TaskCode})",
            updated.Id,
            updated.Code);

        return MapToResponse(await GetTaskOrThrowAsync(updated.Id, cancellationToken));
    }

    private async Task<MissionTask> GetTaskOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken);

        if (task is null)
        {
            throw new NotFoundException($"Task with id '{id}' was not found.");
        }

        return task;
    }

    private async Task EnsureMissionExistsAsync(Guid missionId, CancellationToken cancellationToken)
    {
        var mission = await _missionRepository.GetByIdAsync(missionId, cancellationToken);

        if (mission is null)
        {
            throw new NotFoundException($"Mission with id '{missionId}' was not found.");
        }
    }

    private async Task EnsureTaskTypeExistsAsync(Guid taskTypeId, CancellationToken cancellationToken)
    {
        var taskType = await _taskRepository.GetTypeByIdAsync(taskTypeId, cancellationToken);

        if (taskType is null)
        {
            throw new NotFoundException($"Task type with id '{taskTypeId}' was not found.");
        }
    }

    private async Task EnsureTaskStatusExistsAsync(Guid taskStatusId, CancellationToken cancellationToken)
    {
        var taskStatus = await _taskRepository.GetStatusByIdAsync(taskStatusId, cancellationToken);

        if (taskStatus is null)
        {
            throw new NotFoundException($"Task status with id '{taskStatusId}' was not found.");
        }
    }

    private async Task EnsureTaskCodeIsUniqueForMissionAsync(
        Guid missionId,
        string code,
        Guid? excludeTaskId,
        CancellationToken cancellationToken)
    {
        var normalizedCode = code.Trim();

        if (await _taskRepository.ExistsWithCodeForMissionAsync(
                missionId,
                normalizedCode,
                excludeTaskId,
                cancellationToken))
        {
            throw new ConflictException(
                $"A Task with code '{normalizedCode}' already exists for the specified Mission.");
        }
    }

    private static void EnsureTaskCanBeEdited(MissionTask task)
    {
        if (task.Status is not null
            && MissionTaskStatus.NonEditable.Contains(task.Status.Code))
        {
            throw new BusinessRuleException("Completed Tasks cannot be edited.");
        }
    }

    private async Task EnsureStatusTransitionIsValidAsync(
        MissionTask task,
        Guid newTaskStatusId,
        CancellationToken cancellationToken)
    {
        if (task.TaskStatusId == newTaskStatusId)
        {
            return;
        }

        var newStatus = await _taskRepository.GetStatusByIdAsync(newTaskStatusId, cancellationToken);

        if (newStatus is not null
            && string.Equals(newStatus.Code, MissionTaskStatus.Completed, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException(
                "Status changes to Completed must use the dedicated task complete endpoint.");
        }
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static TaskResponse MapToResponse(MissionTask task)
    {
        return new TaskResponse
        {
            Id = task.Id,
            MissionId = task.MissionId,
            Code = task.Code,
            Name = task.Name,
            Description = task.Description,
            TaskTypeId = task.TaskTypeId,
            TaskStatusId = task.TaskStatusId,
            Status = task.Status?.Code ?? string.Empty,
            Priority = task.Priority,
            EstimatedHours = task.EstimatedHours,
            PlannedStartDate = task.PlannedStart,
            PlannedEndDate = task.PlannedEnd,
            ActualStartDate = task.ActualStart,
            ActualEndDate = task.ActualEnd,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}
