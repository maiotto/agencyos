namespace AgencyOS.Application.DTOs;

public class ClientQueryParameters
{
    public string? Status { get; set; }

    public string? Industry { get; set; }

    public string? CompanyName { get; set; }

    public string OrderBy { get; set; } = "companyName";

    public string OrderDirection { get; set; } = "asc";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
