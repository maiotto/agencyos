using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class DeliveryStrategyEvaluationCalculationTests
{
    [Fact]
    public void EvaluateStrategy_CalculatesAllMetricsForInternalHumanStrategy()
    {
        var resourceId = Guid.NewGuid();
        var strategy = CreateStrategy(resourceId, ExecutionResourceType.InternalHuman, 16m, 1360m);

        var metrics = DeliveryStrategyEvaluationCalculation.EvaluateStrategy(
            strategy,
            new Dictionary<Guid, CapacityResponse>
            {
                [resourceId] = new CapacityResponse
                {
                    ExecutionResourceId = resourceId,
                    TotalCapacityHours = 160m,
                    AvailableHours = 160m
                }
            },
            new Dictionary<Guid, WorkloadResponse>
            {
                [resourceId] = new WorkloadResponse
                {
                    ExecutionResourceId = resourceId,
                    TotalPlannedHours = 0m
                }
            },
            new Dictionary<Guid, AvailabilityResponse>
            {
                [resourceId] = new AvailabilityResponse
                {
                    ExecutionResourceId = resourceId,
                    AvailableHours = 160m
                }
            },
            new Dictionary<Guid, IReadOnlyList<AllocationConflictResponse>>());

        Assert.Equal(1360m, metrics.EstimatedCost);
        Assert.Equal(16m, metrics.EstimatedDurationHours);
        Assert.Equal(10m, metrics.CapacityUtilizationPercentage);
        Assert.Equal(10m, metrics.ResourceUtilizationPercentage);
        Assert.Equal(10m, metrics.WorkloadImpactPercentage);
        Assert.Equal(10m, metrics.AvailabilityImpactPercentage);
        Assert.Equal(0m, metrics.OperationalRiskScore);
        Assert.Equal(100m, metrics.HumanUtilizationPercentage);
        Assert.Equal(0m, metrics.AiUtilizationPercentage);
        Assert.Equal(0m, metrics.AutomationUtilizationPercentage);
        Assert.Equal(0m, metrics.ExternalResourceUtilizationPercentage);
    }

    [Fact]
    public void EvaluateStrategy_IncreasesOperationalRiskWhenConflictsExist()
    {
        var resourceId = Guid.NewGuid();
        var strategy = CreateStrategy(resourceId, ExecutionResourceType.InternalHuman, 16m, 1360m);

        var metrics = DeliveryStrategyEvaluationCalculation.EvaluateStrategy(
            strategy,
            new Dictionary<Guid, CapacityResponse>
            {
                [resourceId] = new CapacityResponse
                {
                    ExecutionResourceId = resourceId,
                    TotalCapacityHours = 160m,
                    AvailableHours = 160m
                }
            },
            new Dictionary<Guid, WorkloadResponse>
            {
                [resourceId] = new WorkloadResponse
                {
                    ExecutionResourceId = resourceId,
                    TotalPlannedHours = 140m
                }
            },
            new Dictionary<Guid, AvailabilityResponse>
            {
                [resourceId] = new AvailabilityResponse
                {
                    ExecutionResourceId = resourceId,
                    AvailableHours = 20m
                }
            },
            new Dictionary<Guid, IReadOnlyList<AllocationConflictResponse>>
            {
                [resourceId] =
                [
                    new AllocationConflictResponse
                    {
                        ExecutionResourceId = resourceId,
                        Severity = AllocationConflictCalculation.Severity.High
                    }
                ]
            });

        Assert.True(metrics.OperationalRiskScore > 0m);
        Assert.True(metrics.CapacityUtilizationPercentage > 90m);
        Assert.True(metrics.AvailabilityImpactPercentage > 50m);
    }

    [Fact]
    public void EvaluateStrategy_CalculatesMixedResourceTypeUtilization()
    {
        var humanResourceId = Guid.NewGuid();
        var aiResourceId = Guid.NewGuid();
        var strategy = new DeliveryStrategyResponse
        {
            StrategyId = Guid.NewGuid(),
            EstimatedHours = 20m,
            EstimatedCost = 500m,
            AssignedResources =
            [
                new DeliveryStrategyTaskAssignmentResponse
                {
                    ExecutionResourceId = humanResourceId,
                    ResourceType = ExecutionResourceType.InternalHuman,
                    PlannedHours = 12m
                },
                new DeliveryStrategyTaskAssignmentResponse
                {
                    ExecutionResourceId = aiResourceId,
                    ResourceType = ExecutionResourceType.AiAgent,
                    PlannedHours = 8m
                }
            ]
        };

        var metrics = DeliveryStrategyEvaluationCalculation.EvaluateStrategy(
            strategy,
            new Dictionary<Guid, CapacityResponse>
            {
                [humanResourceId] = new CapacityResponse
                {
                    ExecutionResourceId = humanResourceId,
                    TotalCapacityHours = 160m
                },
                [aiResourceId] = new CapacityResponse
                {
                    ExecutionResourceId = aiResourceId,
                    TotalCapacityHours = 160m
                }
            },
            new Dictionary<Guid, WorkloadResponse>(),
            new Dictionary<Guid, AvailabilityResponse>
            {
                [humanResourceId] = new AvailabilityResponse
                {
                    ExecutionResourceId = humanResourceId,
                    AvailableHours = 160m
                },
                [aiResourceId] = new AvailabilityResponse
                {
                    ExecutionResourceId = aiResourceId,
                    AvailableHours = 160m
                }
            },
            new Dictionary<Guid, IReadOnlyList<AllocationConflictResponse>>());

        Assert.Equal(60m, metrics.HumanUtilizationPercentage);
        Assert.Equal(40m, metrics.AiUtilizationPercentage);
    }

    private static DeliveryStrategyResponse CreateStrategy(
        Guid resourceId,
        string resourceType,
        decimal plannedHours,
        decimal estimatedCost)
    {
        return new DeliveryStrategyResponse
        {
            StrategyId = Guid.NewGuid(),
            EstimatedHours = plannedHours,
            EstimatedCost = estimatedCost,
            AssignedResources =
            [
                new DeliveryStrategyTaskAssignmentResponse
                {
                    ExecutionResourceId = resourceId,
                    ResourceType = resourceType,
                    PlannedHours = plannedHours
                }
            ]
        };
    }
}
