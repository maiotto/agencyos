namespace AgencyOS.Application.DTOs;

public class EvaluateDeliveryStrategyRequest
{
    public Guid ContractId { get; set; }

    public Guid MissionId { get; set; }

    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }
}
