using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IAvailabilityEngineService
{
    Task<IReadOnlyList<AvailabilityResponse>> GetAllAsync(
        AvailabilityQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<AvailabilityResponse> GetByResourceIdAsync(
        Guid resourceId,
        AvailabilityQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<AvailabilitySummaryResponse> GetSummaryAsync(
        AvailabilityQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
