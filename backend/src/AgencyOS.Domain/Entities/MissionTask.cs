namespace AgencyOS.Domain.Entities;

public class MissionTask
{
    public Guid Id { get; set; }

    public Guid MissionId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid TaskTypeId { get; set; }

    public Guid TaskStatusId { get; set; }

    public string Priority { get; set; } = TaskPriority.Medium;

    public decimal EstimatedHours { get; set; }

    public DateOnly? PlannedStart { get; set; }

    public DateOnly? PlannedEnd { get; set; }

    public DateOnly? ActualStart { get; set; }

    public DateOnly? ActualEnd { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public TaskStatusLookup? Status { get; set; }

    public TaskTypeLookup? Type { get; set; }
}
