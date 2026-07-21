using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace AgencyOS.Application.Services;

public sealed class DeliveryStrategyTaskAssignmentCandidate
{
    public Guid TaskId { get; init; }

    public string TaskCode { get; init; } = string.Empty;

    public string TaskName { get; init; } = string.Empty;

    public Guid ExecutionResourceId { get; init; }

    public string ExecutionResourceCode { get; init; } = string.Empty;

    public string ExecutionResourceName { get; init; } = string.Empty;

    public string ResourceType { get; init; } = string.Empty;

    public decimal PlannedHours { get; init; }

    public decimal CostRate { get; init; }
}

public static class DeliveryStrategyGeneration
{
    public static IEnumerable<IReadOnlySet<string>> GenerateResourceMixSubsets(
        IReadOnlyList<string> availableResourceTypes)
    {
        var typeCount = availableResourceTypes.Count;
        var subsetCount = 1 << typeCount;

        for (var mask = 1; mask < subsetCount; mask++)
        {
            var subset = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < typeCount; index++)
            {
                if ((mask & (1 << index)) != 0)
                {
                    subset.Add(availableResourceTypes[index]);
                }
            }

            yield return subset;
        }
    }

    public static string BuildStrategyName(IEnumerable<string> resourceTypes)
    {
        return string.Join(
            " + ",
            resourceTypes.OrderBy(type => type, StringComparer.OrdinalIgnoreCase));
    }

    public static Guid CreateDeterministicStrategyId(
        Guid contractId,
        Guid missionId,
        IEnumerable<string> resourceTypes)
    {
        var mixKey = string.Join(
            "|",
            resourceTypes.OrderBy(type => type, StringComparer.OrdinalIgnoreCase));
        var hashInput = $"{contractId:N}:{missionId:N}:{mixKey}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(hashInput));

        return new Guid(hash.AsSpan(0, 16));
    }

    public static IReadOnlyList<DeliveryStrategyTaskAssignmentCandidate> AssignResourcesToTasks(
        IReadOnlySet<string> resourceMix,
        IReadOnlyList<MissionTask> tasks,
        IReadOnlyList<ExecutionResource> activeResources)
    {
        var orderedTasks = tasks
            .OrderBy(task => task.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(task => task.Id)
            .ToList();
        var orderedTypes = resourceMix
            .OrderBy(type => type, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var resourcesByType = activeResources
            .Where(resource => resourceMix.Contains(resource.ResourceType))
            .GroupBy(resource => resource.ResourceType, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(resource => resource.Code, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(resource => resource.Id)
                    .ToList(),
                StringComparer.OrdinalIgnoreCase);

        var assignments = new List<DeliveryStrategyTaskAssignmentCandidate>();

        for (var taskIndex = 0; taskIndex < orderedTasks.Count; taskIndex++)
        {
            var task = orderedTasks[taskIndex];
            var resourceType = orderedTypes[taskIndex % orderedTypes.Count];

            if (!resourcesByType.TryGetValue(resourceType, out var resources) || resources.Count == 0)
            {
                return [];
            }

            var resource = resources[taskIndex / orderedTypes.Count % resources.Count];

            assignments.Add(new DeliveryStrategyTaskAssignmentCandidate
            {
                TaskId = task.Id,
                TaskCode = task.Code,
                TaskName = task.Name,
                ExecutionResourceId = resource.Id,
                ExecutionResourceCode = resource.Code,
                ExecutionResourceName = resource.Name,
                ResourceType = resource.ResourceType,
                PlannedHours = task.EstimatedHours,
                CostRate = resource.CostRate ?? 0
            });
        }

        return assignments;
    }

    public static bool IsOperationallyValid(
        IReadOnlyList<DeliveryStrategyTaskAssignmentCandidate> assignments,
        IReadOnlyDictionary<Guid, CapacityResponse> capacitiesByResourceId,
        IReadOnlyDictionary<Guid, WorkloadResponse> workloadsByResourceId,
        IReadOnlyDictionary<Guid, AvailabilityResponse> availabilitiesByResourceId,
        IReadOnlyDictionary<Guid, IReadOnlyList<AllocationConflictResponse>> conflictsByResourceId)
    {
        if (assignments.Count == 0)
        {
            return false;
        }

        var additionalHoursByResource = assignments
            .GroupBy(assignment => assignment.ExecutionResourceId)
            .ToDictionary(group => group.Key, group => group.Sum(assignment => assignment.PlannedHours));

        foreach (var (resourceId, additionalHours) in additionalHoursByResource)
        {
            if (!capacitiesByResourceId.TryGetValue(resourceId, out var capacity))
            {
                return false;
            }

            if (capacity.AvailableHours < additionalHours)
            {
                return false;
            }

            if (workloadsByResourceId.TryGetValue(resourceId, out var workload))
            {
                var projectedPlannedHours = workload.TotalPlannedHours + additionalHours;

                if (capacity.TotalCapacityHours > 0
                    && projectedPlannedHours > capacity.TotalCapacityHours)
                {
                    return false;
                }
            }

            if (availabilitiesByResourceId.TryGetValue(resourceId, out var availability)
                && availability.AvailableHours < additionalHours)
            {
                return false;
            }

            if (conflictsByResourceId.TryGetValue(resourceId, out var conflicts)
                && conflicts.Any(conflict =>
                    string.Equals(conflict.Severity, AllocationConflictCalculation.Severity.Critical, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(conflict.Severity, AllocationConflictCalculation.Severity.High, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        return true;
    }

    public static decimal CalculateEstimatedHours(
        IReadOnlyList<DeliveryStrategyTaskAssignmentCandidate> assignments)
    {
        return assignments.Sum(assignment => assignment.PlannedHours);
    }

    public static decimal CalculateEstimatedCost(
        IReadOnlyList<DeliveryStrategyTaskAssignmentCandidate> assignments)
    {
        return decimal.Round(
            assignments.Sum(assignment => assignment.PlannedHours * assignment.CostRate),
            2,
            MidpointRounding.AwayFromZero);
    }

    public static DeliveryStrategyResourceMixResponse BuildResourceMix(
        IReadOnlyList<DeliveryStrategyTaskAssignmentCandidate> assignments)
    {
        var totalHours = CalculateEstimatedHours(assignments);

        if (totalHours <= 0)
        {
            return new DeliveryStrategyResourceMixResponse();
        }

        var items = assignments
            .GroupBy(assignment => assignment.ResourceType, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var plannedHours = group.Sum(assignment => assignment.PlannedHours);

                return new DeliveryStrategyResourceMixItemResponse
                {
                    ResourceType = group.Key,
                    ResourceCount = group.Select(assignment => assignment.ExecutionResourceId).Distinct().Count(),
                    PlannedHours = plannedHours,
                    PercentageOfHours = decimal.Round(
                        (plannedHours / totalHours) * 100,
                        2,
                        MidpointRounding.AwayFromZero)
                };
            })
            .OrderBy(item => item.ResourceType, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new DeliveryStrategyResourceMixResponse
        {
            Items = items
        };
    }

    public static DeliveryStrategyResponse BuildStrategyResponse(
        Guid contractId,
        Guid missionId,
        IReadOnlySet<string> resourceMix,
        IReadOnlyList<DeliveryStrategyTaskAssignmentCandidate> assignments,
        DeliveryStrategyPlanningMetadataResponse planningMetadata)
    {
        var estimatedHours = CalculateEstimatedHours(assignments);
        var estimatedCost = CalculateEstimatedCost(assignments);

        return new DeliveryStrategyResponse
        {
            StrategyId = CreateDeterministicStrategyId(contractId, missionId, resourceMix),
            ContractId = contractId,
            MissionId = missionId,
            StrategyName = BuildStrategyName(resourceMix),
            AssignedResources = assignments
                .Select(assignment => new DeliveryStrategyTaskAssignmentResponse
                {
                    TaskId = assignment.TaskId,
                    TaskCode = assignment.TaskCode,
                    TaskName = assignment.TaskName,
                    ExecutionResourceId = assignment.ExecutionResourceId,
                    ExecutionResourceCode = assignment.ExecutionResourceCode,
                    ExecutionResourceName = assignment.ExecutionResourceName,
                    ResourceType = assignment.ResourceType,
                    PlannedHours = assignment.PlannedHours
                })
                .ToList(),
            ResourceMix = BuildResourceMix(assignments),
            EstimatedHours = estimatedHours,
            EstimatedCost = estimatedCost,
            PlanningMetadata = planningMetadata
        };
    }

    public static IReadOnlyList<DeliveryStrategyResponse> OrderStrategies(
        IEnumerable<DeliveryStrategyResponse> strategies)
    {
        return strategies
            .OrderBy(strategy => strategy.StrategyName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(strategy => strategy.StrategyId)
            .ToList();
    }
}
