namespace AgencyOS.Application.DTOs;

public class DeliveryStrategyDecisionFactorsResponse
{
    public IReadOnlyList<DeliveryStrategyDecisionFactorResponse> Factors { get; set; } =
        Array.Empty<DeliveryStrategyDecisionFactorResponse>();
}
