using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface ICapacityHistoryService
{
    Task PersistCompletedCalculationAsync(
        CapacityResponse capacity,
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task PersistCompletedCalculationsAsync(
        IReadOnlyList<(CapacityResponse Capacity, Guid CompanyId)> capacities,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CapacityHistoryResponse>> QueryAsync(
        CapacityHistoryQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<CapacityHistoryResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CapacityHistoryResponse>> GetByExecutionResourceIdAsync(
        Guid executionResourceId,
        CapacityHistoryQueryParameters? parameters = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CapacityHistoryResponse>> GetByCompanyIdAsync(
        Guid companyId,
        CapacityHistoryQueryParameters? parameters = null,
        CancellationToken cancellationToken = default);

    Task<CapacityHistoryCompareResponse> CompareAsync(
        CapacityHistoryCompareQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<CapacityHistoryAggregateResponse> AggregateAsync(
        CapacityHistoryQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
