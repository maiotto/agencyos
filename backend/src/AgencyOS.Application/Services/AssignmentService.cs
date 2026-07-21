using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class AssignmentService : IAssignmentService
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IExecutionResourceRepository _executionResourceRepository;
    private readonly ILogger<AssignmentService> _logger;

    public AssignmentService(
        IAssignmentRepository assignmentRepository,
        ITaskRepository taskRepository,
        IExecutionResourceRepository executionResourceRepository,
        ILogger<AssignmentService> logger)
    {
        _assignmentRepository = assignmentRepository;
        _taskRepository = taskRepository;
        _executionResourceRepository = executionResourceRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AssignmentResponse>> GetAllAsync(
        AssignmentQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var assignments = await _assignmentRepository.GetAllAsync(parameters, cancellationToken);
        return assignments.Select(MapToResponse).ToList();
    }

    public async Task<AssignmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await GetAssignmentOrThrowAsync(id, cancellationToken);
        return MapToResponse(assignment);
    }

    public async Task<AssignmentResponse> CreateAsync(
        CreateAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureTaskExistsAsync(request.TaskId, cancellationToken);
        await EnsureActiveExecutionResourceExistsAsync(request.ExecutionResourceId, cancellationToken);

        var now = DateTimeOffset.UtcNow;

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            TaskId = request.TaskId,
            ExecutionResourceId = request.ExecutionResourceId,
            AssignmentRole = request.AssignmentRole.Trim(),
            PlannedHours = request.PlannedHours,
            PlannedStartDate = request.PlannedStartDate,
            PlannedEndDate = request.PlannedEndDate,
            AllocationPercentage = request.AllocationPercentage,
            Status = request.Status.Trim(),
            Notes = NormalizeOptionalText(request.Notes),
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _assignmentRepository.AddAsync(assignment, cancellationToken);

        _logger.LogInformation(
            "Assignment Created: {AssignmentId} for Task {TaskId} and Execution Resource {ExecutionResourceId}",
            created.Id,
            created.TaskId,
            created.ExecutionResourceId);

        return MapToResponse(created);
    }

    public async Task<AssignmentResponse> UpdateAsync(
        Guid id,
        UpdateAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var assignment = await GetAssignmentOrThrowAsync(id, cancellationToken);

        EnsureAssignmentCanBeEdited(assignment);
        EnsureStatusChangeIsValid(assignment, request.Status);

        assignment.AssignmentRole = request.AssignmentRole.Trim();
        assignment.PlannedHours = request.PlannedHours;
        assignment.PlannedStartDate = request.PlannedStartDate;
        assignment.PlannedEndDate = request.PlannedEndDate;
        assignment.AllocationPercentage = request.AllocationPercentage;
        assignment.Status = request.Status.Trim();
        assignment.Notes = NormalizeOptionalText(request.Notes);
        assignment.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _assignmentRepository.UpdateAsync(assignment, cancellationToken);

        _logger.LogInformation(
            "Assignment Updated: {AssignmentId}",
            updated.Id);

        return MapToResponse(updated);
    }

    public async Task<AssignmentResponse> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await GetAssignmentOrThrowAsync(id, cancellationToken);

        if (string.Equals(assignment.Status, AssignmentStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            return MapToResponse(assignment);
        }

        assignment.Status = AssignmentStatus.Cancelled;
        assignment.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _assignmentRepository.UpdateAsync(assignment, cancellationToken);

        _logger.LogInformation(
            "Assignment Cancelled: {AssignmentId}",
            updated.Id);

        return MapToResponse(updated);
    }

    private async Task<Assignment> GetAssignmentOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id, cancellationToken);

        if (assignment is null)
        {
            throw new NotFoundException($"Assignment with id '{id}' was not found.");
        }

        return assignment;
    }

    private async Task EnsureTaskExistsAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);

        if (task is null)
        {
            throw new NotFoundException($"Task with id '{taskId}' was not found.");
        }
    }

    private async Task EnsureActiveExecutionResourceExistsAsync(
        Guid executionResourceId,
        CancellationToken cancellationToken)
    {
        var resource = await _executionResourceRepository.GetByIdAsync(executionResourceId, cancellationToken);

        if (resource is null)
        {
            throw new NotFoundException($"Execution Resource with id '{executionResourceId}' was not found.");
        }

        if (!ExecutionResourceStatus.CanReceiveAssignments(resource.Status))
        {
            throw new BusinessRuleException("Only Active Execution Resources can receive assignments.");
        }
    }

    private static void EnsureAssignmentCanBeEdited(Assignment assignment)
    {
        if (AssignmentStatus.NonEditable.Contains(assignment.Status))
        {
            throw new BusinessRuleException("Cancelled Assignments cannot be edited.");
        }
    }

    private static void EnsureStatusChangeIsValid(Assignment assignment, string newStatus)
    {
        if (string.Equals(assignment.Status, newStatus.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (string.Equals(newStatus, AssignmentStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException(
                "Status changes to Cancelled must use the dedicated assignment cancel endpoints.");
        }
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static AssignmentResponse MapToResponse(Assignment assignment)
    {
        return new AssignmentResponse
        {
            Id = assignment.Id,
            TaskId = assignment.TaskId,
            ExecutionResourceId = assignment.ExecutionResourceId,
            AssignmentRole = assignment.AssignmentRole,
            PlannedHours = assignment.PlannedHours,
            PlannedStartDate = assignment.PlannedStartDate,
            PlannedEndDate = assignment.PlannedEndDate,
            AllocationPercentage = assignment.AllocationPercentage,
            Status = assignment.Status,
            Notes = assignment.Notes,
            CreatedAt = assignment.CreatedAt,
            UpdatedAt = assignment.UpdatedAt
        };
    }
}
