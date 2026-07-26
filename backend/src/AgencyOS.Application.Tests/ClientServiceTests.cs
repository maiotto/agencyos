using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class ClientServiceTests
{
    private readonly Mock<IClientRepository> _clientRepository = new();
    private readonly Mock<ILogger<ClientService>> _logger = new();

    private ClientService CreateService() => new(_clientRepository.Object, _logger.Object);

    [Fact]
    public async Task GetPagedAsync_ReturnsClampedPageSize()
    {
        _clientRepository
            .Setup(repository => repository.GetPagedAsync(
                It.IsAny<ClientQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<Client>(), 0));

        var service = CreateService();
        var result = await service.GetPagedAsync(new ClientQueryParameters
        {
            Page = 1,
            PageSize = 500
        });

        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundWhenClientMissing()
    {
        var clientId = Guid.NewGuid();
        _clientRepository
            .Setup(repository => repository.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(clientId));
    }

    [Fact]
    public async Task CreateAsync_PersistsCanonicalStatus()
    {
        Client? persisted = null;
        _clientRepository
            .Setup(repository => repository.ExistsWithTaxIdentifierAsync(
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _clientRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
            .Callback<Client, CancellationToken>((client, _) => persisted = client)
            .ReturnsAsync((Client client, CancellationToken _) => client);

        var service = CreateService();
        var result = await service.CreateAsync(new CreateClientRequest
        {
            LegalName = "Acme Corporation Ltd.",
            TaxIdentifier = "12-3456789",
            Status = "active"
        });

        Assert.NotNull(persisted);
        Assert.Equal(ClientStatus.Active, persisted!.Status);
        Assert.Equal(ClientStatus.Active, result.Status);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflictWhenTaxIdentifierExists()
    {
        _clientRepository
            .Setup(repository => repository.ExistsWithTaxIdentifierAsync(
                "12-3456789",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(new CreateClientRequest
            {
                LegalName = "Acme",
                TaxIdentifier = "12-3456789",
                Status = ClientStatus.Active
            }));

        Assert.Contains("12-3456789", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_PersistsCanonicalStatus()
    {
        var clientId = Guid.NewGuid();
        var client = CreateClient(clientId, ClientStatus.Active);

        _clientRepository
            .Setup(repository => repository.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        _clientRepository
            .Setup(repository => repository.ExistsWithTaxIdentifierAsync(
                It.IsAny<string>(),
                clientId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _clientRepository
            .Setup(repository => repository.UpdateAsync(client, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var service = CreateService();
        var result = await service.UpdateAsync(clientId, new UpdateClientRequest
        {
            LegalName = "Acme Corporation Ltd.",
            Status = "INACTIVE"
        });

        Assert.Equal(ClientStatus.Inactive, client.Status);
        Assert.Equal(ClientStatus.Inactive, result.Status);
    }

    [Fact]
    public async Task DeactivateAsync_SetsInactive()
    {
        var clientId = Guid.NewGuid();
        var client = CreateClient(clientId, ClientStatus.Active);

        _clientRepository
            .Setup(repository => repository.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        _clientRepository
            .Setup(repository => repository.UpdateAsync(client, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var service = CreateService();
        await service.DeactivateAsync(clientId);

        Assert.Equal(ClientStatus.Inactive, client.Status);
        _clientRepository.Verify(
            repository => repository.UpdateAsync(client, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_IsIdempotentWhenAlreadyInactive()
    {
        var clientId = Guid.NewGuid();
        var client = CreateClient(clientId, ClientStatus.Inactive);

        _clientRepository
            .Setup(repository => repository.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var service = CreateService();
        await service.DeactivateAsync(clientId);

        _clientRepository.Verify(
            repository => repository.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Client CreateClient(Guid id, string status)
    {
        return new Client
        {
            Id = id,
            LegalName = "Acme",
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
