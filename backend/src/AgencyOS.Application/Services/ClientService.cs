using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<ClientService> _logger;

    public ClientService(IClientRepository clientRepository, ILogger<ClientService> logger)
    {
        _clientRepository = clientRepository;
        _logger = logger;
    }

    public async Task<PagedResponse<ClientResponse>> GetPagedAsync(
        ClientQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _clientRepository.GetPagedAsync(parameters, cancellationToken);

        var page = Math.Max(1, parameters.Page);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);

        return new PagedResponse<ClientResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<ClientResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await GetClientOrThrowAsync(id, cancellationToken);
        return MapToResponse(client);
    }

    public async Task<ClientResponse> CreateAsync(
        CreateClientRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureTaxIdentifierIsUniqueAsync(request.TaxIdentifier, null, cancellationToken);

        var now = DateTimeOffset.UtcNow;

        var client = new Client
        {
            Id = Guid.NewGuid(),
            LegalName = request.LegalName.Trim(),
            TradeName = NormalizeOptionalText(request.TradeName),
            TaxId = NormalizeOptionalText(request.TaxIdentifier),
            Website = NormalizeOptionalText(request.Website),
            Segment = NormalizeOptionalText(request.Industry),
            Status = ClientStatus.Normalize(request.Status),
            AccountOwner = request.AccountOwner,
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _clientRepository.AddAsync(client, cancellationToken);

        _logger.LogInformation("Client Created: {ClientId} ({LegalName})", created.Id, created.LegalName);

        return MapToResponse(created);
    }

    public async Task<ClientResponse> UpdateAsync(
        Guid id,
        UpdateClientRequest request,
        CancellationToken cancellationToken = default)
    {
        var client = await GetClientOrThrowAsync(id, cancellationToken);

        await EnsureTaxIdentifierIsUniqueAsync(request.TaxIdentifier, id, cancellationToken);

        client.LegalName = request.LegalName.Trim();
        client.TradeName = NormalizeOptionalText(request.TradeName);
        client.TaxId = NormalizeOptionalText(request.TaxIdentifier);
        client.Website = NormalizeOptionalText(request.Website);
        client.Segment = NormalizeOptionalText(request.Industry);
        client.Status = ClientStatus.Normalize(request.Status);
        client.AccountOwner = request.AccountOwner;
        client.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _clientRepository.UpdateAsync(client, cancellationToken);

        _logger.LogInformation("Client Updated: {ClientId} ({LegalName})", updated.Id, updated.LegalName);

        return MapToResponse(updated);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await GetClientOrThrowAsync(id, cancellationToken);

        if (string.Equals(client.Status, ClientStatus.Inactive, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        client.Status = ClientStatus.Inactive;
        client.UpdatedAt = DateTimeOffset.UtcNow;

        await _clientRepository.UpdateAsync(client, cancellationToken);

        _logger.LogInformation("Client Deactivated: {ClientId} ({LegalName})", client.Id, client.LegalName);
    }

    private async Task<Client> GetClientOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var client = await _clientRepository.GetByIdAsync(id, cancellationToken);

        if (client is null)
        {
            throw new NotFoundException($"Client with id '{id}' was not found.");
        }

        return client;
    }

    private async Task EnsureTaxIdentifierIsUniqueAsync(
        string? taxIdentifier,
        Guid? excludeClientId,
        CancellationToken cancellationToken)
    {
        var normalizedTaxIdentifier = NormalizeOptionalText(taxIdentifier);

        if (normalizedTaxIdentifier is null)
        {
            return;
        }

        if (await _clientRepository.ExistsWithTaxIdentifierAsync(
                normalizedTaxIdentifier,
                excludeClientId,
                cancellationToken))
        {
            throw new ConflictException($"A Client with tax identifier '{normalizedTaxIdentifier}' already exists.");
        }
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static ClientResponse MapToResponse(Client client)
    {
        return new ClientResponse
        {
            Id = client.Id,
            LeadId = client.LeadId,
            LegalName = client.LegalName,
            TradeName = client.TradeName,
            TaxIdentifier = client.TaxId,
            Website = client.Website,
            Industry = client.Segment,
            Status = client.Status,
            AccountOwner = client.AccountOwner,
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt
        };
    }
}
