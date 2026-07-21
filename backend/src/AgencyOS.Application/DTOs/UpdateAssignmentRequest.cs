namespace AgencyOS.Application.DTOs;

public class UpdateAssignmentRequest
{
    public string AssignmentRole { get; set; } = string.Empty;

    public decimal PlannedHours { get; set; }

    public DateOnly PlannedStartDate { get; set; }

    public DateOnly PlannedEndDate { get; set; }

    public int AllocationPercentage { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
