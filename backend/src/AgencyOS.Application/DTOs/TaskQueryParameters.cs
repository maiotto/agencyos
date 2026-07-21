namespace AgencyOS.Application.DTOs;

public class TaskQueryParameters
{
    public Guid? MissionId { get; set; }

    public string? Status { get; set; }

    public string? Priority { get; set; }

    public Guid? TaskTypeId { get; set; }
}
