using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IDeliveryStrategyRankingService
{
    Task<RankDeliveryStrategyResponse> RankAsync(
        RankDeliveryStrategyRequest request,
        CancellationToken cancellationToken = default);
}
