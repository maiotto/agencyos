namespace AgencyOS.Application.DTOs;

public class ContractQueryParameters
{
    public Guid? ClientId { get; set; }

    public string? Status { get; set; }

    public string? ContractType { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string OrderBy { get; set; } = "contractName";

    public string OrderDirection { get; set; } = "asc";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
