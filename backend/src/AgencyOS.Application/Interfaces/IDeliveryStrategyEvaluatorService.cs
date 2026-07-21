using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IDeliveryStrategyEvaluatorService
{
    Task<EvaluateDeliveryStrategyResponse> EvaluateAsync(
        EvaluateDeliveryStrategyRequest request,
        CancellationToken cancellationToken = default);
}
