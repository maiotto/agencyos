namespace AgencyOS.Application.DTOs;

public class UpdateContractRequest
{
    public string ContractCode { get; set; } = string.Empty;

    public string ContractName { get; set; } = string.Empty;

    public string ContractType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal EstimatedValue { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateOnly? RenewalDate { get; set; }
}
