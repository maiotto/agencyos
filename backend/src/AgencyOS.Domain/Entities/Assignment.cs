namespace AgencyOS.Domain.Entities;

public class Assignment
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }

    public Guid ExecutionResourceId { get; set; }

    public string AssignmentRole { get; set; } = string.Empty;

    public decimal PlannedHours { get; set; }

    public DateOnly PlannedStartDate { get; set; }

    public DateOnly PlannedEndDate { get; set; }

    public int AllocationPercentage { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public MissionTask? Task { get; set; }

    public ExecutionResource? ExecutionResource { get; set; }
}
