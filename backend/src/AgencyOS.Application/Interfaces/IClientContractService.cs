using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IClientContractService
{
    Task<PagedResponse<ContractResponse>> GetPagedAsync(
        ContractQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ContractResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ContractResponse> CreateAsync(
        CreateContractRequest request,
        CancellationToken cancellationToken = default);

    Task<ContractResponse> UpdateAsync(
        Guid id,
        UpdateContractRequest request,
        CancellationToken cancellationToken = default);

    Task<ContractResponse> CancelAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ContractResponse> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ContractResponse> CloseAsync(Guid id, CancellationToken cancellationToken = default);
}
