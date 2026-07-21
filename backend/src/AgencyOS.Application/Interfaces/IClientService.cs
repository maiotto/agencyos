using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IClientService
{
    Task<PagedResponse<ClientResponse>> GetPagedAsync(
        ClientQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ClientResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ClientResponse> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken = default);

    Task<ClientResponse> UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
