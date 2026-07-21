using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface ICapacityCalculatorService
{
    Task<IReadOnlyList<CapacityResponse>> GetAllAsync(
        CapacityQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<CapacityResponse> GetByResourceIdAsync(
        Guid resourceId,
        CapacityQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<CapacitySummaryResponse> GetSummaryAsync(
        CapacityQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
