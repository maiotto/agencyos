namespace AgencyOS.Application.DTOs;

public class DeliveryStrategyTaskAssignmentResponse
{
    public Guid TaskId { get; set; }

    public string TaskCode { get; set; } = string.Empty;

    public string TaskName { get; set; } = string.Empty;

    public Guid ExecutionResourceId { get; set; }

    public string ExecutionResourceCode { get; set; } = string.Empty;

    public string ExecutionResourceName { get; set; } = string.Empty;

    public string ResourceType { get; set; } = string.Empty;

    public decimal PlannedHours { get; set; }
}
