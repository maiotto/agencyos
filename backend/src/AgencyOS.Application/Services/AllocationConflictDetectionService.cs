using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AgencyOS.Application.Services;

public class AllocationConflictDetectionService : IAllocationConflictDetectionService
{
    private readonly ICapacityCalculatorService _capacityCalculatorService;
    private readonly IWorkloadCalculatorService _workloadCalculatorService;
    private readonly IAvailabilityEngineService _availabilityEngineService;
    private readonly ILogger<AllocationConflictDetectionService> _logger;

    public AllocationConflictDetectionService(
        ICapacityCalculatorService capacityCalculatorService,
        IWorkloadCalculatorService workloadCalculatorService,
        IAvailabilityEngineService availabilityEngineService,
        ILogger<AllocationConflictDetectionService> logger)
    {
        _capacityCalculatorService = capacityCalculatorService;
        _workloadCalculatorService = workloadCalculatorService;
        _availabilityEngineService = availabilityEngineService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AllocationConflictResponse>> GetAllAsync(
        AllocationConflictQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Allocation conflict detection started for all active execution resources from {PeriodStartDate} to {PeriodEndDate}",
            parameters.PeriodStartDate,
            parameters.PeriodEndDate);

        try
        {
            var capacityParameters = MapCapacityParameters(parameters);
            var workloadParameters = MapWorkloadParameters(parameters);
            var availabilityParameters = MapAvailabilityParameters(parameters);

            var capacities = await _capacityCalculatorService.GetAllAsync(capacityParameters, cancellationToken);
            var workloads = await _workloadCalculatorService.GetAllAsync(workloadParameters, cancellationToken);
            var availabilities = await _availabilityEngineService.GetAllAsync(
                availabilityParameters,
                cancellationToken);

            var workloadsByResourceId = workloads.ToDictionary(workload => workload.ExecutionResourceId);
            var availabilitiesByResourceId = availabilities.ToDictionary(
                availability => availability.ExecutionResourceId);
            var periodDays = CapacityCalculation.GetInclusivePeriodDays(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate);

            var conflicts = capacities
                .SelectMany(capacity =>
                {
                    workloadsByResourceId.TryGetValue(capacity.ExecutionResourceId, out var workload);
                    availabilitiesByResourceId.TryGetValue(capacity.ExecutionResourceId, out var availability);

                    return AllocationConflictCalculation.DetectConflicts(
                        capacity,
                        workload,
                        availability,
                        periodDays);
                })
                .ToList();

            var orderedConflicts = AllocationConflictCalculation.OrderConflicts(conflicts);

            stopwatch.Stop();

            _logger.LogInformation(
                "Allocation conflict detection completed for all active execution resources with {ConflictCount} conflicts detected in {ElapsedMilliseconds} ms",
                orderedConflicts.Count,
                stopwatch.ElapsedMilliseconds);

            return orderedConflicts;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Allocation conflict detection failed for all active execution resources after {ElapsedMilliseconds} ms",
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public async Task<IReadOnlyList<AllocationConflictResponse>> GetByResourceIdAsync(
        Guid resourceId,
        AllocationConflictQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Allocation conflict detection started for execution resource {ExecutionResourceId} from {PeriodStartDate} to {PeriodEndDate}",
            resourceId,
            parameters.PeriodStartDate,
            parameters.PeriodEndDate);

        try
        {
            var capacityParameters = MapCapacityParameters(parameters);
            var workloadParameters = MapWorkloadParameters(parameters);
            var availabilityParameters = MapAvailabilityParameters(parameters);

            var capacity = await _capacityCalculatorService.GetByResourceIdAsync(
                resourceId,
                capacityParameters,
                cancellationToken);
            var workload = await _workloadCalculatorService.GetByResourceIdAsync(
                resourceId,
                workloadParameters,
                cancellationToken);
            var availability = await _availabilityEngineService.GetByResourceIdAsync(
                resourceId,
                availabilityParameters,
                cancellationToken);
            var periodDays = CapacityCalculation.GetInclusivePeriodDays(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate);

            var conflicts = AllocationConflictCalculation.DetectConflicts(
                capacity,
                workload,
                availability,
                periodDays);

            stopwatch.Stop();

            _logger.LogInformation(
                "Allocation conflict detection completed for execution resource {ExecutionResourceId} with {ConflictCount} conflicts detected in {ElapsedMilliseconds} ms",
                resourceId,
                conflicts.Count,
                stopwatch.ElapsedMilliseconds);

            return conflicts;
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Allocation conflict detection failed for execution resource {ExecutionResourceId} after {ElapsedMilliseconds} ms",
                resourceId,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public async Task<AllocationConflictSummaryResponse> GetSummaryAsync(
        AllocationConflictQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Allocation conflict detection started for summary from {PeriodStartDate} to {PeriodEndDate}",
            parameters.PeriodStartDate,
            parameters.PeriodEndDate);

        try
        {
            var capacityParameters = MapCapacityParameters(parameters);
            var capacities = await _capacityCalculatorService.GetAllAsync(capacityParameters, cancellationToken);
            var conflicts = await GetAllAsync(parameters, cancellationToken);

            var summary = new AllocationConflictSummaryResponse
            {
                PeriodStartDate = parameters.PeriodStartDate,
                PeriodEndDate = parameters.PeriodEndDate,
                ActiveResourceCount = capacities.Count,
                TotalConflictCount = conflicts.Count,
                CriticalConflictCount = AllocationConflictCalculation.CountBySeverity(
                    conflicts,
                    AllocationConflictCalculation.Severity.Critical),
                HighConflictCount = AllocationConflictCalculation.CountBySeverity(
                    conflicts,
                    AllocationConflictCalculation.Severity.High),
                MediumConflictCount = AllocationConflictCalculation.CountBySeverity(
                    conflicts,
                    AllocationConflictCalculation.Severity.Medium),
                LowConflictCount = AllocationConflictCalculation.CountBySeverity(
                    conflicts,
                    AllocationConflictCalculation.Severity.Low),
                ResourcesWithConflicts = AllocationConflictCalculation.CountResourcesWithConflicts(conflicts)
            };

            stopwatch.Stop();

            _logger.LogInformation(
                "Allocation conflict detection completed for summary with {ConflictCount} conflicts detected in {ElapsedMilliseconds} ms",
                summary.TotalConflictCount,
                stopwatch.ElapsedMilliseconds);

            return summary;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Allocation conflict summary detection failed after {ElapsedMilliseconds} ms",
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    private static CapacityQueryParameters MapCapacityParameters(AllocationConflictQueryParameters parameters)
    {
        return new CapacityQueryParameters
        {
            PeriodStartDate = parameters.PeriodStartDate,
            PeriodEndDate = parameters.PeriodEndDate
        };
    }

    private static WorkloadQueryParameters MapWorkloadParameters(AllocationConflictQueryParameters parameters)
    {
        return new WorkloadQueryParameters
        {
            PeriodStartDate = parameters.PeriodStartDate,
            PeriodEndDate = parameters.PeriodEndDate
        };
    }

    private static AvailabilityQueryParameters MapAvailabilityParameters(
        AllocationConflictQueryParameters parameters)
    {
        return new AvailabilityQueryParameters
        {
            PeriodStartDate = parameters.PeriodStartDate,
            PeriodEndDate = parameters.PeriodEndDate
        };
    }
}
