namespace AgencyOS.Application.DTOs;

public class CapacitySummaryResponse
{
    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    public int ActiveResourceCount { get; set; }

    public decimal TotalCapacityHours { get; set; }

    public decimal TotalAllocatedHours { get; set; }

    public decimal TotalAvailableHours { get; set; }

    public decimal OverallUtilizationPercentage { get; set; }

    public decimal TotalRemainingCapacityHours { get; set; }
}
