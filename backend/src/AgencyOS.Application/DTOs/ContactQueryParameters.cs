namespace AgencyOS.Application.DTOs;

public class ContactQueryParameters
{
    public Guid? ClientId { get; set; }

    public string? Status { get; set; }

    public bool? IsPrimaryContact { get; set; }

    public string OrderBy { get; set; } = "firstName";

    public string OrderDirection { get; set; } = "asc";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
