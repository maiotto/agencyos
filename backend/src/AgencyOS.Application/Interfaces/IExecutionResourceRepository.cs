using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IExecutionResourceRepository
{
    Task<IReadOnlyList<ExecutionResource>> GetAllAsync(
        ExecutionResourceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutionResource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithCodeAsync(
        string code,
        Guid? excludeResourceId = null,
        CancellationToken cancellationToken = default);

    Task<ExecutionResource> AddAsync(ExecutionResource resource, CancellationToken cancellationToken = default);

    Task<ExecutionResource> UpdateAsync(ExecutionResource resource, CancellationToken cancellationToken = default);
}
