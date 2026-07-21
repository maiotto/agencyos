namespace AgencyOS.Application.DTOs;

public class DeliveryStrategyResourceMixItemResponse
{
    public string ResourceType { get; set; } = string.Empty;

    public int ResourceCount { get; set; }

    public decimal PlannedHours { get; set; }

    public decimal PercentageOfHours { get; set; }
}
