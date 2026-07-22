using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AgencyOS.Application.Services;

public class WorkloadCalculatorService : IWorkloadCalculatorService
{
    private readonly IExecutionResourceRepository _executionResourceRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ILogger<WorkloadCalculatorService> _logger;

    public WorkloadCalculatorService(
        IExecutionResourceRepository executionResourceRepository,
        IAssignmentRepository assignmentRepository,
        ILogger<WorkloadCalculatorService> logger)
    {
        _executionResourceRepository = executionResourceRepository;
        _assignmentRepository = assignmentRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WorkloadResponse>> GetAllAsync(
        WorkloadQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Workload calculation started for all active execution resources from {PeriodStartDate} to {PeriodEndDate}",
            parameters.PeriodStartDate,
            parameters.PeriodEndDate);

        try
        {
            var activeResources = await GetActiveResourcesAsync(cancellationToken);
            var assignments = await _assignmentRepository.GetForCapacityCalculationAsync(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                excludeMissionId: parameters.ExcludeMissionId,
                cancellationToken: cancellationToken);

            var assignmentsByResource = GroupAssignments(assignments);
            var periodDays = CapacityCalculation.GetInclusivePeriodDays(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate);

            var results = activeResources
                .Select(resource => BuildWorkloadResponse(
                    resource,
                    parameters,
                    periodDays,
                    assignmentsByResource.GetValueOrDefault(resource.Id, Array.Empty<Assignment>())))
                .OrderBy(response => response.ExecutionResourceName)
                .ThenBy(response => response.ExecutionResourceCode)
                .ToList();

            stopwatch.Stop();

            _logger.LogInformation(
                "Workload calculation completed for {ResourceCount} active execution resources in {ElapsedMilliseconds} ms",
                results.Count,
                stopwatch.ElapsedMilliseconds);

            return results;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Workload calculation failed for all active execution resources after {ElapsedMilliseconds} ms",
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public async Task<WorkloadResponse> GetByResourceIdAsync(
        Guid resourceId,
        WorkloadQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Workload calculation started for execution resource {ExecutionResourceId} from {PeriodStartDate} to {PeriodEndDate}",
            resourceId,
            parameters.PeriodStartDate,
            parameters.PeriodEndDate);

        try
        {
            var resource = await GetActiveResourceOrThrowAsync(resourceId, cancellationToken);
            var assignments = await _assignmentRepository.GetForCapacityCalculationAsync(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                executionResourceId: resourceId,
                excludeMissionId: parameters.ExcludeMissionId,
                cancellationToken: cancellationToken);

            var periodDays = CapacityCalculation.GetInclusivePeriodDays(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate);

            var result = BuildWorkloadResponse(resource, parameters, periodDays, assignments);

            stopwatch.Stop();

            _logger.LogInformation(
                "Workload calculation completed for execution resource {ExecutionResourceId} in {ElapsedMilliseconds} ms",
                resourceId,
                stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Workload calculation failed for execution resource {ExecutionResourceId} after {ElapsedMilliseconds} ms",
                resourceId,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public async Task<WorkloadSummaryResponse> GetSummaryAsync(
        WorkloadQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Workload calculation started for summary from {PeriodStartDate} to {PeriodEndDate}",
            parameters.PeriodStartDate,
            parameters.PeriodEndDate);

        try
        {
            var activeResources = await GetActiveResourcesAsync(cancellationToken);
            var assignments = await _assignmentRepository.GetForCapacityCalculationAsync(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                excludeMissionId: parameters.ExcludeMissionId,
                cancellationToken: cancellationToken);

            var assignmentsByResource = GroupAssignments(assignments);
            var periodDays = CapacityCalculation.GetInclusivePeriodDays(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate);

            var totalPlannedHours = 0m;
            var totalAssignmentCount = 0;
            var totalCapacityHours = 0m;

            foreach (var resource in activeResources)
            {
                var resourceAssignments = assignmentsByResource.GetValueOrDefault(resource.Id, Array.Empty<Assignment>());
                totalPlannedHours += resourceAssignments.Sum(assignment => assignment.PlannedHours);
                totalAssignmentCount += resourceAssignments.Count;
                totalCapacityHours += CapacityCalculation.CalculateTotalCapacityHours(
                    resource.CapacityHoursPerWeek,
                    periodDays);
            }

            var summary = new WorkloadSummaryResponse
            {
                PeriodStartDate = parameters.PeriodStartDate,
                PeriodEndDate = parameters.PeriodEndDate,
                ActiveResourceCount = activeResources.Count,
                TotalPlannedHours = totalPlannedHours,
                TotalAssignmentCount = totalAssignmentCount,
                AverageHoursPerAssignment = WorkloadCalculation.CalculateAverageHoursPerAssignment(
                    totalPlannedHours,
                    totalAssignmentCount),
                OverallWorkloadPercentage = WorkloadCalculation.CalculateWorkloadPercentage(
                    totalCapacityHours,
                    totalPlannedHours)
            };

            stopwatch.Stop();

            _logger.LogInformation(
                "Workload calculation completed for summary across {ResourceCount} active execution resources in {ElapsedMilliseconds} ms",
                summary.ActiveResourceCount,
                stopwatch.ElapsedMilliseconds);

            return summary;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Workload summary calculation failed after {ElapsedMilliseconds} ms",
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    private async Task<IReadOnlyList<ExecutionResource>> GetActiveResourcesAsync(
        CancellationToken cancellationToken)
    {
        var parameters = new ExecutionResourceQueryParameters
        {
            Status = ExecutionResourceStatus.Active
        };

        return await _executionResourceRepository.GetAllAsync(parameters, cancellationToken);
    }

    private async Task<ExecutionResource> GetActiveResourceOrThrowAsync(
        Guid resourceId,
        CancellationToken cancellationToken)
    {
        var resource = await _executionResourceRepository.GetByIdAsync(resourceId, cancellationToken);

        if (resource is null)
        {
            throw new NotFoundException($"Execution Resource with id '{resourceId}' was not found.");
        }

        if (!ExecutionResourceStatus.CanReceiveAssignments(resource.Status))
        {
            throw new NotFoundException(
                $"Workload is calculated only for Active Execution Resources. Resource '{resourceId}' is not active.");
        }

        return resource;
    }

    private static Dictionary<Guid, IReadOnlyList<Assignment>> GroupAssignments(
        IReadOnlyList<Assignment> assignments)
    {
        return assignments
            .GroupBy(assignment => assignment.ExecutionResourceId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<Assignment>)group.ToList());
    }

    private static WorkloadResponse BuildWorkloadResponse(
        ExecutionResource resource,
        WorkloadQueryParameters parameters,
        int periodDays,
        IReadOnlyList<Assignment> assignments)
    {
        var orderedAssignments = assignments
            .OrderBy(assignment => assignment.PlannedStartDate)
            .ThenBy(assignment => assignment.Id)
            .ToList();

        var totalPlannedHours = orderedAssignments.Sum(assignment => assignment.PlannedHours);
        var assignmentCount = orderedAssignments.Count;
        var totalCapacityHours = CapacityCalculation.CalculateTotalCapacityHours(
            resource.CapacityHoursPerWeek,
            periodDays);

        return new WorkloadResponse
        {
            ExecutionResourceId = resource.Id,
            ExecutionResourceCode = resource.Code,
            ExecutionResourceName = resource.Name,
            PeriodStartDate = parameters.PeriodStartDate,
            PeriodEndDate = parameters.PeriodEndDate,
            TotalPlannedHours = totalPlannedHours,
            AssignmentCount = assignmentCount,
            AverageHoursPerAssignment = WorkloadCalculation.CalculateAverageHoursPerAssignment(
                totalPlannedHours,
                assignmentCount),
            WorkloadPercentage = WorkloadCalculation.CalculateWorkloadPercentage(
                totalCapacityHours,
                totalPlannedHours),
            AssignmentDistribution = orderedAssignments
                .Select(MapAssignmentDistributionItem)
                .ToList()
        };
    }

    private static WorkloadAssignmentDistributionItem MapAssignmentDistributionItem(Assignment assignment)
    {
        return new WorkloadAssignmentDistributionItem
        {
            AssignmentId = assignment.Id,
            TaskId = assignment.TaskId,
            AssignmentRole = assignment.AssignmentRole,
            PlannedHours = assignment.PlannedHours,
            PlannedStartDate = assignment.PlannedStartDate,
            PlannedEndDate = assignment.PlannedEndDate,
            Status = assignment.Status
        };
    }
}
