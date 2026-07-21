namespace AgencyOS.Application.DTOs;

public class DeliveryStrategyQueryParameters
{
    public Guid MissionId { get; set; }

    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }
}
