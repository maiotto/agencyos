using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

public static class DeliveryStrategyEvaluationCalculation
{
    public static DeliveryStrategyEvaluationMetricsResponse EvaluateStrategy(
        DeliveryStrategyResponse strategy,
        IReadOnlyDictionary<Guid, CapacityResponse> capacitiesByResourceId,
        IReadOnlyDictionary<Guid, WorkloadResponse> workloadsByResourceId,
        IReadOnlyDictionary<Guid, AvailabilityResponse> availabilitiesByResourceId,
        IReadOnlyDictionary<Guid, IReadOnlyList<AllocationConflictResponse>> conflictsByResourceId)
    {
        var assignments = strategy.AssignedResources;
        var totalStrategyHours = strategy.EstimatedHours;

        var additionalHoursByResource = assignments
            .GroupBy(assignment => assignment.ExecutionResourceId)
            .ToDictionary(group => group.Key, group => group.Sum(assignment => assignment.PlannedHours));

        var capacityUtilization = CalculateWeightedAverage(
            additionalHoursByResource,
            resourceId =>
            {
                if (!capacitiesByResourceId.TryGetValue(resourceId, out var capacity)
                    || capacity.TotalCapacityHours <= 0)
                {
                    return 0m;
                }

                workloadsByResourceId.TryGetValue(resourceId, out var workload);
                var projectedHours = (workload?.TotalPlannedHours ?? 0m) + additionalHoursByResource[resourceId];

                return CapacityCalculation.CalculateUtilizationPercentage(
                    capacity.TotalCapacityHours,
                    projectedHours);
            });

        var resourceUtilization = CalculateSimpleAverage(
            additionalHoursByResource.Keys,
            resourceId =>
            {
                if (!capacitiesByResourceId.TryGetValue(resourceId, out var capacity)
                    || capacity.TotalCapacityHours <= 0)
                {
                    return 0m;
                }

                workloadsByResourceId.TryGetValue(resourceId, out var workload);
                var projectedHours = (workload?.TotalPlannedHours ?? 0m) + additionalHoursByResource[resourceId];

                return CapacityCalculation.CalculateUtilizationPercentage(
                    capacity.TotalCapacityHours,
                    projectedHours);
            });

        var workloadImpact = CalculateWorkloadImpact(
            additionalHoursByResource,
            capacitiesByResourceId,
            workloadsByResourceId);

        var availabilityImpact = CalculateAvailabilityImpact(
            additionalHoursByResource,
            availabilitiesByResourceId);

        var operationalRisk = CalculateOperationalRiskScore(
            additionalHoursByResource,
            capacitiesByResourceId,
            workloadsByResourceId,
            availabilitiesByResourceId,
            conflictsByResourceId);

        var humanUtilization = CalculateResourceTypeUtilization(
            assignments,
            totalStrategyHours,
            ExecutionResourceType.InternalHuman,
            ExecutionResourceType.ExternalHuman);

        var aiUtilization = CalculateResourceTypeUtilization(
            assignments,
            totalStrategyHours,
            ExecutionResourceType.AiAgent,
            ExecutionResourceType.AiService);

        var automationUtilization = CalculateResourceTypeUtilization(
            assignments,
            totalStrategyHours,
            ExecutionResourceType.Automation);

        var externalUtilization = CalculateResourceTypeUtilization(
            assignments,
            totalStrategyHours,
            ExecutionResourceType.ExternalHuman);

        return new DeliveryStrategyEvaluationMetricsResponse
        {
            EstimatedCost = strategy.EstimatedCost,
            EstimatedDurationHours = strategy.EstimatedHours,
            CapacityUtilizationPercentage = capacityUtilization,
            ResourceUtilizationPercentage = resourceUtilization,
            WorkloadImpactPercentage = workloadImpact,
            AvailabilityImpactPercentage = availabilityImpact,
            OperationalRiskScore = operationalRisk,
            HumanUtilizationPercentage = humanUtilization,
            AiUtilizationPercentage = aiUtilization,
            AutomationUtilizationPercentage = automationUtilization,
            ExternalResourceUtilizationPercentage = externalUtilization
        };
    }

    private static decimal CalculateWorkloadImpact(
        IReadOnlyDictionary<Guid, decimal> additionalHoursByResource,
        IReadOnlyDictionary<Guid, CapacityResponse> capacitiesByResourceId,
        IReadOnlyDictionary<Guid, WorkloadResponse> workloadsByResourceId)
    {
        decimal totalAdditionalHours = 0m;
        decimal totalCapacityHours = 0m;

        foreach (var (resourceId, additionalHours) in additionalHoursByResource)
        {
            if (!capacitiesByResourceId.TryGetValue(resourceId, out var capacity)
                || capacity.TotalCapacityHours <= 0)
            {
                continue;
            }

            totalAdditionalHours += additionalHours;
            totalCapacityHours += capacity.TotalCapacityHours;
        }

        if (totalCapacityHours <= 0)
        {
            return 0m;
        }

        return decimal.Round(
            (totalAdditionalHours / totalCapacityHours) * 100,
            2,
            MidpointRounding.AwayFromZero);
    }

