using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class MissionServiceTests
{
    private readonly Mock<IMissionRepository> _missionRepository = new();
    private readonly Mock<IClientContractRepository> _contractRepository = new();
    private readonly Mock<ILogger<MissionService>> _logger = new();

    private MissionService CreateService() =>
        new(_missionRepository.Object, _contractRepository.Object, _logger.Object);

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundWhenMissionMissing()
    {
        var missionId = Guid.NewGuid();
        _missionRepository
            .Setup(repository => repository.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mission?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(missionId));
    }

    [Fact]
    public async Task CreateAsync_PersistsMissionWhenContractIsActive()
    {
        var contractId = Guid.NewGuid();
        Mission? persisted = null;

        SetupActiveContract(contractId);
        SetupUniqueCode();

        _missionRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Mission>(), It.IsAny<CancellationToken>()))
            .Callback<Mission, CancellationToken>((mission, _) => persisted = mission)
            .ReturnsAsync((Mission mission, CancellationToken _) => mission);

        var service = CreateService();
        var result = await service.CreateAsync(CreateValidRequest(contractId));

        Assert.NotNull(persisted);
        Assert.Equal(contractId, persisted!.ClientContractId);
        Assert.Equal("MSN-2026-001", persisted.Code);
        Assert.Equal("Acme Delivery Mission", result.Name);
    }

    [Fact]
    public async Task CreateAsync_TrimsTextAndNormalizesOptionalDescription()
    {
        var contractId = Guid.NewGuid();
        Mission? persisted = null;

        SetupActiveContract(contractId);
        SetupUniqueCode();

        _missionRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Mission>(), It.IsAny<CancellationToken>()))
            .Callback<Mission, CancellationToken>((mission, _) => persisted = mission)
            .ReturnsAsync((Mission mission, CancellationToken _) => mission);

        var request = CreateValidRequest(contractId);
        request.Code = "  MSN-2026-001  ";
        request.Name = "  Acme Delivery Mission  ";
        request.Description = "   ";
        request.Priority = "  HIGH  ";

        var service = CreateService();
        await service.CreateAsync(request);

        Assert.NotNull(persisted);
        Assert.Equal("MSN-2026-001", persisted!.Code);
        Assert.Equal("Acme Delivery Mission", persisted.Name);
        Assert.Null(persisted.Description);
        Assert.Equal("HIGH", persisted.Priority);
    }

    [Fact]
    public async Task CreateAsync_ThrowsNotFoundWhenContractMissing()
    {
        var contractId = Guid.NewGuid();
        _contractRepository
            .Setup(repository => repository.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClientContract?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateAsync(CreateValidRequest(contractId)));
    }

    [Fact]
    public async Task CreateAsync_ThrowsBusinessRuleWhenContractIsNotActive()
    {
        var contractId = Guid.NewGuid();
        _contractRepository
            .Setup(repository => repository.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClientContract
            {
                Id = contractId,
                ClientId = Guid.NewGuid(),
                ContractNumber = "CTR-001",
                Name = "Acme",
                BillingModel = ContractType.MonthlyRetainer,
                Status = ContractStatus.Draft,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.CreateAsync(CreateValidRequest(contractId)));

        Assert.Equal("Only Active Contracts may authorize Mission creation.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflictWhenMissionCodeExists()
    {
        var contractId = Guid.NewGuid();
        SetupActiveContract(contractId);

        _missionRepository
            .Setup(repository => repository.ExistsWithCodeAsync(
                "MSN-2026-001",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(CreateValidRequest(contractId)));

        Assert.Contains("MSN-2026-001", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsConflictWhenMissionCodeExists()
    {
        var missionId = Guid.NewGuid();
        var mission = CreateMission(missionId);

        _missionRepository
            .Setup(repository => repository.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mission);

        _missionRepository
            .Setup(repository => repository.ExistsWithCodeAsync(
                "MSN-DUP",
                missionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var request = CreateValidUpdateRequest();
        request.Code = "MSN-DUP";

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.UpdateAsync(missionId, request));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesMissionFields()
    {
        var missionId = Guid.NewGuid();
        var mission = CreateMission(missionId);

        _missionRepository
            .Setup(repository => repository.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mission);

        _missionRepository
            .Setup(repository => repository.ExistsWithCodeAsync(
                It.IsAny<string>(),
                missionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _missionRepository
            .Setup(repository => repository.UpdateAsync(mission, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mission);

        var service = CreateService();
        var request = CreateValidUpdateRequest();
        request.Name = "Updated Mission";

        var result = await service.UpdateAsync(missionId, request);

        Assert.Equal("Updated Mission", mission.Name);
        Assert.Equal("Updated Mission", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesMission()
    {
        var missionId = Guid.NewGuid();
        var mission = CreateMission(missionId);

        _missionRepository
            .Setup(repository => repository.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mission);

        _missionRepository
            .Setup(repository => repository.DeleteAsync(mission, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();
        await service.DeleteAsync(missionId);

        _missionRepository.Verify(
            repository => repository.DeleteAsync(mission, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void SetupActiveContract(Guid contractId)
    {
        _contractRepository
            .Setup(repository => repository.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClientContract
            {
                Id = contractId,
                ClientId = Guid.NewGuid(),
                ContractNumber = "CTR-001",
                Name = "Acme",
                BillingModel = ContractType.MonthlyRetainer,
                Status = ContractStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
    }

    private void SetupUniqueCode()
    {
        _missionRepository
            .Setup(repository => repository.ExistsWithCodeAsync(
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private static CreateMissionRequest CreateValidRequest(Guid contractId)
    {
        return new CreateMissionRequest
        {
            ClientContractId = contractId,
            Code = "MSN-2026-001",
            Name = "Acme Delivery Mission",
            Description = "Primary delivery mission",
            MissionTypeId = Guid.Parse("11111111-1111-4111-8111-000000000002"),
            MissionStatusId = Guid.Parse("11111111-1111-4111-8121-000000000002"),
            Priority = "NORMAL",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 6, 30)
        };
    }

    private static UpdateMissionRequest CreateValidUpdateRequest()
    {
        return new UpdateMissionRequest
        {
            ClientContractId = Guid.NewGuid(),
            Code = "MSN-2026-001",
            Name = "Acme Delivery Mission",
            Description = "Primary delivery mission",
            MissionTypeId = Guid.Parse("11111111-1111-4111-8111-000000000002"),
            MissionStatusId = Guid.Parse("11111111-1111-4111-8121-000000000002"),
            Priority = "NORMAL",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 6, 30)
        };
    }

    private static Mission CreateMission(Guid id)
    {
        return new Mission
        {
            Id = id,
            ClientContractId = Guid.NewGuid(),
            Code = "MSN-2026-001",
            Name = "Acme Delivery Mission",
            MissionTypeId = Guid.Parse("11111111-1111-4111-8111-000000000002"),
            MissionStatusId = Guid.Parse("11111111-1111-4111-8121-000000000002"),
            Priority = "NORMAL",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
