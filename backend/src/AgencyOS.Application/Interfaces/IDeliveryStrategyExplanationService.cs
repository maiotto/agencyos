using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IDeliveryStrategyExplanationService
{
    Task<DeliveryStrategyExplanationResponse> GetExplanationAsync(
        Guid strategyId,
        DeliveryStrategyExplanationQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
