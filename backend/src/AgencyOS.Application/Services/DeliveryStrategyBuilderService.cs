using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AgencyOS.Application.Services;

public class DeliveryStrategyBuilderService : IDeliveryStrategyBuilderService
{
    private readonly IClientContractRepository _contractRepository;
    private readonly IMissionRepository _missionRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IExecutionResourceRepository _executionResourceRepository;
    private readonly ICapacityCalculatorService _capacityCalculatorService;
    private readonly IWorkloadCalculatorService _workloadCalculatorService;
    private readonly IAvailabilityEngineService _availabilityEngineService;
    private readonly IAllocationConflictDetectionService _allocationConflictDetectionService;
    private readonly ILogger<DeliveryStrategyBuilderService> _logger;

    public DeliveryStrategyBuilderService(
        IClientContractRepository contractRepository,
        IMissionRepository missionRepository,
        ITaskRepository taskRepository,
        IExecutionResourceRepository executionResourceRepository,
        ICapacityCalculatorService capacityCalculatorService,
        IWorkloadCalculatorService workloadCalculatorService,
        IAvailabilityEngineService availabilityEngineService,
        IAllocationConflictDetectionService allocationConflictDetectionService,
        ILogger<DeliveryStrategyBuilderService> logger)
    {
        _contractRepository = contractRepository;
        _missionRepository = missionRepository;
        _taskRepository = taskRepository;
        _executionResourceRepository = executionResourceRepository;
        _capacityCalculatorService = capacityCalculatorService;
        _workloadCalculatorService = workloadCalculatorService;
        _availabilityEngineService = availabilityEngineService;
        _allocationConflictDetectionService = allocationConflictDetectionService;
        _logger = logger;
    }

    public async Task<BuildDeliveryStrategyResponse> BuildAsync(
        BuildDeliveryStrategyRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Delivery strategy generation started for Contract {ContractId} and Mission {MissionId} from {PeriodStartDate} to {PeriodEndDate}",
            request.ContractId,
            request.MissionId,
            request.PeriodStartDate,
            request.PeriodEndDate);

