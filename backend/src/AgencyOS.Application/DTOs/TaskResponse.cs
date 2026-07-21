namespace AgencyOS.Application.DTOs;

public class TaskResponse
{
    public Guid Id { get; set; }

    public Guid MissionId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid TaskTypeId { get; set; }

    public Guid TaskStatusId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public decimal EstimatedHours { get; set; }

    public DateOnly? PlannedStartDate { get; set; }

    public DateOnly? PlannedEndDate { get; set; }

    public DateOnly? ActualStartDate { get; set; }

    public DateOnly? ActualEndDate { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
