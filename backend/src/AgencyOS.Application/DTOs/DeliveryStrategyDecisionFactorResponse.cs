namespace AgencyOS.Application.DTOs;

public class DeliveryStrategyDecisionFactorResponse
{
    public string Dimension { get; set; } = string.Empty;

    public decimal RawValue { get; set; }

    public decimal NormalizedScore { get; set; }

    public decimal Weight { get; set; }

    public decimal WeightedContribution { get; set; }
}
