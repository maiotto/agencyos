using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface ICapacityHistoryRepository
{
    Task<CapacityHistory> AddAsync(CapacityHistory history, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IReadOnlyList<CapacityHistory> histories, CancellationToken cancellationToken = default);

    Task<CapacityHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CapacityHistory>> QueryAsync(
        CapacityHistoryQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CapacityHistory>> GetByExecutionResourceIdAsync(
        Guid executionResourceId,
        CapacityHistoryQueryParameters? parameters = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CapacityHistory>> GetByCompanyIdAsync(
        Guid companyId,
        CapacityHistoryQueryParameters? parameters = null,
        CancellationToken cancellationToken = default);
}