    private static decimal CalculateAvailabilityImpact(
        IReadOnlyDictionary<Guid, decimal> additionalHoursByResource,
        IReadOnlyDictionary<Guid, AvailabilityResponse> availabilitiesByResourceId)
    {
        return CalculateWeightedAverage(
            additionalHoursByResource,
            resourceId =>
            {
                if (!availabilitiesByResourceId.TryGetValue(resourceId, out var availability)
                    || availability.AvailableHours <= 0)
                {
                    return 100m;
                }

                return decimal.Round(
                    (additionalHoursByResource[resourceId] / availability.AvailableHours) * 100,
                    2,
                    MidpointRounding.AwayFromZero);
            });
    }

    private static decimal CalculateOperationalRiskScore(
        IReadOnlyDictionary<Guid, decimal> additionalHoursByResource,
        IReadOnlyDictionary<Guid, CapacityResponse> capacitiesByResourceId,
        IReadOnlyDictionary<Guid, WorkloadResponse> workloadsByResourceId,
        IReadOnlyDictionary<Guid, AvailabilityResponse> availabilitiesByResourceId,
        IReadOnlyDictionary<Guid, IReadOnlyList<AllocationConflictResponse>> conflictsByResourceId)
    {
        if (additionalHoursByResource.Count == 0)
        {
            return 0m;
        }

        var riskScores = additionalHoursByResource.Keys
            .Select(resourceId => CalculateResourceRiskScore(
                resourceId,
                additionalHoursByResource[resourceId],
                capacitiesByResourceId,
                workloadsByResourceId,
                availabilitiesByResourceId,
                conflictsByResourceId))
            .ToList();

        return decimal.Round(
            riskScores.Average(),
            2,
            MidpointRounding.AwayFromZero);
    }

    private static decimal CalculateResourceRiskScore(
        Guid resourceId,
        decimal additionalHours,
        IReadOnlyDictionary<Guid, CapacityResponse> capacitiesByResourceId,
        IReadOnlyDictionary<Guid, WorkloadResponse> workloadsByResourceId,
        IReadOnlyDictionary<Guid, AvailabilityResponse> availabilitiesByResourceId,
        IReadOnlyDictionary<Guid, IReadOnlyList<AllocationConflictResponse>> conflictsByResourceId)
    {
        var riskScore = 0m;

        if (conflictsByResourceId.TryGetValue(resourceId, out var conflicts))
        {
            riskScore += conflicts.Sum(GetConflictRiskContribution);
        }

        if (capacitiesByResourceId.TryGetValue(resourceId, out var capacity)
            && capacity.TotalCapacityHours > 0)
        {
            workloadsByResourceId.TryGetValue(resourceId, out var workload);
            var projectedHours = (workload?.TotalPlannedHours ?? 0m) + additionalHours;
            var utilization = projectedHours / capacity.TotalCapacityHours * 100;

            riskScore += utilization switch
            {
                >= 100 => 30m,
                >= 90 => 20m,
                >= 80 => 10m,
                _ => 0m
            };
        }

        if (availabilitiesByResourceId.TryGetValue(resourceId, out var availability))
        {
            var remainingHours = availability.AvailableHours - additionalHours;

            if (remainingHours <= 0)
            {
                riskScore += 25m;
            }
            else if (availability.AvailableHours > 0
                     && remainingHours / availability.AvailableHours <= 0.1m)
            {
                riskScore += 15m;
            }
        }

        return Math.Min(100m, riskScore);
    }

    private static decimal GetConflictRiskContribution(AllocationConflictResponse conflict)
    {
        return conflict.Severity switch
        {
            AllocationConflictCalculation.Severity.Critical => 40m,
            AllocationConflictCalculation.Severity.High => 25m,
            AllocationConflictCalculation.Severity.Medium => 15m,
            AllocationConflictCalculation.Severity.Low => 5m,
            _ => 0m
        };
    }

    private static decimal CalculateResourceTypeUtilization(
        IReadOnlyList<DeliveryStrategyTaskAssignmentResponse> assignments,
        decimal totalStrategyHours,
        params string[] resourceTypes)
    {
        if (totalStrategyHours <= 0 || resourceTypes.Length == 0)
        {
            return 0m;
        }

        var typeSet = new HashSet<string>(resourceTypes, StringComparer.OrdinalIgnoreCase);
        var matchingHours = assignments
            .Where(assignment => typeSet.Contains(assignment.ResourceType))
            .Sum(assignment => assignment.PlannedHours);

        return decimal.Round(
            (matchingHours / totalStrategyHours) * 100,
            2,
            MidpointRounding.AwayFromZero);
    }

    private static decimal CalculateWeightedAverage(
        IReadOnlyDictionary<Guid, decimal> weightsByResourceId,
        Func<Guid, decimal> valueSelector)
    {
        if (weightsByResourceId.Count == 0)
        {
            return 0m;
        }

        decimal weightedSum = 0m;
        decimal totalWeight = 0m;

        foreach (var (resourceId, weight) in weightsByResourceId)
        {
            if (weight <= 0)
            {
                continue;
            }

            weightedSum += valueSelector(resourceId) * weight;
            totalWeight += weight;
        }

        if (totalWeight <= 0)
        {
            return 0m;
        }

        return decimal.Round(
            weightedSum / totalWeight,
            2,
            MidpointRounding.AwayFromZero);
    }

    private static decimal CalculateSimpleAverage(
        IEnumerable<Guid> resourceIds,
        Func<Guid, decimal> valueSelector)
    {
        var values = resourceIds.Select(valueSelector).ToList();

        if (values.Count == 0)
        {
            return 0m;
        }

        return decimal.Round(
            values.Average(),
            2,
            MidpointRounding.AwayFromZero);
    }
}
