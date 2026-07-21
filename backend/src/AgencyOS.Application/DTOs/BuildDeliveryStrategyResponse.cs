namespace AgencyOS.Application.DTOs;

public class BuildDeliveryStrategyResponse
{
    public Guid ContractId { get; set; }

    public Guid MissionId { get; set; }

    public IReadOnlyList<DeliveryStrategyResponse> Strategies { get; set; } =
        Array.Empty<DeliveryStrategyResponse>();

    public int GeneratedStrategyCount { get; set; }

    public long ElapsedMilliseconds { get; set; }
}
