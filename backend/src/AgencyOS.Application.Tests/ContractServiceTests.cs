using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class ContractServiceTests
{
    private readonly Mock<IClientContractRepository> _contractRepository = new();
    private readonly Mock<IClientRepository> _clientRepository = new();
    private readonly Mock<ILogger<ClientContractService>> _logger = new();

    private ClientContractService CreateService() =>
        new(_contractRepository.Object, _clientRepository.Object, _logger.Object);

    [Fact]
    public async Task GetPagedAsync_ReturnsClampedPageSize()
    {
        _contractRepository
            .Setup(repository => repository.GetPagedAsync(
                It.IsAny<ContractQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<ClientContract>(), 0));

        var service = CreateService();
        var result = await service.GetPagedAsync(new ContractQueryParameters
        {
            Page = 1,
            PageSize = 500
        });

        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundWhenContractMissing()
    {
        var contractId = Guid.NewGuid();
        _contractRepository
            .Setup(repository => repository.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClientContract?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(contractId));
    }

    [Fact]
    public async Task CreateAsync_PersistsDraftRegardlessOfRequestedStatus()
    {
        var clientId = Guid.NewGuid();
        ClientContract? persisted = null;

        SetupActiveClient(clientId);
        SetupUniqueCode();

        _contractRepository
            .Setup(repository => repository.AddAsync(It.IsAny<ClientContract>(), It.IsAny<CancellationToken>()))
            .Callback<ClientContract, CancellationToken>((contract, _) => persisted = contract)
            .ReturnsAsync((ClientContract contract, CancellationToken _) => contract);

        var service = CreateService();
        var result = await service.CreateAsync(CreateValidRequest(clientId, ContractStatus.Active));

        Assert.NotNull(persisted);
        Assert.Equal(ContractStatus.Draft, persisted!.Status);
        Assert.Equal(ContractStatus.Draft, result.Status);
    }

    [Fact]
    public async Task CreateAsync_ThrowsNotFoundWhenClientMissing()
    {
        var clientId = Guid.NewGuid();
        _clientRepository
            .Setup(repository => repository.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateAsync(CreateValidRequest(clientId)));
    }

    [Fact]
    public async Task CreateAsync_ThrowsBusinessRuleWhenClientIsNotActive()
    {
        var clientId = Guid.NewGuid();
        _clientRepository
            .Setup(repository => repository.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Client
            {
                Id = clientId,
                LegalName = "Acme",
                Status = ClientStatus.Inactive,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.CreateAsync(CreateValidRequest(clientId)));

        Assert.Equal("Only Active Clients may receive new Contracts.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflictWhenContractCodeExists()
    {
        var clientId = Guid.NewGuid();
        SetupActiveClient(clientId);

        _contractRepository
            .Setup(repository => repository.ExistsWithContractCodeAsync(
                "CTR-2026-001",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(CreateValidRequest(clientId)));

        Assert.Contains("CTR-2026-001", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_RejectsClosedContract()
    {
        var contractId = Guid.NewGuid();
        var contract = CreateContract(contractId, ContractStatus.Closed);

        _contractRepository
            .Setup(repository => repository.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.UpdateAsync(contractId, CreateValidUpdateRequest(ContractStatus.Draft)));

        Assert.Equal("Closed or Cancelled Contracts cannot be edited.", exception.Message);
    }

    [Fact]
    public async Task ActivateAsync_ActivatesDraftContract()
    {
        var contractId = Guid.NewGuid();
        var contract = CreateContract(contractId, ContractStatus.Draft);

        _contractRepository
            .Setup(repository => repository.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        _contractRepository
            .Setup(repository => repository.UpdateAsync(contract, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var service = CreateService();
        var result = await service.ActivateAsync(contractId);

        Assert.Equal(ContractStatus.Active, contract.Status);
        Assert.Equal(ContractStatus.Active, result.Status);
    }

    [Fact]
    public async Task ActivateAsync_RejectsNonDraftContract()
    {
        var contractId = Guid.NewGuid();
        var contract = CreateContract(contractId, ContractStatus.Suspended);

        _contractRepository
            .Setup(repository => repository.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.ActivateAsync(contractId));

        Assert.Equal("Only Draft Contracts can be activated.", exception.Message);
    }

    [Fact]
    public async Task CloseAsync_ClosesActiveContract()
    {
        var contractId = Guid.NewGuid();
        var contract = CreateContract(contractId, ContractStatus.Active);

        _contractRepository
            .Setup(repository => repository.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        _contractRepository
            .Setup(repository => repository.UpdateAsync(contract, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var service = CreateService();
        var result = await service.CloseAsync(contractId);

        Assert.Equal(ContractStatus.Closed, result.Status);
    }

    [Fact]
    public async Task CancelAsync_CancelsDraftContract()
    {
        var contractId = Guid.NewGuid();
        var contract = CreateContract(contractId, ContractStatus.Draft);

        _contractRepository
            .Setup(repository => repository.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        _contractRepository
            .Setup(repository => repository.UpdateAsync(contract, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var service = CreateService();
        var result = await service.CancelAsync(contractId);

        Assert.Equal(ContractStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task CancelAsync_IsIdempotentWhenAlreadyCancelled()
    {
        var contractId = Guid.NewGuid();
        var contract = CreateContract(contractId, ContractStatus.Cancelled);

        _contractRepository
            .Setup(repository => repository.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var service = CreateService();
        var result = await service.CancelAsync(contractId);

        Assert.Equal(ContractStatus.Cancelled, result.Status);
        _contractRepository.Verify(
            repository => repository.UpdateAsync(It.IsAny<ClientContract>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupActiveClient(Guid clientId)
    {
        _clientRepository
            .Setup(repository => repository.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Client
            {
                Id = clientId,
                LegalName = "Acme",
                Status = ClientStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
    }

    private void SetupUniqueCode()
    {
        _contractRepository
            .Setup(repository => repository.ExistsWithContractCodeAsync(
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private static CreateContractRequest CreateValidRequest(
        Guid clientId,
        string status = ContractStatus.Draft)
    {
        return new CreateContractRequest
        {
            ClientId = clientId,
            ContractCode = "CTR-2026-001",
            ContractName = "Acme Retainer 2026",
            ContractType = ContractType.MonthlyRetainer,
            Status = status,
            EstimatedValue = 120000,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31)
        };
    }

    private static UpdateContractRequest CreateValidUpdateRequest(string status)
    {
        return new UpdateContractRequest
        {
            ContractCode = "CTR-2026-001",
            ContractName = "Acme Retainer 2026",
            ContractType = ContractType.MonthlyRetainer,
            Status = status,
            EstimatedValue = 120000,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31)
        };
    }

    private static ClientContract CreateContract(Guid id, string status)
    {
        return new ClientContract
        {
            Id = id,
            ClientId = Guid.NewGuid(),
            ContractNumber = "CTR-2026-001",
            Name = "Acme Retainer 2026",
            BillingModel = ContractType.MonthlyRetainer,
            Value = 120000,
            Status = status,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
