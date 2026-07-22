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

        _logger.LogInformation(
            "Delivery strategy diagnostics input. ContractId={ContractId}, MissionId={MissionId}, TaskCount={TaskCount}, ActiveResourceCount={ActiveResourceCount}, CompanyPolicies={CompanyPolicies}, ClientPolicies={ClientPolicies}, AvailableResourceTypes={AvailableResourceTypes}",
            contract.Id,
            mission.Id,
            tasks.Count,
            activeResources.Count,
            policies.AppliedCompanyPolicyRules,
            policies.AppliedClientPolicyRules,
            availableResourceTypes);

        var conflictParameters = new AllocationConflictQueryParameters
        {
            PeriodStartDate = periodStartDate,
            PeriodEndDate = periodEndDate,
            ExcludeMissionId = missionId
        };
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
            _logger.LogInformation(
                "Delivery strategy diagnostics summary. ContractId={ContractId}, MissionId={MissionId}, TotalCandidateMixes={TotalCandidateMixes}, ValidMixes={ValidMixes}, OperationallyValidMixes={OperationallyValidMixes}, FinalStrategies={FinalStrategies}",
                contract.Id,
                mission.Id,
                0,
                0,
                0,
                0);

            return [];
        }

        var resourceMixes = DeliveryStrategyGeneration
            .GenerateResourceMixSubsets(availableResourceTypes)
            .ToList();

        _logger.LogInformation(
            "Delivery strategy diagnostics ResourceMixCount={ResourceMixCount} for ContractId={ContractId}, MissionId={MissionId}",
            resourceMixes.Count,
            contract.Id,
            mission.Id);

        var strategies = new List<DeliveryStrategyResponse>();
        var validMixCount = 0;
        var operationallyValidMixCount = 0;

        foreach (var resourceMix in resourceMixes)
        {
            var resourceMixLabel = FormatResourceMix(resourceMix);
            var isValidResourceMix = DeliveryStrategyPolicyResolution.IsValidResourceMix(resourceMix, policies);

            _logger.LogInformation(
                "Delivery strategy diagnostics resource mix evaluation. ContractId={ContractId}, MissionId={MissionId}, ResourceMix={ResourceMix}, IsValidResourceMix={IsValidResourceMix}",
                contract.Id,
                mission.Id,
                resourceMixLabel,
                isValidResourceMix);

            if (!isValidResourceMix)
            {
                _logger.LogInformation(
                    "Delivery strategy diagnostics invalid resource mix. ContractId={ContractId}, MissionId={MissionId}, ResourceMix={ResourceMix}, Reason={Reason}",
                    contract.Id,
                    mission.Id,
                    resourceMixLabel,
                    DescribeInvalidResourceMixReason(resourceMix, policies));
                continue;
            }

            validMixCount++;

            var assignments = DeliveryStrategyGeneration.AssignResourcesToTasks(
                resourceMix,
                tasks,
                activeResources);

            _logger.LogInformation(
                "Delivery strategy diagnostics assignment result. ContractId={ContractId}, MissionId={MissionId}, ResourceMix={ResourceMix}, AssignedTaskCount={AssignedTaskCount}, ExpectedTaskCount={ExpectedTaskCount}",
                contract.Id,
                mission.Id,
                resourceMixLabel,
                assignments.Count,
                tasks.Count);

            if (assignments.Count != tasks.Count)
            {
                _logger.LogInformation(
                    "Delivery strategy diagnostics assignment failed. ContractId={ContractId}, MissionId={MissionId}, ResourceMix={ResourceMix}, Reason={Reason}",
                    contract.Id,
                    mission.Id,
                    resourceMixLabel,
                    DescribeAssignmentFailureReason(resourceMix, tasks, activeResources, assignments.Count));
                continue;
            }

            _logger.LogInformation(
                "Delivery strategy diagnostics operational inputs. ContractId={ContractId}, MissionId={MissionId}, ResourceMix={ResourceMix}, CapacitySummary={CapacitySummary}, WorkloadSummary={WorkloadSummary}, AvailabilitySummary={AvailabilitySummary}, ConflictCount={ConflictCount}",
                contract.Id,
                mission.Id,
                resourceMixLabel,
                BuildCapacitySummary(assignments, capacitiesByResourceId),
                BuildWorkloadSummary(assignments, workloadsByResourceId),
                BuildAvailabilitySummary(assignments, availabilitiesByResourceId),
                CountConflictsForAssignments(assignments, conflictsByResourceId));

            var isOperationallyValid = DeliveryStrategyGeneration.IsOperationallyValid(
                assignments,
                capacitiesByResourceId,
                workloadsByResourceId,
                availabilitiesByResourceId,
                conflictsByResourceId);

            if (!isOperationallyValid)
            {
                _logger.LogInformation(
                    "Delivery strategy diagnostics operational validation failed. ContractId={ContractId}, MissionId={MissionId}, ResourceMix={ResourceMix}, FailedValidation={FailedValidation}",
                    contract.Id,
                    mission.Id,
                    resourceMixLabel,
                    DescribeOperationalValidationFailure(
                        assignments,
                        capacitiesByResourceId,
                        workloadsByResourceId,
                        availabilitiesByResourceId,
                        conflictsByResourceId));
                continue;
            }

            operationallyValidMixCount++;

            var strategy = DeliveryStrategyGeneration.BuildStrategyResponse(
                contract.Id,
                mission.Id,
                resourceMix,
                assignments,
                planningMetadataTemplate);

            _logger.LogInformation(
                "Delivery strategy diagnostics strategy created. ContractId={ContractId}, MissionId={MissionId}, StrategyId={StrategyId}, ResourceMix={ResourceMix}, AssignmentCount={AssignmentCount}",
                contract.Id,
                mission.Id,
                strategy.StrategyId,
                resourceMixLabel,
                strategy.AssignedResources.Count);

            strategies.Add(strategy);
        }

        var orderedStrategies = DeliveryStrategyGeneration.OrderStrategies(strategies);

        _logger.LogInformation(
            "Delivery strategy diagnostics summary. ContractId={ContractId}, MissionId={MissionId}, TotalCandidateMixes={TotalCandidateMixes}, ValidMixes={ValidMixes}, OperationallyValidMixes={OperationallyValidMixes}, FinalStrategies={FinalStrategies}",
            contract.Id,
            mission.Id,
            resourceMixes.Count,
            validMixCount,
            operationallyValidMixCount,
            orderedStrategies.Count);

        return orderedStrategies;
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

    private static string FormatResourceMix(IReadOnlySet<string> resourceMix)
    {
        return string.Join(
            " + ",
            resourceMix.OrderBy(type => type, StringComparer.OrdinalIgnoreCase));
    }

    private static string DescribeInvalidResourceMixReason(
        IReadOnlySet<string> resourceMix,
        DeliveryStrategyPolicyContext policies)
    {
        if (resourceMix.Count == 0)
        {
            return "Resource mix is empty.";
        }

        var disallowedTypes = resourceMix
            .Where(type => !policies.AllowedResourceTypes.Contains(type)
                || policies.DisallowedResourceTypes.Contains(type))
            .OrderBy(type => type, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (disallowedTypes.Count > 0)
        {
            return $"Resource mix contains disallowed or non-allowed types: {string.Join(", ", disallowedTypes)}.";
        }

        if (policies.RequireHumanReviewForAiAutomation
            && resourceMix.Any(IsAiAutomationResourceType)
            && !resourceMix.Any(IsHumanResourceType))
        {
            return "AI/Automation resource mix requires human review coverage.";
        }

        return "Resource mix failed policy validation.";
    }

    private static string DescribeAssignmentFailureReason(
        IReadOnlySet<string> resourceMix,
        IReadOnlyList<MissionTask> tasks,
        IReadOnlyList<ExecutionResource> activeResources,
        int assignedTaskCount)
    {
        if (assignedTaskCount == 0)
        {
            var orderedTypes = resourceMix
                .OrderBy(type => type, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var resourcesByType = activeResources
                .Where(resource => resourceMix.Contains(resource.ResourceType))
                .GroupBy(resource => resource.ResourceType, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count(),
                    StringComparer.OrdinalIgnoreCase);

            for (var taskIndex = 0; taskIndex < tasks.Count; taskIndex++)
            {
                var resourceType = orderedTypes[taskIndex % orderedTypes.Count];

                if (!resourcesByType.TryGetValue(resourceType, out var resourceCount) || resourceCount == 0)
                {
                    return $"No active resources available for required resource type '{resourceType}' while assigning tasks.";
                }
            }

            return "Assignment returned no task assignments.";
        }

        return $"Assigned {assignedTaskCount} of {tasks.Count} required tasks.";
    }

    private static string BuildCapacitySummary(
        IReadOnlyList<DeliveryStrategyTaskAssignmentCandidate> assignments,
        IReadOnlyDictionary<Guid, CapacityResponse> capacitiesByResourceId)
    {
        var resourceIds = assignments
            .Select(assignment => assignment.ExecutionResourceId)
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        var parts = resourceIds.Select(resourceId =>
        {
            if (!capacitiesByResourceId.TryGetValue(resourceId, out var capacity))
            {
                return $"{resourceId}:missing";
            }

            return $"{capacity.ExecutionResourceCode}:available={capacity.AvailableHours},total={capacity.TotalCapacityHours},allocated={capacity.AllocatedHours}";
        });

        return string.Join("; ", parts);
    }

    private static string BuildWorkloadSummary(
        IReadOnlyList<DeliveryStrategyTaskAssignmentCandidate> assignments,
        IReadOnlyDictionary<Guid, WorkloadResponse> workloadsByResourceId)
    {
        var resourceIds = assignments
            .Select(assignment => assignment.ExecutionResourceId)
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        var parts = resourceIds.Select(resourceId =>
        {
            if (!workloadsByResourceId.TryGetValue(resourceId, out var workload))
            {
                return $"{resourceId}:missing";
            }

            return $"{workload.ExecutionResourceCode}:planned={workload.TotalPlannedHours},assignments={workload.AssignmentCount},workload%={workload.WorkloadPercentage}";
        });

        return string.Join("; ", parts);
    }

    private static string BuildAvailabilitySummary(
        IReadOnlyList<DeliveryStrategyTaskAssignmentCandidate> assignments,
        IReadOnlyDictionary<Guid, AvailabilityResponse> availabilitiesByResourceId)
    {
        var resourceIds = assignments
            .Select(assignment => assignment.ExecutionResourceId)
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        var parts = resourceIds.Select(resourceId =>
        {
            if (!availabilitiesByResourceId.TryGetValue(resourceId, out var availability))
            {
                return $"{resourceId}:missing";
            }

            return $"{availability.ExecutionResourceCode}:available={availability.AvailableHours},occupied={availability.OccupiedHours},availability%={availability.AvailabilityPercentage}";
        });

        return string.Join("; ", parts);
    }

    private static int CountConflictsForAssignments(
        IReadOnlyList<DeliveryStrategyTaskAssignmentCandidate> assignments,
        IReadOnlyDictionary<Guid, IReadOnlyList<AllocationConflictResponse>> conflictsByResourceId)
    {
        return assignments
            .Select(assignment => assignment.ExecutionResourceId)
            .Distinct()
            .Sum(resourceId =>
                conflictsByResourceId.TryGetValue(resourceId, out var conflicts)
                    ? conflicts.Count
                    : 0);
    }

    private static string DescribeOperationalValidationFailure(
        IReadOnlyList<DeliveryStrategyTaskAssignmentCandidate> assignments,
        IReadOnlyDictionary<Guid, CapacityResponse> capacitiesByResourceId,
        IReadOnlyDictionary<Guid, WorkloadResponse> workloadsByResourceId,
        IReadOnlyDictionary<Guid, AvailabilityResponse> availabilitiesByResourceId,
        IReadOnlyDictionary<Guid, IReadOnlyList<AllocationConflictResponse>> conflictsByResourceId)
    {
        if (assignments.Count == 0)
        {
            return "Assignments are empty.";
        }

        var additionalHoursByResource = assignments
            .GroupBy(assignment => assignment.ExecutionResourceId)
            .ToDictionary(group => group.Key, group => group.Sum(assignment => assignment.PlannedHours));

        foreach (var (resourceId, additionalHours) in additionalHoursByResource)
        {
            var resourceLabel = assignments
                .First(assignment => assignment.ExecutionResourceId == resourceId)
                .ExecutionResourceCode;

            if (!capacitiesByResourceId.TryGetValue(resourceId, out var capacity))
            {
                return $"Missing capacity for resource '{resourceLabel}' ({resourceId}).";
            }

            if (capacity.AvailableHours < additionalHours)
            {
                return $"Insufficient capacity for resource '{resourceLabel}': available={capacity.AvailableHours}, required={additionalHours}.";
            }

            if (workloadsByResourceId.TryGetValue(resourceId, out var workload))
            {
                var projectedPlannedHours = workload.TotalPlannedHours + additionalHours;

                if (capacity.TotalCapacityHours > 0
                    && projectedPlannedHours > capacity.TotalCapacityHours)
                {
                    return $"Projected workload exceeds total capacity for resource '{resourceLabel}': projected={projectedPlannedHours}, totalCapacity={capacity.TotalCapacityHours}.";
                }
            }

            if (availabilitiesByResourceId.TryGetValue(resourceId, out var availability)
                && availability.AvailableHours < additionalHours)
            {
                return $"Insufficient availability for resource '{resourceLabel}': available={availability.AvailableHours}, required={additionalHours}.";
            }

            if (conflictsByResourceId.TryGetValue(resourceId, out var conflicts)
                && conflicts.Any(conflict =>
                    string.Equals(conflict.Severity, AllocationConflictCalculation.Severity.Critical, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(conflict.Severity, AllocationConflictCalculation.Severity.High, StringComparison.OrdinalIgnoreCase)))
            {
                var blockingSeverities = conflicts
                    .Where(conflict =>
                        string.Equals(conflict.Severity, AllocationConflictCalculation.Severity.Critical, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(conflict.Severity, AllocationConflictCalculation.Severity.High, StringComparison.OrdinalIgnoreCase))
                    .Select(conflict => conflict.Severity)
                    .Distinct(StringComparer.OrdinalIgnoreCase);

                return $"Blocking allocation conflicts for resource '{resourceLabel}': {string.Join(", ", blockingSeverities)}.";
            }
        }

        return "Operational validation failed.";
    }

    private static bool IsHumanResourceType(string resourceType)
    {
        return string.Equals(resourceType, ExecutionResourceType.InternalHuman, StringComparison.OrdinalIgnoreCase)
            || string.Equals(resourceType, ExecutionResourceType.ExternalHuman, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAiAutomationResourceType(string resourceType)
    {
        return string.Equals(resourceType, ExecutionResourceType.AiAgent, StringComparison.OrdinalIgnoreCase)
            || string.Equals(resourceType, ExecutionResourceType.AiService, StringComparison.OrdinalIgnoreCase)
            || string.Equals(resourceType, ExecutionResourceType.Automation, StringComparison.OrdinalIgnoreCase);
    }
}
