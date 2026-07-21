using AgencyOS.Application.DTOs;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class DeliveryStrategyGenerationTests
{
    [Fact]
    public void GenerateResourceMixSubsets_ReturnsAllNonEmptyCombinations()
    {
        var availableTypes = new List<string>
        {
            ExecutionResourceType.InternalHuman,
            ExecutionResourceType.AiAgent
        };

        var subsets = DeliveryStrategyGeneration.GenerateResourceMixSubsets(availableTypes).ToList();

        Assert.Equal(3, subsets.Count);
        Assert.Contains(subsets, subset => subset.Count == 1 && subset.Contains(ExecutionResourceType.InternalHuman));
        Assert.Contains(subsets, subset => subset.Count == 1 && subset.Contains(ExecutionResourceType.AiAgent));
        Assert.Contains(
            subsets,
            subset => subset.Count == 2
                && subset.Contains(ExecutionResourceType.InternalHuman)
                && subset.Contains(ExecutionResourceType.AiAgent));
    }

    [Fact]
    public void BuildStrategyName_UsesDynamicResourceTypeLabels()
    {
        var strategyName = DeliveryStrategyGeneration.BuildStrategyName(
        [
            ExecutionResourceType.AiAgent,
            ExecutionResourceType.InternalHuman
        ]);

        Assert.Equal("AI Agent + Internal Human", strategyName);
    }

    [Fact]
    public void IsValidResourceMix_RejectsAiWithoutHumanReview()
    {
        var policies = new DeliveryStrategyPolicyContext
        {
            AllowedResourceTypes = ExecutionResourceType.All,
            RequireHumanReviewForAiAutomation = true
        };

        var isValid = DeliveryStrategyPolicyResolution.IsValidResourceMix(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ExecutionResourceType.AiAgent
            },
            policies);

        Assert.False(isValid);
    }

    [Fact]
    public void IsValidResourceMix_AllowsAiWithHumanReview()
    {
        var policies = new DeliveryStrategyPolicyContext
        {
            AllowedResourceTypes = ExecutionResourceType.All,
            RequireHumanReviewForAiAutomation = true
        };

        var isValid = DeliveryStrategyPolicyResolution.IsValidResourceMix(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ExecutionResourceType.AiAgent,
                ExecutionResourceType.InternalHuman
            },
            policies);

        Assert.True(isValid);
    }

    [Fact]
    public void IsOperationallyValid_RejectsWhenCapacityIsInsufficient()
    {
        var resourceId = Guid.NewGuid();
        var assignments = new List<DeliveryStrategyTaskAssignmentCandidate>
        {
            new()
            {
                ExecutionResourceId = resourceId,
                PlannedHours = 20m
            }
        };

        var isValid = DeliveryStrategyGeneration.IsOperationallyValid(
            assignments,
            new Dictionary<Guid, CapacityResponse>
            {
                [resourceId] = new CapacityResponse
                {
                    ExecutionResourceId = resourceId,
                    TotalCapacityHours = 40m,
                    AvailableHours = 10m
                }
            },
            new Dictionary<Guid, WorkloadResponse>
            {
                [resourceId] = new WorkloadResponse
                {
                    ExecutionResourceId = resourceId,
                    TotalPlannedHours = 30m
                }
            },
            new Dictionary<Guid, AvailabilityResponse>
            {
                [resourceId] = new AvailabilityResponse
                {
                    ExecutionResourceId = resourceId,
                    AvailableHours = 10m
                }
            },
            new Dictionary<Guid, IReadOnlyList<AllocationConflictResponse>>());

        Assert.False(isValid);
    }

    [Fact]
    public void BuildStrategyResponse_CalculatesEstimatedHoursAndCost()
    {
        var contractId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var assignments = new List<DeliveryStrategyTaskAssignmentCandidate>
        {
            new()
            {
                TaskId = Guid.NewGuid(),
                TaskCode = "TSK-001",
                TaskName = "Design",
                ExecutionResourceId = Guid.NewGuid(),
                ExecutionResourceCode = "RES-001",
                ExecutionResourceName = "Designer",
                ResourceType = ExecutionResourceType.InternalHuman,
                PlannedHours = 10m,
                CostRate = 50m
            },
            new()
            {
                TaskId = Guid.NewGuid(),
                TaskCode = "TSK-002",
                TaskName = "Review",
                ExecutionResourceId = Guid.NewGuid(),
                ExecutionResourceCode = "RES-002",
                ExecutionResourceName = "Reviewer",
                ResourceType = ExecutionResourceType.InternalHuman,
                PlannedHours = 5m,
                CostRate = 40m
            }
        };
        var resourceMix = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ExecutionResourceType.InternalHuman
        };

        var response = DeliveryStrategyGeneration.BuildStrategyResponse(
            contractId,
            missionId,
            resourceMix,
            assignments,
            new DeliveryStrategyPlanningMetadataResponse());

        Assert.Equal(15m, response.EstimatedHours);
        Assert.Equal(700m, response.EstimatedCost);
        Assert.Equal("Internal Human", response.StrategyName);
        Assert.Single(response.ResourceMix.Items);
    }
}
