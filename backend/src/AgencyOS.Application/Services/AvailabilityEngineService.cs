using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AgencyOS.Application.Services;

public class AvailabilityEngineService : IAvailabilityEngineService
{
    private readonly ICapacityCalculatorService _capacityCalculatorService;
    private readonly IWorkloadCalculatorService _workloadCalculatorService;
    private readonly ILogger<AvailabilityEngineService> _logger;

    public AvailabilityEngineService(
        ICapacityCalculatorService capacityCalculatorService,
        IWorkloadCalculatorService workloadCalculatorService,
        ILogger<AvailabilityEngineService> logger)
    {
        _capacityCalculatorService = capacityCalculatorService;
        _workloadCalculatorService = workloadCalculatorService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AvailabilityResponse>> GetAllAsync(
        AvailabilityQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Availability calculation started for all active execution resources from {PeriodStartDate} to {PeriodEndDate}",
            parameters.PeriodStartDate,
            parameters.PeriodEndDate);

        try
        {
            var capacityParameters = MapCapacityParameters(parameters);
            var workloadParameters = MapWorkloadParameters(parameters);

            var capacities = await _capacityCalculatorService.GetAllAsync(capacityParameters, cancellationToken);
            var workloads = await _workloadCalculatorService.GetAllAsync(workloadParameters, cancellationToken);
            var workloadsByResourceId = workloads.ToDictionary(workload => workload.ExecutionResourceId);
            var periodDays = CapacityCalculation.GetInclusivePeriodDays(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate);

            var results = capacities
                .Select(capacity =>
                {
                    workloadsByResourceId.TryGetValue(capacity.ExecutionResourceId, out var workload);
                    return BuildAvailabilityResponse(capacity, workload, periodDays);
                })
                .OrderBy(response => response.NextAvailableDate ?? DateOnly.MaxValue)
                .ThenBy(response => response.ExecutionResourceName)
                .ThenBy(response => response.ExecutionResourceCode)
                .ToList();

            stopwatch.Stop();

            _logger.LogInformation(
                "Availability calculation completed for {ResourceCount} active execution resources in {ElapsedMilliseconds} ms",
                results.Count,
                stopwatch.ElapsedMilliseconds);

            return results;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Availability calculation failed for all active execution resources after {ElapsedMilliseconds} ms",
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public async Task<AvailabilityResponse> GetByResourceIdAsync(
        Guid resourceId,
        AvailabilityQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Availability calculation started for execution resource {ExecutionResourceId} from {PeriodStartDate} to {PeriodEndDate}",
            resourceId,
            parameters.PeriodStartDate,
            parameters.PeriodEndDate);

        try
        {
            var capacityParameters = MapCapacityParameters(parameters);
            var workloadParameters = MapWorkloadParameters(parameters);

            var capacity = await _capacityCalculatorService.GetByResourceIdAsync(
                resourceId,
                capacityParameters,
                cancellationToken);
            var workload = await _workloadCalculatorService.GetByResourceIdAsync(
                resourceId,
                workloadParameters,
                cancellationToken);
            var periodDays = CapacityCalculation.GetInclusivePeriodDays(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate);

            var result = BuildAvailabilityResponse(capacity, workload, periodDays);

            stopwatch.Stop();

            _logger.LogInformation(
                "Availability calculation completed for execution resource {ExecutionResourceId} in {ElapsedMilliseconds} ms",
                resourceId,
                stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Availability calculation failed for execution resource {ExecutionResourceId} after {ElapsedMilliseconds} ms",
                resourceId,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public async Task<AvailabilitySummaryResponse> GetSummaryAsync(
        AvailabilityQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Availability calculation started for summary from {PeriodStartDate} to {PeriodEndDate}",
            parameters.PeriodStartDate,
            parameters.PeriodEndDate);

        try
        {
            var availabilities = await GetAllAsync(parameters, cancellationToken);

            var totalCapacityHours = availabilities.Sum(availability =>
                availability.AvailableHours + availability.OccupiedHours);
            var totalAvailableHours = availabilities.Sum(availability => availability.AvailableHours);
            var totalOccupiedHours = availabilities.Sum(availability => availability.OccupiedHours);

            var summary = new AvailabilitySummaryResponse
            {
                PeriodStartDate = parameters.PeriodStartDate,
                PeriodEndDate = parameters.PeriodEndDate,
                ActiveResourceCount = availabilities.Count,
                TotalAvailableHours = totalAvailableHours,
                TotalOccupiedHours = totalOccupiedHours,
                OverallAvailabilityPercentage = AvailabilityCalculation.CalculateAvailabilityPercentage(
                    totalCapacityHours,
                    totalAvailableHours),
                ResourcesWithAvailability = availabilities.Count(availability => availability.AvailableHours > 0)
            };

            stopwatch.Stop();

            _logger.LogInformation(
                "Availability calculation completed for summary across {ResourceCount} active execution resources in {ElapsedMilliseconds} ms",
                summary.ActiveResourceCount,
                stopwatch.ElapsedMilliseconds);

            return summary;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Availability summary calculation failed after {ElapsedMilliseconds} ms",
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    private static AvailabilityResponse BuildAvailabilityResponse(
        CapacityResponse capacity,
        WorkloadResponse? workload,
        int periodDays)
    {
        var assignments = workload?.AssignmentDistribution ?? Array.Empty<WorkloadAssignmentDistributionItem>();
        var occupiedHours = workload?.TotalPlannedHours ?? 0m;
        var availableHours = AvailabilityCalculation.CapAvailableHours(
            capacity.AvailableHours,
            capacity.TotalCapacityHours);
        var capacityHoursPerWeek = AvailabilityCalculation.DeriveCapacityHoursPerWeek(
            capacity.TotalCapacityHours,
            periodDays);

        return new AvailabilityResponse
        {
            ExecutionResourceId = capacity.ExecutionResourceId,
            ExecutionResourceCode = capacity.ExecutionResourceCode,
            ExecutionResourceName = capacity.ExecutionResourceName,
            PeriodStartDate = capacity.PeriodStartDate,
            PeriodEndDate = capacity.PeriodEndDate,
            NextAvailableDate = AvailabilityCalculation.FindNextAvailableDate(
                capacity.PeriodStartDate,
                capacity.PeriodEndDate,
                capacityHoursPerWeek,
                assignments),
            AvailableHours = availableHours,
            OccupiedHours = occupiedHours,
            AvailabilityPercentage = AvailabilityCalculation.CalculateAvailabilityPercentage(
                capacity.TotalCapacityHours,
                availableHours),
            AvailableTimeSlots = AvailabilityCalculation.BuildAvailableTimeSlots(
                capacity.PeriodStartDate,
                capacity.PeriodEndDate,
                capacityHoursPerWeek,
                assignments)
        };
    }

    private static CapacityQueryParameters MapCapacityParameters(AvailabilityQueryParameters parameters)
    {
        return new CapacityQueryParameters
        {
            PeriodStartDate = parameters.PeriodStartDate,
            PeriodEndDate = parameters.PeriodEndDate
        };
    }

    private static WorkloadQueryParameters MapWorkloadParameters(AvailabilityQueryParameters parameters)
    {
        return new WorkloadQueryParameters
        {
            PeriodStartDate = parameters.PeriodStartDate,
            PeriodEndDate = parameters.PeriodEndDate
        };
    }
}
