using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IResourceAvailabilityService
{
    Task<IReadOnlyList<ResourceAvailabilityResponse>> GetAllAsync(
        ResourceAvailabilityQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ResourceAvailabilityResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ResourceAvailabilityResponse> CreateAsync(
        CreateResourceAvailabilityRequest request,
        CancellationToken cancellationToken = default);

    Task<ResourceAvailabilityResponse> UpdateAsync(
        Guid id,
        UpdateResourceAvailabilityRequest request,
        CancellationToken cancellationToken = default);

    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationalResourceAvailabilityResponse> GetOperationalAvailabilityAsync(
        Guid executionResourceId,
        DateOnly date,
        CancellationToken cancellationToken = default);
}
