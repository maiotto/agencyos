namespace AgencyOS.Application.DTOs;

public class DeliveryStrategyEvaluationMetricsResponse
{
    public decimal EstimatedCost { get; set; }

    public decimal EstimatedDurationHours { get; set; }

    public decimal CapacityUtilizationPercentage { get; set; }

    public decimal ResourceUtilizationPercentage { get; set; }

    public decimal WorkloadImpactPercentage { get; set; }

    public decimal AvailabilityImpactPercentage { get; set; }

    public decimal OperationalRiskScore { get; set; }

    public decimal HumanUtilizationPercentage { get; set; }

    public decimal AiUtilizationPercentage { get; set; }

    public decimal AutomationUtilizationPercentage { get; set; }

    public decimal ExternalResourceUtilizationPercentage { get; set; }
}
