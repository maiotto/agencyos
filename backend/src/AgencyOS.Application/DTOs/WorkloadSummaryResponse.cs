namespace AgencyOS.Application.DTOs;

public class WorkloadSummaryResponse
{
    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    public int ActiveResourceCount { get; set; }

    public decimal TotalPlannedHours { get; set; }

    public int TotalAssignmentCount { get; set; }

    public decimal AverageHoursPerAssignment { get; set; }

    public decimal OverallWorkloadPercentage { get; set; }
}
