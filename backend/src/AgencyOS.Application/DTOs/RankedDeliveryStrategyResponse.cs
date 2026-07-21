namespace AgencyOS.Application.DTOs;

public class RankedDeliveryStrategyResponse
{
    public int RankPosition { get; set; }

    public decimal FinalScore { get; set; }

    public EvaluatedDeliveryStrategyResponse EvaluatedStrategy { get; set; } = new();

    public DeliveryStrategyDecisionFactorsResponse DecisionFactors { get; set; } = new();
}
