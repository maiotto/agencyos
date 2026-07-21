namespace AgencyOS.Application.DTOs;

public class DeliveryStrategyResourceMixResponse
{
    public IReadOnlyList<DeliveryStrategyResourceMixItemResponse> Items { get; set; } =
        Array.Empty<DeliveryStrategyResourceMixItemResponse>();
}
