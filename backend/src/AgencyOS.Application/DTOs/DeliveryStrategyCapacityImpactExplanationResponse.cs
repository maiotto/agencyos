namespace AgencyOS.Application.DTOs;

public class DeliveryStrategyCapacityImpactExplanationResponse
{
    public decimal CapacityUtilizationPercentage { get; set; }

    public decimal WorkloadImpactPercentage { get; set; }

    public decimal AvailabilityImpactPercentage { get; set; }

    public decimal ResourceUtilizationPercentage { get; set; }
}