        try
        {
            var strategies = await GenerateStrategiesAsync(
                request.ContractId,
                request.MissionId,
                request.PeriodStartDate,
                request.PeriodEndDate,
                cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "Delivery strategy generation completed for Contract {ContractId} with {StrategyCount} strategies generated in {ElapsedMilliseconds} ms",
                request.ContractId,
                strategies.Count,
                stopwatch.ElapsedMilliseconds);

            return new BuildDeliveryStrategyResponse
            {
                ContractId = request.ContractId,
                MissionId = request.MissionId,
                Strategies = strategies,
                GeneratedStrategyCount = strategies.Count,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Delivery strategy generation failed for Contract {ContractId} and Mission {MissionId} after {ElapsedMilliseconds} ms",
                request.ContractId,
                request.MissionId,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public async Task<IReadOnlyList<DeliveryStrategyResponse>> GetByContractIdAsync(
        Guid contractId,
        DeliveryStrategyQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Delivery strategy generation started for Contract {ContractId} and Mission {MissionId} from {PeriodStartDate} to {PeriodEndDate}",
            contractId,
            parameters.MissionId,
            parameters.PeriodStartDate,
            parameters.PeriodEndDate);

        try
        {
            var strategies = await GenerateStrategiesAsync(
                contractId,
                parameters.MissionId,
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "Delivery strategy generation completed for Contract {ContractId} with {StrategyCount} strategies generated in {ElapsedMilliseconds} ms",
                contractId,
                strategies.Count,
                stopwatch.ElapsedMilliseconds);

            return strategies;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Delivery strategy generation failed for Contract {ContractId} and Mission {MissionId} after {ElapsedMilliseconds} ms",
                contractId,
                parameters.MissionId,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    private async Task<IReadOnlyList<DeliveryStrategyResponse>> GenerateStrategiesAsync(
        Guid contractId,
        Guid missionId,
        DateOnly periodStartDate,
        DateOnly periodEndDate,
        CancellationToken cancellationToken)
    {
        var contract = await GetContractOrThrowAsync(contractId, cancellationToken);
        var mission = await GetMissionForContractOrThrowAsync(missionId, contractId, cancellationToken);
        var tasks = await GetTasksForMissionOrThrowAsync(mission.Id, cancellationToken);
        var activeResources = await GetActiveResourcesAsync(cancellationToken);
        var policies = ResolvePolicies(contract);
        var availableResourceTypes = DeliveryStrategyPolicyResolution.GetAvailableResourceTypes(
            activeResources,
            policies);

        var conflictParameters = new AllocationConflictQueryParameters
        {
            PeriodStartDate = periodStartDate,
            PeriodEndDate = periodEndDate
        };
        var capacityParameters = new CapacityQueryParameters
        {
            PeriodStartDate = periodStartDate,
            PeriodEndDate = periodEndDate
        };
        var workloadParameters = new WorkloadQueryParameters
        {
            PeriodStartDate = periodStartDate,
            PeriodEndDate = periodEndDate
        };
        var availabilityParameters = new AvailabilityQueryParameters
        {
            PeriodStartDate = periodStartDate,
            PeriodEndDate = periodEndDate
        };

        var capacities = await _capacityCalculatorService.GetAllAsync(capacityParameters, cancellationToken);
        var workloads = await _workloadCalculatorService.GetAllAsync(workloadParameters, cancellationToken);
        var availabilities = await _availabilityEngineService.GetAllAsync(
            availabilityParameters,
            cancellationToken);
        var conflicts = await _allocationConflictDetectionService.GetAllAsync(
            conflictParameters,
            cancellationToken);

        var capacitiesByResourceId = capacities.ToDictionary(capacity => capacity.ExecutionResourceId);
        var workloadsByResourceId = workloads.ToDictionary(workload => workload.ExecutionResourceId);
        var availabilitiesByResourceId = availabilities.ToDictionary(availability => availability.ExecutionResourceId);
        var conflictsByResourceId = conflicts
            .GroupBy(conflict => conflict.ExecutionResourceId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<AllocationConflictResponse>)group.ToList());

        var planningMetadataTemplate = new DeliveryStrategyPlanningMetadataResponse
        {
            PeriodStartDate = periodStartDate,
            PeriodEndDate = periodEndDate,
            TaskCount = tasks.Count,
            ActiveResourceCount = activeResources.Count,
            DetectedConflictCount = conflicts.Count,
            AppliedCompanyPolicyRules = policies.AppliedCompanyPolicyRules,
            AppliedClientPolicyRules = policies.AppliedClientPolicyRules
        };

        if (availableResourceTypes.Count == 0)
        {
            return [];
        }

        var strategies = new List<DeliveryStrategyResponse>();

        foreach (var resourceMix in DeliveryStrategyGeneration.GenerateResourceMixSubsets(availableResourceTypes))
        {
            if (!DeliveryStrategyPolicyResolution.IsValidResourceMix(resourceMix, policies))
            {
                continue;
            }

            var assignments = DeliveryStrategyGeneration.AssignResourcesToTasks(
                resourceMix,
                tasks,
                activeResources);

            if (assignments.Count != tasks.Count)
            {
                continue;
            }

            if (!DeliveryStrategyGeneration.IsOperationallyValid(
                    assignments,
                    capacitiesByResourceId,
                    workloadsByResourceId,
                    availabilitiesByResourceId,
                    conflictsByResourceId))
            {
                continue;
            }

            strategies.Add(
                DeliveryStrategyGeneration.BuildStrategyResponse(
                    contract.Id,
                    mission.Id,
                    resourceMix,
                    assignments,
                    planningMetadataTemplate));
        }

        return DeliveryStrategyGeneration.OrderStrategies(strategies);
    }

    private async Task<ClientContract> GetContractOrThrowAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        var contract = await _contractRepository.GetByIdAsync(contractId, cancellationToken);

        if (contract is null)
        {
            throw new NotFoundException($"Contract with id '{contractId}' was not found.");
        }

        return contract;
    }

    private async Task<Mission> GetMissionForContractOrThrowAsync(
        Guid missionId,
        Guid contractId,
        CancellationToken cancellationToken)
    {
        var mission = await _missionRepository.GetByIdAsync(missionId, cancellationToken);

        if (mission is null)
        {
            throw new NotFoundException($"Mission with id '{missionId}' was not found.");
        }

        if (mission.ClientContractId != contractId)
        {
            throw new BusinessRuleException(
                $"Mission with id '{missionId}' does not belong to Contract with id '{contractId}'.");
        }

        return mission;
    }

    private async Task<IReadOnlyList<MissionTask>> GetTasksForMissionOrThrowAsync(
        Guid missionId,
        CancellationToken cancellationToken)
    {
        var tasks = await _taskRepository.GetAllAsync(
            new TaskQueryParameters { MissionId = missionId },
            cancellationToken);

        if (tasks.Count == 0)
        {
            throw new BusinessRuleException(
                $"Mission with id '{missionId}' does not contain any Tasks required for strategy generation.");
        }

        return tasks;
    }

    private async Task<IReadOnlyList<ExecutionResource>> GetActiveResourcesAsync(
        CancellationToken cancellationToken)
    {
        return await _executionResourceRepository.GetAllAsync(
            new ExecutionResourceQueryParameters
            {
                Status = ExecutionResourceStatus.Active
            },
            cancellationToken);
    }

    private static DeliveryStrategyPolicyContext ResolvePolicies(ClientContract contract)
    {
        var companyPolicy = DeliveryStrategyPolicyResolution.ResolveCompanyPolicy();
        var clientPolicy = DeliveryStrategyPolicyResolution.ResolveClientPolicy(contract);

        return DeliveryStrategyPolicyResolution.MergePolicies(companyPolicy, clientPolicy);
    }
}
