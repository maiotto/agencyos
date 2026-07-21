namespace AgencyOS.Application.DTOs;

public class DeliveryStrategyExplanationResponse
{
    public Guid StrategyId { get; set; }

    public Guid ContractId { get; set; }

    public Guid MissionId { get; set; }

    public Guid CompanyDecisionProfileId { get; set; }

    public DeliveryStrategySummaryResponse StrategySummary { get; set; } = new();

    public DeliveryStrategyDecisionFactorsResponse DecisionFactors { get; set; } = new();

    public IReadOnlyList<DeliveryStrategyExplanationInsightResponse> Strengths { get; set; } =
        Array.Empty<DeliveryStrategyExplanationInsightResponse>();

    public IReadOnlyList<DeliveryStrategyExplanationInsightResponse> Weaknesses { get; set; } =
        Array.Empty<DeliveryStrategyExplanationInsightResponse>();

    public IReadOnlyList<DeliveryStrategyExplanationRiskResponse> Risks { get; set; } =
        Array.Empty<DeliveryStrategyExplanationRiskResponse>();

    public DeliveryStrategyResourceCompositionExplanationResponse ResourceComposition { get; set; } = new();

    public DeliveryStrategyCapacityImpactExplanationResponse CapacityImpact { get; set; } = new();

    public decimal EstimatedCost { get; set; }

    public decimal EstimatedDurationHours { get; set; }

    public long ElapsedMilliseconds { get; set; }
}
