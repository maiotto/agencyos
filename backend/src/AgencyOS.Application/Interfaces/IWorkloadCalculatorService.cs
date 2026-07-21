using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IWorkloadCalculatorService
{
    Task<IReadOnlyList<WorkloadResponse>> GetAllAsync(
        WorkloadQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<WorkloadResponse> GetByResourceIdAsync(
        Guid resourceId,
        WorkloadQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<WorkloadSummaryResponse> GetSummaryAsync(
        WorkloadQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
