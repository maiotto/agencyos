namespace AgencyOS.Application.DTOs;

public class EvaluatedDeliveryStrategyResponse
{
    public DeliveryStrategyResponse Strategy { get; set; } = new();

    public DeliveryStrategyEvaluationMetricsResponse EvaluationMetrics { get; set; } = new();
}
