using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IAllocationConflictDetectionService
{
    Task<IReadOnlyList<AllocationConflictResponse>> GetAllAsync(
        AllocationConflictQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AllocationConflictResponse>> GetByResourceIdAsync(
        Guid resourceId,
        AllocationConflictQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<AllocationConflictSummaryResponse> GetSummaryAsync(
        AllocationConflictQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
