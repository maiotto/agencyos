using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;

namespace AgencyOS.Application.Services;

/// <summary>
/// Builds raw My Work Dashboard section data directly from existing read repositories/services
/// (US-501 / BR-2401..BR-2410). Every number is a projection over already-persisted data; nothing
/// is recalculated beyond what the existing Capacity/Workload engines already compute, and nothing
/// is written back to any repository.
/// </summary>
public class PersonalDashboardAggregationService : IPersonalDashboardAggregationService
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IMissionRepository _missionRepository;
    private readonly IRecommendationWorkflowService _recommendationWorkflowService;
    private readonly IRecommendationService _recommendationService;
    private readonly IDecisionService _decisionService;
    private readonly ICapacityCalculatorService _capacityCalculatorService;
    private readonly IWorkloadCalculatorService _workloadCalculatorService;

    public PersonalDashboardAggregationService(
        IAssignmentRepository assignmentRepository,
        ITaskRepository taskRepository,
        IMissionRepository missionRepository,
        IRecommendationWorkflowService recommendationWorkflowService,
        IRecommendationService recommendationService,
        IDecisionService decisionService,
        ICapacityCalculatorService capacityCalculatorService,
        IWorkloadCalculatorService workloadCalculatorService)
    {
        _assignmentRepository = assignmentRepository;
        _taskRepository = taskRepository;
        _missionRepository = missionRepository;
        _recommendationWorkflowService = recommendationWorkflowService;
        _recommendationService = recommendationService;
        _decisionService = decisionService;
        _capacityCalculatorService = capacityCalculatorService;
        _workloadCalculatorService = workloadCalculatorService;
    }

    public async Task<IReadOnlyList<MyWorkMissionCardResponse>> GetMissionsAsync(
        Guid? executionResourceId,
        CancellationToken cancellationToken = default)
    {
        var activeWork = await LoadActiveWorkAsync(executionResourceId, cancellationToken);

        return activeWork.Missions
            .Select(mission => new MyWorkMissionCardResponse
            {
                Id = mission.Id,
                Code = mission.Code,
                Name = mission.Name,
                Priority = mission.Priority,
                StartDate = mission.StartDate,
                EndDate = mission.EndDate,
                ActiveTaskCount = activeWork.Tasks.Count(task => task.MissionId == mission.Id),
                DrillDownPath = $"/recommendations?missionId={mission.Id}"
            })
            .OrderBy(mission => mission.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<IReadOnlyList<MyWorkTaskCardResponse>> GetTasksAsync(
        Guid? executionResourceId,
        CancellationToken cancellationToken = default)
    {
        var activeWork = await LoadActiveWorkAsync(executionResourceId, cancellationToken);
        var missionNames = activeWork.Missions.ToDictionary(mission => mission.Id, mission => mission.Name);
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);

        return activeWork.Tasks
            .Select(task =>
            {
                var assignment = activeWork.AssignmentsByTaskId.GetValueOrDefault(task.Id);
                return new MyWorkTaskCardResponse
                {
                    Id = task.Id,
                    MissionId = task.MissionId,
                    MissionName = missionNames.GetValueOrDefault(task.MissionId, string.Empty),
                    Code = task.Code,
                    Name = task.Name,
                    Status = task.Status?.Code,
                    Priority = task.Priority,
                    PlannedStart = task.PlannedStart,
                    PlannedEnd = task.PlannedEnd,
                    EstimatedHours = task.EstimatedHours,
                    AssignmentId = assignment?.Id ?? Guid.Empty,
                    AssignmentStatus = assignment?.Status ?? string.Empty,
                    AssignmentPlannedHours = assignment?.PlannedHours ?? 0m,
                    IsOverdue = task.PlannedEnd.HasValue && task.PlannedEnd.Value < today,
                    DrillDownPath = $"/recommendations?missionId={task.MissionId}"
                };
            })
            .OrderBy(task => task.PlannedEnd ?? DateOnly.MaxValue)
            .ThenBy(task => task.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<IReadOnlyList<MyWorkRecommendationCardResponse>> GetRecommendationsAsync(
        Guid companyId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var pendingWorkflows = await _recommendationWorkflowService.GetAllAsync(
            new RecommendationWorkflowQueryParameters { Status = RecommendationWorkflowStatus.PendingApproval },
            cancellationToken);

        var queue = new Dictionary<Guid, RecommendationWorkflowResponse>();
        foreach (var workflow in pendingWorkflows.Where(workflow => workflow.CompanyId == companyId))
        {
            queue[workflow.Id] = workflow;
        }

        if (!IsSystemUser(userId))
        {
            var ownRecommendations = await _recommendationService.GetByCompanyIdAsync(
                companyId,
                new RecommendationQueryParameters { GeneratedBy = userId, IncludeArchived = true },
                cancellationToken);

            if (ownRecommendations.Count > 0)
            {
                var ownRecommendationIds = ownRecommendations
                    .Select(recommendation => recommendation.Id)
                    .ToHashSet();

                var allWorkflows = await _recommendationWorkflowService.GetAllAsync(
                    new RecommendationWorkflowQueryParameters(),
                    cancellationToken);

                foreach (var workflow in allWorkflows)
                {
                    if (workflow.CompanyId == companyId && ownRecommendationIds.Contains(workflow.RecommendationId))
                    {
                        queue[workflow.Id] = workflow;
                    }
                }
            }
        }

        return queue.Values
            .OrderByDescending(workflow => workflow.CreatedAt)
            .Select(workflow => new MyWorkRecommendationCardResponse
            {
                Id = workflow.Id,
                RecommendationId = workflow.RecommendationId,
                MissionId = workflow.MissionId,
                Title = workflow.Title,
                Status = workflow.Status,
                CreatedBy = workflow.CreatedBy,
                CreatedAt = workflow.CreatedAt,
                DrillDownPath = $"/recommendations/workflow/{workflow.Id}"
            })
            .ToList();
    }

    public async Task<IReadOnlyList<MyWorkDecisionCardResponse>> GetDecisionsAsync(
        Guid companyId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var companyDecisions = await _decisionService.FilterAsync(
            new DecisionQueryParameters { CompanyId = companyId },
            cancellationToken);

        var pending = companyDecisions
            .Where(decision =>
                DecisionStatus.IsCreated(decision.DecisionStatus)
                || DecisionStatus.IsInProgress(decision.DecisionStatus))
            .ToList();

        if (!IsSystemUser(userId))
        {
            var ownPending = pending
                .Where(decision => string.Equals(decision.CreatedBy, userId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (ownPending.Count > 0)
            {
                pending = ownPending;
            }
        }

        return pending
            .OrderBy(decision => decision.DecisionDate)
            .Select(decision => new MyWorkDecisionCardResponse
            {
                Id = decision.Id,
                RecommendationId = decision.RecommendationId,
                MissionId = decision.MissionId,
                DecisionStatus = decision.DecisionStatus,
                ImplementationStatus = decision.ImplementationStatus,
                DecisionDate = decision.DecisionDate,
                CreatedBy = decision.CreatedBy,
                DrillDownPath = $"/decisions/{decision.Id}"
            })
            .ToList();
    }

    public async Task<MyWorkCapacitySummaryResponse> GetCapacityAsync(
        Guid? executionResourceId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default)
    {
        var empty = new MyWorkCapacitySummaryResponse
        {
            HasData = false,
            ExecutionResourceId = executionResourceId,
            PeriodStartDate = periodStart,
            PeriodEndDate = periodEnd,
            DrillDownPath = "/capacity"
        };

        if (!executionResourceId.HasValue)
        {
            return empty;
        }

        try
        {
            var capacity = await _capacityCalculatorService.GetByResourceIdAsync(
                executionResourceId.Value,
                new CapacityQueryParameters { PeriodStartDate = periodStart, PeriodEndDate = periodEnd },
                cancellationToken);

            return new MyWorkCapacitySummaryResponse
            {
                HasData = true,
                ExecutionResourceId = executionResourceId,
                PeriodStartDate = capacity.PeriodStartDate,
                PeriodEndDate = capacity.PeriodEndDate,
                TotalCapacityHours = capacity.TotalCapacityHours,
                AllocatedHours = capacity.AllocatedHours,
                AvailableHours = capacity.AvailableHours,
                UtilizationPercentage = capacity.UtilizationPercentage,
                DrillDownPath = $"/capacity?executionResourceId={executionResourceId}"
            };
        }
        catch (Exception ex) when (ex is NotFoundException or BusinessRuleException)
        {
            // Read-only dashboard: an inactive resource or an incomplete operational configuration
            // (BR-2401) must never break the page — surface an empty summary instead.
            return empty;
        }
    }

    public async Task<MyWorkWorkloadSummaryResponse> GetWorkloadAsync(
        Guid? executionResourceId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default)
    {
        var empty = new MyWorkWorkloadSummaryResponse
        {
            HasData = false,
            ExecutionResourceId = executionResourceId,
            PeriodStartDate = periodStart,
            PeriodEndDate = periodEnd,
            DrillDownPath = "/workload/history"
        };

        if (!executionResourceId.HasValue)
        {
            return empty;
        }

        try
        {
            var workload = await _workloadCalculatorService.GetByResourceIdAsync(
                executionResourceId.Value,
                new WorkloadQueryParameters { PeriodStartDate = periodStart, PeriodEndDate = periodEnd },
                cancellationToken);

            return new MyWorkWorkloadSummaryResponse
            {
                HasData = true,
                ExecutionResourceId = executionResourceId,
                PeriodStartDate = workload.PeriodStartDate,
                PeriodEndDate = workload.PeriodEndDate,
                TotalPlannedHours = workload.TotalPlannedHours,
                AssignmentCount = workload.AssignmentCount,
                WorkloadPercentage = workload.WorkloadPercentage,
                DrillDownPath = $"/workload/history?executionResourceId={executionResourceId}"
            };
        }
        catch (Exception ex) when (ex is NotFoundException or BusinessRuleException)
        {
            return empty;
        }
    }

    /// <summary>
    /// Loads the resolved Execution Resource's active Assignments (BR-2401), their still-active
    /// Tasks, and the Missions those Tasks belong to. Company isolation for Missions is inherited
    /// transitively — Mission has no CompanyId of its own, so scoping is enforced entirely by only
    /// ever reaching Missions through the caller's own Execution Resource assignments.
    /// </summary>
    private async Task<ActiveWork> LoadActiveWorkAsync(
        Guid? executionResourceId,
        CancellationToken cancellationToken)
    {
        if (!executionResourceId.HasValue)
        {
            return ActiveWork.Empty;
        }

        var assignments = await _assignmentRepository.GetAllAsync(
            new AssignmentQueryParameters { ExecutionResourceId = executionResourceId.Value },
            cancellationToken);

        var activeAssignments = assignments
            .Where(assignment =>
                !string.Equals(assignment.Status, AssignmentStatus.Completed, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(assignment.Status, AssignmentStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var assignmentByTaskId = activeAssignments
            .GroupBy(assignment => assignment.TaskId)
            .ToDictionary(group => group.Key, group => group.First());

        var taskIds = assignmentByTaskId.Keys.ToList();
        var tasks = new List<MissionTask>();
        foreach (var taskId in taskIds)
        {
            var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
            if (task is not null)
            {
                tasks.Add(task);
            }
        }

        var activeTasks = tasks
            .Where(task =>
                task.Status is null
                || (!string.Equals(task.Status.Code, MissionTaskStatus.Completed, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(task.Status.Code, MissionTaskStatus.Cancelled, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var missionIds = activeTasks.Select(task => task.MissionId).Distinct().ToList();
        var missions = new List<Mission>();
        foreach (var missionId in missionIds)
        {
            var mission = await _missionRepository.GetByIdAsync(missionId, cancellationToken);
            if (mission is not null)
            {
                missions.Add(mission);
            }
        }

        return new ActiveWork(activeTasks, missions, assignmentByTaskId);
    }

    private static bool IsSystemUser(string userId) =>
        string.Equals(userId, "system", StringComparison.OrdinalIgnoreCase);

    private sealed class ActiveWork(
        IReadOnlyList<MissionTask> tasks,
        IReadOnlyList<Mission> missions,
        IReadOnlyDictionary<Guid, Assignment> assignmentsByTaskId)
    {
        public static readonly ActiveWork Empty = new(
            Array.Empty<MissionTask>(),
            Array.Empty<Mission>(),
            new Dictionary<Guid, Assignment>());

        public IReadOnlyList<MissionTask> Tasks { get; } = tasks;

        public IReadOnlyList<Mission> Missions { get; } = missions;

        public IReadOnlyDictionary<Guid, Assignment> AssignmentsByTaskId { get; } = assignmentsByTaskId;
    }
}
