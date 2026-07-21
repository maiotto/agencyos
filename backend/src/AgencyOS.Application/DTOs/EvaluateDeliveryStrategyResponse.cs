namespace AgencyOS.Application.DTOs;

public class EvaluateDeliveryStrategyResponse
{
    public Guid ContractId { get; set; }

    public Guid MissionId { get; set; }

    public IReadOnlyList<EvaluatedDeliveryStrategyResponse> EvaluatedStrategies { get; set; } =
        Array.Empty<EvaluatedDeliveryStrategyResponse>();

    public int EvaluatedStrategyCount { get; set; }

    public long ElapsedMilliseconds { get; set; }
}
