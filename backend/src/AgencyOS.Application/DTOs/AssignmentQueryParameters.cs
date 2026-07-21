namespace AgencyOS.Application.DTOs;

public class AssignmentQueryParameters
{
    public Guid? TaskId { get; set; }

    public Guid? MissionId { get; set; }

    public Guid? ExecutionResourceId { get; set; }

    public string? ResourceType { get; set; }

    public string? Status { get; set; }

    public string OrderBy { get; set; } = "plannedStartDate";

    public string OrderDirection { get; set; } = "asc";
}
