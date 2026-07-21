namespace AgencyOS.Application.DTOs;

public class WorkloadResponse
{
    public Guid ExecutionResourceId { get; set; }

    public string ExecutionResourceCode { get; set; } = string.Empty;

    public string ExecutionResourceName { get; set; } = string.Empty;

    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    public decimal TotalPlannedHours { get; set; }

    public int AssignmentCount { get; set; }

    public decimal AverageHoursPerAssignment { get; set; }

    public decimal WorkloadPercentage { get; set; }

    public IReadOnlyList<WorkloadAssignmentDistributionItem> AssignmentDistribution { get; set; } =
        Array.Empty<WorkloadAssignmentDistributionItem>();
}
