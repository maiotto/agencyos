namespace AgencyOS.Application.DTOs;

public class DeliveryStrategyResourceCompositionExplanationResponse
{
    public IReadOnlyList<DeliveryStrategyResourceMixItemResponse> Items { get; set; } =
        Array.Empty<DeliveryStrategyResourceMixItemResponse>();

    public decimal HumanUtilizationPercentage { get; set; }

    public decimal AiUtilizationPercentage { get; set; }

    public decimal ExternalResourceUtilizationPercentage { get; set; }

    public decimal AutomationUtilizationPercentage { get; set; }
}
