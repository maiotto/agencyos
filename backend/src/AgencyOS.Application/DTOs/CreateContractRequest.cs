using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.DTOs;

public class CreateContractRequest
{
    public Guid ClientId { get; set; }

    public string ContractCode { get; set; } = string.Empty;

    public string ContractName { get; set; } = string.Empty;

    public string ContractType { get; set; } = string.Empty;

    public string Status { get; set; } = ContractStatus.Draft;

    public decimal EstimatedValue { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateOnly? RenewalDate { get; set; }
}
