using AgencyOS.Application.DTOs;
using AgencyOS.Application.Services;

namespace AgencyOS.Application.Tests;

public class AllocationConflictCalculationTests
{
    [Fact]
    public void DetectConflicts_DetectsInvalidAssignment()
    {
        var capacity = CreateCapacityResponse(Guid.NewGuid(), "RES-001");
        var workload = CreateWorkloadResponse(capacity.ExecutionResourceId, "RES-001", new[]
        {
            CreateAssignment(
                new DateOnly(2026, 7, 10),
                new DateOnly(2026, 7, 1),
                8m)
        });

        var conflicts = AllocationConflictCalculation.DetectConflicts(capacity, workload, null, 7);

        Assert.Contains(
            conflicts,
            conflict =>
                conflict.ConflictType == AllocationConflictCalculation.ConflictType.InvalidAssignment
                && conflict.Severity == AllocationConflictCalculation.Severity.High);
    }

    [Fact]
    public void DetectConflicts_DetectsScheduleOverlap()
    {
        var capacity = CreateCapacityResponse(Guid.NewGuid(), "RES-001");
        var workload = CreateWorkloadResponse(capacity.ExecutionResourceId, "RES-001", new[]
        {
            CreateAssignment(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5), 20m),
            CreateAssignment(new DateOnly(2026, 7, 3), new DateOnly(2026, 7, 7), 20m)
        });

        var conflicts = AllocationConflictCalculation.DetectConflicts(capacity, workload, null, 7);

        Assert.Contains(
            conflicts,
            conflict =>
                conflict.ConflictType == AllocationConflictCalculation.ConflictType.ScheduleOverlap
                && conflict.RelatedAssignments.Count == 2);
    }

    [Fact]
    public void DetectConflicts_DetectsCapacityExceeded()
    {
        var capacity = CreateCapacityResponse(Guid.NewGuid(), "RES-001", totalCapacityHours: 20m);
        var workload = CreateWorkloadResponse(capacity.ExecutionResourceId, "RES-001", new[]
        {
            CreateAssignment(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 7), 30m)
        });

        var conflicts = AllocationConflictCalculation.DetectConflicts(capacity, workload, null, 7);

        Assert.Contains(
            conflicts,
            conflict =>
                conflict.ConflictType == AllocationConflictCalculation.ConflictType.CapacityExceeded
                && conflict.Severity == AllocationConflictCalculation.Severity.Critical);
    }

    [Fact]
    public void DetectConflicts_DetectsCalendarConflict()
    {
        var capacity = CreateCapacityResponse(Guid.NewGuid(), "RES-001");
        var workload = CreateWorkloadResponse(capacity.ExecutionResourceId, "RES-001", new[]
        {
            CreateAssignment(new DateOnly(2026, 7, 4), new DateOnly(2026, 7, 5), 8m)
        });

        var conflicts = AllocationConflictCalculation.DetectConflicts(capacity, workload, null, 7);

        Assert.Contains(
            conflicts,
            conflict =>
                conflict.ConflictType == AllocationConflictCalculation.ConflictType.CalendarConflict
                && conflict.Severity == AllocationConflictCalculation.Severity.High);
    }

    [Fact]
    public void DetectConflicts_DetectsResourceUnavailable()
    {
        var capacity = CreateCapacityResponse(Guid.NewGuid(), "RES-001", totalCapacityHours: 0m);
        var workload = CreateWorkloadResponse(capacity.ExecutionResourceId, "RES-001", new[]
        {
            CreateAssignment(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 3), 8m)
        });

        var conflicts = AllocationConflictCalculation.DetectConflicts(capacity, workload, null, 7);

        Assert.Contains(
            conflicts,
            conflict =>
                conflict.ConflictType == AllocationConflictCalculation.ConflictType.ResourceUnavailable
                && conflict.Severity == AllocationConflictCalculation.Severity.Critical);
    }

    [Fact]
    public void OrderConflicts_OrdersBySeverityThenResourceName()
    {
        var resourceId = Guid.NewGuid();
        var conflicts = new List<AllocationConflictResponse>
        {
            CreateConflict(resourceId, "RES-002", "Beta", AllocationConflictCalculation.Severity.Low),
            CreateConflict(resourceId, "RES-001", "Alpha", AllocationConflictCalculation.Severity.Critical),
            CreateConflict(resourceId, "RES-001", "Alpha", AllocationConflictCalculation.Severity.Medium)
        };

        var orderedConflicts = AllocationConflictCalculation.OrderConflicts(conflicts);

        Assert.Equal(AllocationConflictCalculation.Severity.Critical, orderedConflicts[0].Severity);
        Assert.Equal(AllocationConflictCalculation.Severity.Medium, orderedConflicts[1].Severity);
        Assert.Equal(AllocationConflictCalculation.Severity.Low, orderedConflicts[2].Severity);
    }

    private static CapacityResponse CreateCapacityResponse(
        Guid resourceId,
        string code,
        decimal totalCapacityHours = 40m)
    {
        return new CapacityResponse
        {
            ExecutionResourceId = resourceId,
            ExecutionResourceCode = code,
            ExecutionResourceName = $"Resource {code}",
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7),
            TotalCapacityHours = totalCapacityHours,
            AllocatedHours = 0m,
            AvailableHours = totalCapacityHours,
            UtilizationPercentage = 0m,
            RemainingCapacityHours = totalCapacityHours
        };
    }

    private static WorkloadResponse CreateWorkloadResponse(
        Guid resourceId,
        string code,
        IReadOnlyList<WorkloadAssignmentDistributionItem> assignments)
    {
        var totalPlannedHours = assignments.Sum(assignment => assignment.PlannedHours);

        return new WorkloadResponse
        {
            ExecutionResourceId = resourceId,
            ExecutionResourceCode = code,
            ExecutionResourceName = $"Resource {code}",
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7),
            TotalPlannedHours = totalPlannedHours,
            AssignmentCount = assignments.Count,
            AverageHoursPerAssignment = totalPlannedHours,
            WorkloadPercentage = 100m,
            AssignmentDistribution = assignments
        };
    }

    private static WorkloadAssignmentDistributionItem CreateAssignment(
        DateOnly startDate,
        DateOnly endDate,
        decimal plannedHours)
    {
        return new WorkloadAssignmentDistributionItem
        {
            AssignmentId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            AssignmentRole = "Responsible",
            PlannedHours = plannedHours,
            PlannedStartDate = startDate,
            PlannedEndDate = endDate,
            Status = "Planned"
        };
    }

    private static AllocationConflictResponse CreateConflict(
        Guid resourceId,
        string code,
        string name,
        string severity)
    {
        return new AllocationConflictResponse
        {
            ConflictId = Guid.NewGuid(),
            ConflictType = AllocationConflictCalculation.ConflictType.ScheduleOverlap,
            ExecutionResourceId = resourceId,
            ExecutionResourceCode = code,
            ExecutionResourceName = name,
            Severity = severity,
            Description = "Test conflict",
            SuggestedResolution = "Test resolution"
        };
    }
}
