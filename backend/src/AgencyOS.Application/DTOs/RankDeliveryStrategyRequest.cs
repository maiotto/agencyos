namespace AgencyOS.Application.DTOs;

public class RankDeliveryStrategyRequest
{
    public Guid ContractId { get; set; }

    public Guid MissionId { get; set; }

    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    public Guid CompanyDecisionProfileId { get; set; }
}
