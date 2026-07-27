using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Services;

public static class AllocationConflictCalculation
{
    public static class ConflictType
    {
        public const string CapacityExceeded = "Capacity Exceeded";
        public const string ScheduleOverlap = "Schedule Overlap";
        public const string ResourceUnavailable = "Resource Unavailable";
        public const string CalendarConflict = "Calendar Conflict";
        public const string InvalidAssignment = "Invalid Assignment";
    }

    public static class Severity
    {
        public const string Low = "Low";
        public const string Medium = "Medium";
        public const string High = "High";
        public const string Critical = "Critical";
    }

    public static IReadOnlyList<AllocationConflictResponse> DetectConflicts(
        CapacityResponse capacity,
        WorkloadResponse? workload,
        AvailabilityResponse? availability,
        int periodDays)
    {
        var assignments = workload?.AssignmentDistribution ?? Array.Empty<WorkloadAssignmentDistributionItem>();
        var conflicts = new List<AllocationConflictResponse>();

        conflicts.AddRange(DetectInvalidAssignments(capacity, assignments));
        conflicts.AddRange(DetectScheduleOverlaps(capacity, assignments));
        conflicts.AddRange(DetectCapacityExceeded(capacity, workload, periodDays, assignments));
        conflicts.AddRange(DetectCalendarConflicts(capacity, assignments));
        conflicts.AddRange(DetectResourceUnavailable(capacity, workload, availability));

        return OrderConflicts(conflicts);
    }

    public static IReadOnlyList<AllocationConflictResponse> OrderConflicts(
        IEnumerable<AllocationConflictResponse> conflicts)
    {
        return conflicts
            .OrderBy(conflict => GetSeverityOrder(conflict.Severity))
            .ThenBy(conflict => conflict.ExecutionResourceName)
            .ThenBy(conflict => conflict.ExecutionResourceCode)
            .ThenBy(conflict => conflict.ConflictType)
            .ToList();
    }

    public static int CountBySeverity(IReadOnlyList<AllocationConflictResponse> conflicts, string severity)
    {
        return conflicts.Count(conflict =>
            string.Equals(conflict.Severity, severity, StringComparison.OrdinalIgnoreCase));
    }

    public static int CountResourcesWithConflicts(IReadOnlyList<AllocationConflictResponse> conflicts)
    {
        return conflicts
            .Select(conflict => conflict.ExecutionResourceId)
            .Distinct()
            .Count();
    }

    private static IEnumerable<AllocationConflictResponse> DetectInvalidAssignments(
        CapacityResponse capacity,
        IReadOnlyList<WorkloadAssignmentDistributionItem> assignments)
    {
        foreach (var assignment in assignments)
        {
            if (assignment.PlannedStartDate <= assignment.PlannedEndDate
                && assignment.PlannedHours > 0)
            {
                continue;
            }

            var issues = new List<string>();

            if (assignment.PlannedStartDate > assignment.PlannedEndDate)
            {
                issues.Add("planned end date is before the start date");
            }

            if (assignment.PlannedHours <= 0)
            {
                issues.Add("planned hours must be greater than zero");
            }

            yield return CreateConflict(
                capacity,
                ConflictType.InvalidAssignment,
                Severity.High,
                new[] { assignment },
                $"Assignment has invalid planning data: {string.Join(" and ", issues)}.",
                "Correct assignment dates and planned hours before execution.");
        }
    }

    private static IEnumerable<AllocationConflictResponse> DetectScheduleOverlaps(
        CapacityResponse capacity,
        IReadOnlyList<WorkloadAssignmentDistributionItem> assignments)
    {
        var orderedAssignments = assignments
            .OrderBy(assignment => assignment.PlannedStartDate)
            .ThenBy(assignment => assignment.AssignmentId)
            .ToList();

        for (var firstIndex = 0; firstIndex < orderedAssignments.Count; firstIndex++)
        {
            for (var secondIndex = firstIndex + 1; secondIndex < orderedAssignments.Count; secondIndex++)
            {
                var firstAssignment = orderedAssignments[firstIndex];
                var secondAssignment = orderedAssignments[secondIndex];

                if (!HasDateOverlap(
                        firstAssignment.PlannedStartDate,
                        firstAssignment.PlannedEndDate,
                        secondAssignment.PlannedStartDate,
                        secondAssignment.PlannedEndDate))
                {
                    continue;
                }

                yield return CreateConflict(
                    capacity,
                    ConflictType.ScheduleOverlap,
                    ClassifyScheduleOverlapSeverity(2),
                    new[] { firstAssignment, secondAssignment },
                    "Two assignments overlap in the same planning period.",
                    "Adjust assignment dates to eliminate overlapping schedules.");
            }
        }
    }

