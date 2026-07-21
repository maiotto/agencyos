using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IClientRepository
{
    Task<(IReadOnlyList<Client> Items, int TotalCount)> GetPagedAsync(
        ClientQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithTaxIdentifierAsync(
        string taxIdentifier,
        Guid? excludeClientId = null,
        CancellationToken cancellationToken = default);

    Task<Client> AddAsync(Client client, CancellationToken cancellationToken = default);

    Task<Client> UpdateAsync(Client client, CancellationToken cancellationToken = default);
}
