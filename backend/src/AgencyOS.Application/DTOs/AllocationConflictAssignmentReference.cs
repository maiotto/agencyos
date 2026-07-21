namespace AgencyOS.Application.DTOs;

public class AllocationConflictAssignmentReference
{
    public Guid AssignmentId { get; set; }

    public Guid TaskId { get; set; }

    public string AssignmentRole { get; set; } = string.Empty;

    public decimal PlannedHours { get; set; }

    public DateOnly PlannedStartDate { get; set; }

    public DateOnly PlannedEndDate { get; set; }
}
