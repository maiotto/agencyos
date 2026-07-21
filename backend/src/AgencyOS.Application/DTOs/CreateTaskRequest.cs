using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.DTOs;

public class CreateTaskRequest
{
    public Guid MissionId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid TaskTypeId { get; set; }

    public Guid TaskStatusId { get; set; }

    public string Priority { get; set; } = TaskPriority.Medium;

    public decimal EstimatedHours { get; set; }

    public DateOnly? PlannedStartDate { get; set; }

    public DateOnly? PlannedEndDate { get; set; }
}
