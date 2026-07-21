namespace AgencyOS.Application.DTOs;

public class RankDeliveryStrategyResponse
{
    public Guid ContractId { get; set; }

    public Guid MissionId { get; set; }

    public Guid CompanyDecisionProfileId { get; set; }

    public string CompanyDecisionProfileName { get; set; } = string.Empty;

    public IReadOnlyList<RankedDeliveryStrategyResponse> RankedStrategies { get; set; } =
        Array.Empty<RankedDeliveryStrategyResponse>();

    public int RankedStrategyCount { get; set; }

    public long ElapsedMilliseconds { get; set; }
}
