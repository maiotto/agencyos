using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AgencyOS.Application.Services;

public class CapacityCalculatorService : ICapacityCalculatorService
{
    private readonly IExecutionResourceRepository _executionResourceRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ILogger<CapacityCalculatorService> _logger;

    public CapacityCalculatorService(
        IExecutionResourceRepository executionResourceRepository,
        IAssignmentRepository assignmentRepository,
        ILogger<CapacityCalculatorService> logger)
    {
        _executionResourceRepository = executionResourceRepository;
        _assignmentRepository = assignmentRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CapacityResponse>> GetAllAsync(
        CapacityQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Capacity calculation started for all active execution resources from {PeriodStartDate} to {PeriodEndDate}",
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

            var allocatedHoursByResource = GroupAllocatedHours(assignments);
            var periodDays = CapacityCalculation.GetInclusivePeriodDays(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate);

            var results = activeResources
                .Select(resource => BuildCapacityResponse(
                    resource,
                    parameters,
                    periodDays,
                    allocatedHoursByResource.GetValueOrDefault(resource.Id, 0)))
                .OrderBy(response => response.ExecutionResourceName)
                .ThenBy(response => response.ExecutionResourceCode)
                .ToList();

            stopwatch.Stop();

            _logger.LogInformation(
                "Capacity calculation completed for {ResourceCount} active execution resources in {ElapsedMilliseconds} ms",
                results.Count,
                stopwatch.ElapsedMilliseconds);

            return results;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Capacity calculation failed for all active execution resources after {ElapsedMilliseconds} ms",
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public async Task<CapacityResponse> GetByResourceIdAsync(
        Guid resourceId,
        CapacityQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Capacity calculation started for execution resource {ExecutionResourceId} from {PeriodStartDate} to {PeriodEndDate}",
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

            var allocatedHours = assignments.Sum(a => a.PlannedHours);
            var result = BuildCapacityResponse(resource, parameters, periodDays, allocatedHours);

            stopwatch.Stop();

            _logger.LogInformation(
                "Capacity calculation completed for execution resource {ExecutionResourceId} in {ElapsedMilliseconds} ms",
                resourceId,
                stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Capacity calculation failed for execution resource {ExecutionResourceId} after {ElapsedMilliseconds} ms",
                resourceId,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public async Task<CapacitySummaryResponse> GetSummaryAsync(
        CapacityQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var capacities = await GetAllAsync(parameters, cancellationToken);

        var totalCapacityHours = capacities.Sum(c => c.TotalCapacityHours);
        var totalAllocatedHours = capacities.Sum(c => c.AllocatedHours);
        var totalAvailableHours = capacities.Sum(c => c.AvailableHours);
        var totalRemainingCapacityHours = capacities.Sum(c => c.RemainingCapacityHours);

        return new CapacitySummaryResponse
        {
            PeriodStartDate = parameters.PeriodStartDate,
            PeriodEndDate = parameters.PeriodEndDate,
            ActiveResourceCount = capacities.Count,
            TotalCapacityHours = totalCapacityHours,
            TotalAllocatedHours = totalAllocatedHours,
            TotalAvailableHours = totalAvailableHours,
            OverallUtilizationPercentage = CapacityCalculation.CalculateUtilizationPercentage(
                totalCapacityHours,
                totalAllocatedHours),
            TotalRemainingCapacityHours = totalRemainingCapacityHours
        };
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
                $"Capacity is calculated only for Active Execution Resources. Resource '{resourceId}' is not active.");
        }

        return resource;
    }

    private static Dictionary<Guid, decimal> GroupAllocatedHours(IReadOnlyList<Assignment> assignments)
    {
        return assignments
            .GroupBy(assignment => assignment.ExecutionResourceId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(assignment => assignment.PlannedHours));
    }

    private static CapacityResponse BuildCapacityResponse(
        ExecutionResource resource,
        CapacityQueryParameters parameters,
        int periodDays,
        decimal allocatedHours)
    {
        var totalCapacityHours = CapacityCalculation.CalculateTotalCapacityHours(
            resource.CapacityHoursPerWeek,
            periodDays);
        var availableHours = CapacityCalculation.CalculateAvailableHours(totalCapacityHours, allocatedHours);

        return new CapacityResponse
        {
            ExecutionResourceId = resource.Id,
            ExecutionResourceCode = resource.Code,
            ExecutionResourceName = resource.Name,
            PeriodStartDate = parameters.PeriodStartDate,
            PeriodEndDate = parameters.PeriodEndDate,
            TotalCapacityHours = totalCapacityHours,
            AllocatedHours = allocatedHours,
            AvailableHours = availableHours,
            UtilizationPercentage = CapacityCalculation.CalculateUtilizationPercentage(
                totalCapacityHours,
                allocatedHours),
            RemainingCapacityHours = availableHours
        };
    }
}
