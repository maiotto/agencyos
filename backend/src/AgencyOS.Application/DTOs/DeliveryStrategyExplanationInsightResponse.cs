namespace AgencyOS.Application.DTOs;

public class DeliveryStrategyExplanationInsightResponse
{
    public string Category { get; set; } = string.Empty;

    public string Dimension { get; set; } = string.Empty;

    public decimal NormalizedScore { get; set; }

    public decimal RawValue { get; set; }

    public string ReasonCode { get; set; } = string.Empty;
}