    private static IEnumerable<AllocationConflictResponse> DetectCapacityExceeded(
        CapacityResponse capacity,
        WorkloadResponse? workload,
        int _,
        IReadOnlyList<WorkloadAssignmentDistributionItem> assignments)
    {
        var totalPlannedHours = workload?.TotalPlannedHours ?? 0m;

        if (totalPlannedHours > capacity.TotalCapacityHours && capacity.TotalCapacityHours > 0)
        {
            var utilizationPercentage = CapacityCalculation.CalculateUtilizationPercentage(
                capacity.TotalCapacityHours,
                totalPlannedHours);

            yield return CreateConflict(
                capacity,
                ConflictType.CapacityExceeded,
                ClassifyCapacityExceededSeverity(utilizationPercentage),
                assignments,
                $"Planned workload of {totalPlannedHours} hours exceeds available capacity of {capacity.TotalCapacityHours} hours.",
                "Reduce planned hours, extend the assignment period, or reassign work to another resource.");
        }

        if (assignments.Count == 0 || capacity.TotalCapacityHours <= 0)
        {
            yield break;
        }

        var dailyCapacityHours = AvailabilityCalculation.GetDailyCapacityHoursFromCapacity(
            capacity.OperationalDays);
        var dailyOccupiedHours = AvailabilityCalculation.BuildDailyOccupiedHours(
            capacity.PeriodStartDate,
            capacity.PeriodEndDate,
            assignments,
            dailyCapacityHours.Keys.OrderBy(date => date).ToList());

        foreach (var dayEntry in dailyOccupiedHours.OrderBy(entry => entry.Key))
        {
            var dayCapacity = dailyCapacityHours.GetValueOrDefault(dayEntry.Key);
            if (dayEntry.Value <= dayCapacity)
            {
                continue;
            }

            var dayAssignments = assignments
                .Where(assignment => IsAssignmentActiveOnDay(assignment, dayEntry.Key))
                .ToList();

            yield return CreateConflict(
                capacity,
                ConflictType.CapacityExceeded,
                ClassifyDailyCapacityExceededSeverity(dayEntry.Value, dayCapacity),
                dayAssignments,
                $"Daily workload of {decimal.Round(dayEntry.Value, 2, MidpointRounding.AwayFromZero)} hours on {dayEntry.Key:yyyy-MM-dd} exceeds daily capacity of {decimal.Round(dayCapacity, 2, MidpointRounding.AwayFromZero)} hours.",
                "Redistribute daily workload or extend assignment dates to stay within daily capacity.");
        }
    }

    private static IEnumerable<AllocationConflictResponse> DetectCalendarConflicts(
        CapacityResponse capacity,
        IReadOnlyList<WorkloadAssignmentDistributionItem> assignments)
    {
        var operationalDaySet = capacity.OperationalDays
            .Where(day => day.IsOperationalDay)
            .Select(day => day.Date)
            .ToHashSet();

        foreach (var assignment in assignments)
        {
            var rangeStart = assignment.PlannedStartDate > capacity.PeriodStartDate
                ? assignment.PlannedStartDate
                : capacity.PeriodStartDate;
            var rangeEnd = assignment.PlannedEndDate < capacity.PeriodEndDate
                ? assignment.PlannedEndDate
                : capacity.PeriodEndDate;

            if (rangeStart > rangeEnd)
            {
                continue;
            }

            var nonWorkingDays = new List<DateOnly>();

            for (var date = rangeStart; date <= rangeEnd; date = date.AddDays(1))
            {
                if (!operationalDaySet.Contains(date))
                {
                    nonWorkingDays.Add(date);
                }
            }

            if (nonWorkingDays.Count == 0)
            {
                continue;
            }

            var totalDays = rangeEnd.DayNumber - rangeStart.DayNumber + 1;

            yield return CreateConflict(
                capacity,
                ConflictType.CalendarConflict,
                ClassifyCalendarConflictSeverity(nonWorkingDays.Count, totalDays),
                new[] { assignment },
                nonWorkingDays.Count == totalDays
                    ? "Assignment is scheduled entirely outside the working calendar."
                    : $"Assignment includes {nonWorkingDays.Count} non-working day(s) in the planning period.",
                "Move assignments to working days within the calendar.");
        }
    }

