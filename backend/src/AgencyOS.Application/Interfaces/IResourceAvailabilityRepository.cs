using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IResourceAvailabilityRepository
{
    Task<IReadOnlyList<ResourceAvailability>> GetAllAsync(
        ResourceAvailabilityQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ResourceAvailability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResourceAvailability>> GetActiveOverlappingAsync(
        Guid executionResourceId,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        Guid? excludeResourceAvailabilityId = null,
        CancellationToken cancellationToken = default);

    Task<ResourceAvailability?> GetActiveCoveringDateAsync(
        Guid executionResourceId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResourceAvailability>> GetActiveCoveringPeriodAsync(
        Guid executionResourceId,
        DateOnly periodStartDate,
        DateOnly periodEndDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResourceAvailability>> GetActiveCoveringPeriodForResourcesAsync(
        IReadOnlyCollection<Guid> executionResourceIds,
        DateOnly periodStartDate,
        DateOnly periodEndDate,
        CancellationToken cancellationToken = default);

    Task<ResourceAvailability> AddAsync(
        ResourceAvailability resourceAvailability,
        CancellationToken cancellationToken = default);

    Task<ResourceAvailability> UpdateAsync(
        ResourceAvailability resourceAvailability,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        ResourceAvailability resourceAvailability,
        CancellationToken cancellationToken = default);
}
