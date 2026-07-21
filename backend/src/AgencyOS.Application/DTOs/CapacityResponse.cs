namespace AgencyOS.Application.DTOs;

public class CapacityResponse
{
    public Guid ExecutionResourceId { get; set; }

    public string ExecutionResourceCode { get; set; } = string.Empty;

    public string ExecutionResourceName { get; set; } = string.Empty;

    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    public decimal TotalCapacityHours { get; set; }

    public decimal AllocatedHours { get; set; }

    public decimal AvailableHours { get; set; }

    public decimal UtilizationPercentage { get; set; }

    public decimal RemainingCapacityHours { get; set; }
}
