using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IExecutionResourceService
{
    Task<IReadOnlyList<ExecutionResourceResponse>> GetAllAsync(
        ExecutionResourceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutionResourceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ExecutionResourceResponse> CreateAsync(
        CreateExecutionResourceRequest request,
        CancellationToken cancellationToken = default);

    Task<ExecutionResourceResponse> UpdateAsync(
        Guid id,
        UpdateExecutionResourceRequest request,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
