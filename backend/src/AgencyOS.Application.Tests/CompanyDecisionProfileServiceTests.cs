using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class CompanyDecisionProfileServiceTests
{
    private static readonly Guid CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

    private readonly Mock<ICompanyDecisionProfileRepository> _repository = new();
    private readonly Mock<ICompanyRepository> _companyRepository = new();
    private readonly Mock<IAuditService> _auditService = new();
    private readonly Mock<ILogger<CompanyDecisionProfileService>> _logger = new();

    private CompanyDecisionProfileService CreateService() =>
        new(_repository.Object, _companyRepository.Object, _auditService.Object, _logger.Object);

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundWhenMissing()
    {
        var profileId = Guid.NewGuid();
        _repository
            .Setup(repository => repository.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CompanyDecisionProfile?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CreateService().GetByIdAsync(profileId));
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflictWhenCodeAlreadyExists()
    {
        _repository
            .Setup(repository => repository.ExistsCodeAsync(
                CompanyId, "DupCode", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateService().CreateAsync(CreateValidRequest(code: "DupCode")));
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflictWhenActiveNameAlreadyExists_BR1902()
    {
        _repository
            .Setup(repository => repository.ExistsCodeAsync(
                CompanyId, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository
            .Setup(repository => repository.ExistsActiveNameAsync(
                CompanyId, "Duplicate Name", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateService().CreateAsync(CreateValidRequest(name: "Duplicate Name")));
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflictWhenAnotherDefaultExists_BR1901()
    {
        _repository
            .Setup(repository => repository.ExistsCodeAsync(
                CompanyId, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository
            .Setup(repository => repository.ExistsActiveNameAsync(
                CompanyId, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository
            .Setup(repository => repository.GetDefaultActiveAsync(CompanyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProfile(defaultProfile: true));

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateService().CreateAsync(CreateValidRequest(defaultProfile: true)));
    }

    [Fact]
    public async Task CreateAsync_PersistsAndRecordsAudit()
    {
        CompanyDecisionProfile? persisted = null;
        _repository
            .Setup(repository => repository.ExistsCodeAsync(
                CompanyId, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository
            .Setup(repository => repository.ExistsActiveNameAsync(
                CompanyId, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository
            .Setup(repository => repository.AddAsync(It.IsAny<CompanyDecisionProfile>(), It.IsAny<CancellationToken>()))
            .Callback<CompanyDecisionProfile, CancellationToken>((profile, _) => persisted = profile)
            .ReturnsAsync((CompanyDecisionProfile profile, CancellationToken _) => profile);

        var response = await CreateService().CreateAsync(CreateValidRequest());

        Assert.NotNull(persisted);
        Assert.Equal(1, response.Version);
        Assert.Equal(CompanyDecisionProfileStatus.Active, response.Status);
        _auditService.Verify(
            audit => audit.RecordSafeAsync(
                It.Is<AuditEventWriteRequest>(request =>
                    request.EntityType == AuditEntityTypes.CompanyDecisionProfile
                    && request.EventType == AuditEventTypes.Created),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CreatesNewVersionAndDeactivatesPrevious_BR1905()
    {
        var current = CreateProfile();

        _repository
            .Setup(repository => repository.GetByIdAsync(current.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(current);
        _repository
            .Setup(repository => repository.GetLatestByFamilyAsync(current.ProfileFamilyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(current);
        _repository
            .Setup(repository => repository.UpdateAsync(current, It.IsAny<CancellationToken>()))
            .ReturnsAsync(current);

        CompanyDecisionProfile? newVersion = null;
        _repository
            .Setup(repository => repository.AddAsync(It.IsAny<CompanyDecisionProfile>(), It.IsAny<CancellationToken>()))
            .Callback<CompanyDecisionProfile, CancellationToken>((profile, _) => newVersion = profile)
            .ReturnsAsync((CompanyDecisionProfile profile, CancellationToken _) => profile);

        var response = await CreateService().UpdateAsync(current.Id, CreateValidUpdateRequest());

        Assert.NotNull(newVersion);
        Assert.NotEqual(current.Id, response.Id);
        Assert.Equal(2, response.Version);
        Assert.Equal(CompanyDecisionProfileStatus.Inactive, current.Status);
        _repository.Verify(
            repository => repository.UpdateAsync(current, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsWhenNotLatestVersion_BR1905()
    {
        var current = CreateProfile();
        var latest = current.CreateNewVersion(
            "Latest",
            null,
            ValidWeightsJson(),
            0.2m,
            0.2m,
            0.2m,
            0.2m,
            0.2m,
            null,
            null,
            null,
            DateTimeOffset.UtcNow);

        _repository
            .Setup(repository => repository.GetByIdAsync(current.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(current);
        _repository
            .Setup(repository => repository.GetLatestByFamilyAsync(current.ProfileFamilyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(latest);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService().UpdateAsync(current.Id, CreateValidUpdateRequest()));
    }

    [Fact]
    public async Task CloneAsync_CreatesNewFamilyAndRecordsAudit()
    {
        var source = CreateProfile();
        _repository
            .Setup(repository => repository.GetByIdAsync(source.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(source);
        _repository
            .Setup(repository => repository.ExistsCodeAsync(
                source.CompanyId, "ClonedCode", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository
            .Setup(repository => repository.ExistsActiveNameAsync(
                source.CompanyId, "Cloned Name", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository
            .Setup(repository => repository.AddAsync(It.IsAny<CompanyDecisionProfile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CompanyDecisionProfile profile, CancellationToken _) => profile);

        var response = await CreateService().CloneAsync(
            source.Id,
            new CloneCompanyDecisionProfileRequest { Name = "Cloned Name", Code = "ClonedCode" });

        Assert.Equal(1, response.Version);
        Assert.False(response.DefaultProfile);
        Assert.NotEqual(source.ProfileFamilyId, response.ProfileFamilyId);
        _auditService.Verify(
            audit => audit.RecordSafeAsync(
                It.Is<AuditEventWriteRequest>(request => request.Action == "CompanyDecisionProfile.Clone"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetDefaultAsync_ThrowsConflictWhenAnotherDefaultExists_BR1901()
    {
        var profile = CreateProfile();
        var otherDefault = CreateProfile(defaultProfile: true);

        _repository
            .Setup(repository => repository.GetByIdAsync(profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _repository
            .Setup(repository => repository.GetDefaultActiveAsync(profile.CompanyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherDefault);

        await Assert.ThrowsAsync<ConflictException>(() => CreateService().SetDefaultAsync(profile.Id));
    }

    [Fact]
    public async Task SetDefaultAsync_SetsDefaultAndRecordsAudit()
    {
        var profile = CreateProfile();

        _repository
            .Setup(repository => repository.GetByIdAsync(profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _repository
            .Setup(repository => repository.GetDefaultActiveAsync(profile.CompanyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CompanyDecisionProfile?)null);
        _repository
            .Setup(repository => repository.UpdateAsync(profile, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var response = await CreateService().SetDefaultAsync(profile.Id);

        Assert.True(response.DefaultProfile);
        _auditService.Verify(
            audit => audit.RecordSafeAsync(It.IsAny<AuditEventWriteRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetDefaultAsync_SyncsCompanyDecisionProfileId_BR2007()
    {
        var profile = CreateProfile();
        var company = Company.Create(
            profile.CompanyId,
            "SYNC",
            "Sync Company",
            null,
            "UTC",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            DateTimeOffset.UtcNow);

        _repository
            .Setup(repository => repository.GetByIdAsync(profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _repository
            .Setup(repository => repository.GetDefaultActiveAsync(profile.CompanyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CompanyDecisionProfile?)null);
        _repository
            .Setup(repository => repository.UpdateAsync(profile, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _companyRepository
            .Setup(repository => repository.GetByIdAsync(profile.CompanyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        await CreateService().SetDefaultAsync(profile.Id);

        Assert.Equal(profile.Id, company.DecisionProfileId);
        _companyRepository.Verify(
            repository => repository.UpdateAsync(company, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_ThrowsBusinessRuleWhenDefault()
    {
        var profile = CreateProfile(defaultProfile: true);

        _repository
            .Setup(repository => repository.GetByIdAsync(profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().DeactivateAsync(profile.Id));
    }

    [Fact]
    public async Task ArchiveAsync_ThrowsBusinessRuleWhenDefault()
    {
        var profile = CreateProfile(defaultProfile: true);

        _repository
            .Setup(repository => repository.GetByIdAsync(profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().ArchiveAsync(profile.Id));
    }

    [Fact]
    public async Task ActivateAsync_RecordsAudit()
    {
        var profile = CreateProfile();
        profile.Deactivate(DateTimeOffset.UtcNow);

        _repository
            .Setup(repository => repository.GetByIdAsync(profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _repository
            .Setup(repository => repository.UpdateAsync(profile, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        await CreateService().ActivateAsync(profile.Id);

        Assert.True(profile.IsActive);
        _auditService.Verify(
            audit => audit.RecordSafeAsync(
                It.Is<AuditEventWriteRequest>(request => request.EventType == AuditEventTypes.Activated),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static CompanyDecisionProfile CreateProfile(bool defaultProfile = false) =>
        CompanyDecisionProfile.Create(
            CompanyId,
            "TestCode" + Guid.NewGuid().ToString("N")[..6],
            "Test Profile",
            "Description",
            ValidWeightsJson(),
            0.2m,
            0.2m,
            0.2m,
            0.2m,
            0.2m,
            null,
            null,
            null,
            defaultProfile,
            DateTimeOffset.UtcNow);

    private static string ValidWeightsJson() =>
        CompanyDecisionProfile.SerializeDimensions(
        [
            new DecisionProfileDimensionSetting
            {
                Dimension = RankingDimension.EstimatedCost,
                Weight = 0.5m,
                PreferHigherValues = false
            },
            new DecisionProfileDimensionSetting
            {
                Dimension = RankingDimension.OperationalRisk,
                Weight = 0.5m,
                PreferHigherValues = false
            }
        ]);

    private static CreateCompanyDecisionProfileRequest CreateValidRequest(
        string code = "TestCode",
        string name = "Test Profile",
        bool defaultProfile = false) =>
        new()
        {
            CompanyId = CompanyId,
            Code = code,
            Name = name,
            PriorityWeights =
            [
                new DecisionProfileDimensionRequest { Dimension = RankingDimension.EstimatedCost, Weight = 0.5m },
                new DecisionProfileDimensionRequest { Dimension = RankingDimension.OperationalRisk, Weight = 0.5m }
            ],
            CapacityWeight = 0.2m,
            WorkloadWeight = 0.2m,
            CostWeight = 0.2m,
            RiskWeight = 0.2m,
            QualityWeight = 0.2m,
            DefaultProfile = defaultProfile
        };

    private static UpdateCompanyDecisionProfileRequest CreateValidUpdateRequest() =>
        new()
        {
            Name = "Updated Name",
            PriorityWeights =
            [
                new DecisionProfileDimensionRequest { Dimension = RankingDimension.EstimatedCost, Weight = 0.5m },
                new DecisionProfileDimensionRequest { Dimension = RankingDimension.OperationalRisk, Weight = 0.5m }
            ],
            CapacityWeight = 0.2m,
            WorkloadWeight = 0.2m,
            CostWeight = 0.2m,
            RiskWeight = 0.2m,
            QualityWeight = 0.2m
        };
}
