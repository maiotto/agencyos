namespace AgencyOS.Application.DTOs;

public class LeadQueryParameters
{
    public string? Status { get; set; }

    public string? Company { get; set; }

    public Guid? AssignedUserId { get; set; }

    /// <summary>
    /// When false (default), archived Leads are excluded from search results.
    /// </summary>
    public bool IncludeArchived { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
