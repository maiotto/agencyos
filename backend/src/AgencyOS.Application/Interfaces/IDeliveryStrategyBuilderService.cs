using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IDeliveryStrategyBuilderService
{
    Task<BuildDeliveryStrategyResponse> BuildAsync(
        BuildDeliveryStrategyRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DeliveryStrategyResponse>> GetByContractIdAsync(
        Guid contractId,
        DeliveryStrategyQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
