namespace AgencyOS.Application.DTOs;

public class DeliveryStrategyExplanationRiskResponse
{
    public string Metric { get; set; } = string.Empty;

    public decimal Value { get; set; }

    public string ReasonCode { get; set; } = string.Empty;
}
