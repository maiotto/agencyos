using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IWorkloadHistoryRepository
{
    Task<WorkloadHistory> AddAsync(WorkloadHistory history, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IReadOnlyList<WorkloadHistory> histories, CancellationToken cancellationToken = default);

    Task<WorkloadHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkloadHistory>> QueryAsync(
        WorkloadHistoryQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkloadHistory>> GetByExecutionResourceIdAsync(
        Guid executionResourceId,
        WorkloadHistoryQueryParameters? parameters = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkloadHistory>> GetByCompanyIdAsync(
        Guid companyId,
        WorkloadHistoryQueryParameters? parameters = null,
        CancellationToken cancellationToken = default);
}
