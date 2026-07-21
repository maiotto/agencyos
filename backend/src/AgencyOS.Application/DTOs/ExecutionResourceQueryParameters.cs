namespace AgencyOS.Application.DTOs;

public class ExecutionResourceQueryParameters
{
    public string? ResourceType { get; set; }

    public string? Status { get; set; }

    public string? Skill { get; set; }

    public string OrderBy { get; set; } = "resourceName";

    public string OrderDirection { get; set; } = "asc";
}