    private static IEnumerable<AllocationConflictResponse> DetectResourceUnavailable(
        CapacityResponse capacity,
        WorkloadResponse? workload,
        AvailabilityResponse? availability)
    {
        var hasAssignments = workload?.AssignmentCount > 0;

        if (!hasAssignments)
        {
            yield break;
        }

        if (capacity.TotalCapacityHours > 0)
        {
            yield break;
        }

        yield return CreateConflict(
            capacity,
            ConflictType.ResourceUnavailable,
            Severity.Critical,
            workload?.AssignmentDistribution ?? Array.Empty<WorkloadAssignmentDistributionItem>(),
            "Assignments exist for a resource with no operational capacity in the planning period.",
            "Increase resource capacity or reassign work to an available execution resource.");
    }

    private static AllocationConflictResponse CreateConflict(
        CapacityResponse capacity,
        string conflictType,
        string severity,
        IEnumerable<WorkloadAssignmentDistributionItem> assignments,
        string description,
        string suggestedResolution)
    {
        var relatedAssignments = assignments
            .Select(MapAssignmentReference)
            .OrderBy(assignment => assignment.PlannedStartDate)
            .ThenBy(assignment => assignment.AssignmentId)
            .ToList();

        return new AllocationConflictResponse
        {
            ConflictId = Guid.NewGuid(),
            ConflictType = conflictType,
            ExecutionResourceId = capacity.ExecutionResourceId,
            ExecutionResourceCode = capacity.ExecutionResourceCode,
            ExecutionResourceName = capacity.ExecutionResourceName,
            RelatedAssignments = relatedAssignments,
            Severity = severity,
            Description = description,
            SuggestedResolution = suggestedResolution
        };
    }

    private static AllocationConflictAssignmentReference MapAssignmentReference(
        WorkloadAssignmentDistributionItem assignment)
    {
        return new AllocationConflictAssignmentReference
        {
            AssignmentId = assignment.AssignmentId,
            TaskId = assignment.TaskId,
            AssignmentRole = assignment.AssignmentRole,
            PlannedHours = assignment.PlannedHours,
            PlannedStartDate = assignment.PlannedStartDate,
            PlannedEndDate = assignment.PlannedEndDate
        };
    }

    private static bool HasDateOverlap(
        DateOnly firstStartDate,
        DateOnly firstEndDate,
        DateOnly secondStartDate,
        DateOnly secondEndDate)
    {
        return firstStartDate <= secondEndDate && secondStartDate <= firstEndDate;
    }

    private static bool IsAssignmentActiveOnDay(
        WorkloadAssignmentDistributionItem assignment,
        DateOnly day)
    {
        return assignment.PlannedStartDate <= day && assignment.PlannedEndDate >= day;
    }

    private static string ClassifyCapacityExceededSeverity(decimal utilizationPercentage)
    {
        if (utilizationPercentage >= 150)
        {
            return Severity.Critical;
        }

        if (utilizationPercentage >= 120)
        {
            return Severity.High;
        }

        return Severity.Medium;
    }

    private static string ClassifyDailyCapacityExceededSeverity(
        decimal occupiedHours,
        decimal dailyCapacityHours)
    {
        if (dailyCapacityHours <= 0)
        {
            return Severity.Critical;
        }

        var utilizationPercentage = (occupiedHours / dailyCapacityHours) * 100;

        if (utilizationPercentage >= 150)
        {
            return Severity.Critical;
        }

        if (utilizationPercentage >= 120)
        {
            return Severity.High;
        }

        return Severity.Medium;
    }

    private static string ClassifyScheduleOverlapSeverity(int overlappingAssignmentCount)
    {
        return overlappingAssignmentCount >= 3 ? Severity.High : Severity.Medium;
    }

    private static string ClassifyCalendarConflictSeverity(int nonWorkingDaysCount, int totalDaysInAssignment)
    {
        if (totalDaysInAssignment > 0 && nonWorkingDaysCount == totalDaysInAssignment)
        {
            return Severity.High;
        }

        return nonWorkingDaysCount > 1 ? Severity.Medium : Severity.Low;
    }

    private static int GetSeverityOrder(string severity)
    {
        return severity switch
        {
            Severity.Critical => 0,
            Severity.High => 1,
            Severity.Medium => 2,
            Severity.Low => 3,
            _ => 4
        };
    }
}
