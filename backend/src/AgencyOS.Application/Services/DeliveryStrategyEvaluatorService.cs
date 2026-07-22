using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AgencyOS.Application.Services;

public class DeliveryStrategyEvaluatorService : IDeliveryStrategyEvaluatorService
{
    private readonly IDeliveryStrategyBuilderService _deliveryStrategyBuilderService;
    private readonly ICapacityCalculatorService _capacityCalculatorService;
    private readonly IWorkloadCalculatorService _workloadCalculatorService;
    private readonly IAvailabilityEngineService _availabilityEngineService;
    private readonly IAllocationConflictDetectionService _allocationConflictDetectionService;
    private readonly ILogger<DeliveryStrategyEvaluatorService> _logger;

    public DeliveryStrategyEvaluatorService(
        IDeliveryStrategyBuilderService deliveryStrategyBuilderService,
        ICapacityCalculatorService capacityCalculatorService,
        IWorkloadCalculatorService workloadCalculatorService,
        IAvailabilityEngineService availabilityEngineService,
        IAllocationConflictDetectionService allocationConflictDetectionService,
        ILogger<DeliveryStrategyEvaluatorService> logger)
    {
        _deliveryStrategyBuilderService = deliveryStrategyBuilderService;
        _capacityCalculatorService = capacityCalculatorService;
        _workloadCalculatorService = workloadCalculatorService;
        _availabilityEngineService = availabilityEngineService;
        _allocationConflictDetectionService = allocationConflictDetectionService;
        _logger = logger;
    }

    public async Task<EvaluateDeliveryStrategyResponse> EvaluateAsync(
        EvaluateDeliveryStrategyRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Delivery strategy evaluation started for Contract {ContractId} and Mission {MissionId} from {PeriodStartDate} to {PeriodEndDate}",
            request.ContractId,
            request.MissionId,
            request.PeriodStartDate,
            request.PeriodEndDate);

        try
        {
            var buildRequest = new BuildDeliveryStrategyRequest
            {
                ContractId = request.ContractId,
                MissionId = request.MissionId,
                PeriodStartDate = request.PeriodStartDate,
                PeriodEndDate = request.PeriodEndDate
            };

            var buildResponse = await _deliveryStrategyBuilderService.BuildAsync(
                buildRequest,
                cancellationToken);

            if (buildResponse.Strategies.Count == 0)
            {
                throw new BusinessRuleException(
                    "No delivery strategies were generated for evaluation.");
            }

            var operationalData = await LoadOperationalDataAsync(
                request.MissionId,
                request.PeriodStartDate,
                request.PeriodEndDate,
                cancellationToken);

            var evaluatedStrategies = buildResponse.Strategies
                .Select(strategy => new EvaluatedDeliveryStrategyResponse
                {
                    Strategy = strategy,
                    EvaluationMetrics = DeliveryStrategyEvaluationCalculation.EvaluateStrategy(
                        strategy,
                        operationalData.CapacitiesByResourceId,
                        operationalData.WorkloadsByResourceId,
                        operationalData.AvailabilitiesByResourceId,
                        operationalData.ConflictsByResourceId)
                })
                .OrderBy(result => result.Strategy.StrategyName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(result => result.Strategy.StrategyId)
                .ToList();

            stopwatch.Stop();

            _logger.LogInformation(
                "Delivery strategy evaluation completed for Contract {ContractId} with {StrategyCount} strategies evaluated in {ElapsedMilliseconds} ms",
                request.ContractId,
                evaluatedStrategies.Count,
                stopwatch.ElapsedMilliseconds);

            return new EvaluateDeliveryStrategyResponse
            {
                ContractId = request.ContractId,
                MissionId = request.MissionId,
                EvaluatedStrategies = evaluatedStrategies,
                EvaluatedStrategyCount = evaluatedStrategies.Count,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex) when (ex is not BusinessRuleException and not NotFoundException)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Delivery strategy evaluation failed for Contract {ContractId} and Mission {MissionId} after {ElapsedMilliseconds} ms",
                request.ContractId,
                request.MissionId,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    private async Task<OperationalEvaluationData> LoadOperationalDataAsync(
        Guid missionId,
        DateOnly periodStartDate,
        DateOnly periodEndDate,
        CancellationToken cancellationToken)
    {
        var capacityParameters = new CapacityQueryParameters
        {
            PeriodStartDate = periodStartDate,
            PeriodEndDate = periodEndDate,
            ExcludeMissionId = missionId
        };
        var workloadParameters = new WorkloadQueryParameters
        {
            PeriodStartDate = periodStartDate,
            PeriodEndDate = periodEndDate,
            ExcludeMissionId = missionId
        };
        var availabilityParameters = new AvailabilityQueryParameters
        {
            PeriodStartDate = periodStartDate,
            PeriodEndDate = periodEndDate,
            ExcludeMissionId = missionId
        };
        var conflictParameters = new AllocationConflictQueryParameters
        {
            PeriodStartDate = periodStartDate,
            PeriodEndDate = periodEndDate,
            ExcludeMissionId = missionId
        };

        var capacities = await _capacityCalculatorService.GetAllAsync(capacityParameters, cancellationToken);
        var workloads = await _workloadCalculatorService.GetAllAsync(workloadParameters, cancellationToken);
        var availabilities = await _availabilityEngineService.GetAllAsync(
            availabilityParameters,
            cancellationToken);
        var conflicts = await _allocationConflictDetectionService.GetAllAsync(
            conflictParameters,
            cancellationToken);

        return new OperationalEvaluationData
        {
            CapacitiesByResourceId = capacities.ToDictionary(capacity => capacity.ExecutionResourceId),
            WorkloadsByResourceId = workloads.ToDictionary(workload => workload.ExecutionResourceId),
            AvailabilitiesByResourceId = availabilities.ToDictionary(
                availability => availability.ExecutionResourceId),
            ConflictsByResourceId = conflicts
                .GroupBy(conflict => conflict.ExecutionResourceId)
                .ToDictionary(group => group.Key, group => (IReadOnlyList<AllocationConflictResponse>)group.ToList())
        };
    }

    private sealed class OperationalEvaluationData
    {
        public required IReadOnlyDictionary<Guid, CapacityResponse> CapacitiesByResourceId { get; init; }

        public required IReadOnlyDictionary<Guid, WorkloadResponse> WorkloadsByResourceId { get; init; }

        public required IReadOnlyDictionary<Guid, AvailabilityResponse> AvailabilitiesByResourceId { get; init; }

        public required IReadOnlyDictionary<Guid, IReadOnlyList<AllocationConflictResponse>> ConflictsByResourceId { get; init; }
    }
}
