using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IClientContractRepository
{
    Task<(IReadOnlyList<ClientContract> Items, int TotalCount)> GetPagedAsync(
        ContractQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ClientContract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithContractCodeAsync(
        string contractCode,
        Guid? excludeContractId = null,
        CancellationToken cancellationToken = default);

    Task<ClientContract> AddAsync(ClientContract contract, CancellationToken cancellationToken = default);

    Task<ClientContract> UpdateAsync(ClientContract contract, CancellationToken cancellationToken = default);
}
