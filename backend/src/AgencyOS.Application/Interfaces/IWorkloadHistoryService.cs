using AgencyOS.Application.DTOs;
using AgencyOS.Application.Mappings;

namespace AgencyOS.Application.Interfaces;

public interface IWorkloadHistoryService
{
    Task PersistCompletedCalculationAsync(
        WorkloadHistoryPersistModel model,
        CancellationToken cancellationToken = default);

    Task PersistCompletedCalculationsAsync(
        IReadOnlyList<WorkloadHistoryPersistModel> models,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkloadHistoryResponse>> QueryAsync(
        WorkloadHistoryQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<WorkloadHistoryResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkloadHistoryResponse>> GetByExecutionResourceIdAsync(
        Guid executionResourceId,
        WorkloadHistoryQueryParameters? parameters = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkloadHistoryResponse>> GetByCompanyIdAsync(
        Guid companyId,
        WorkloadHistoryQueryParameters? parameters = null,
        CancellationToken cancellationToken = default);

    Task<WorkloadHistoryCompareResponse> CompareAsync(
        WorkloadHistoryCompareQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<WorkloadHistoryAggregateResponse> AggregateAsync(
        WorkloadHistoryQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<WorkloadHistoryTrendResponse> TrendsAsync(
        WorkloadHistoryQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
