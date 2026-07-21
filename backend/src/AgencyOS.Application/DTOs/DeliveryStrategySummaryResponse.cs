namespace AgencyOS.Application.DTOs;

public class DeliveryStrategySummaryResponse
{
    public Guid StrategyId { get; set; }

    public string StrategyName { get; set; } = string.Empty;

    public int RankPosition { get; set; }

    public int TotalRankedStrategies { get; set; }

    public decimal FinalScore { get; set; }

    public string CompanyDecisionProfileName { get; set; } = string.Empty;
}
