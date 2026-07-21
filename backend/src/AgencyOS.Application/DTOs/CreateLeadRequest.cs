namespace AgencyOS.Application.DTOs;

public class CreateLeadRequest
{
    public string CompanyName { get; set; } = string.Empty;

    public string LeadName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string Source { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal? EstimatedContractValue { get; set; }

    public Guid? AssignedUserId { get; set; }

    public string? Website { get; set; }

    public string? Segment { get; set; }

    public string? Notes { get; set; }
}
