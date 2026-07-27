using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class CompanyServiceTests
{
    private readonly Mock<ICompanyRepository> _repository = new();
    private readonly Mock<ICompanyDecisionProfileRepository> _decisionProfileRepository = new();
    private readonly Mock<IPlanningTemplateRepository> _planningTemplateRepository = new();
    private readonly Mock<IAuditService> _auditService = new();
    private readonly Mock<ILogger<CompanyService>> _logger = new();

    private CompanyService CreateService() =>
        new(
            _repository.Object,
            _decisionProfileRepository.Object,
            _planningTemplateRepository.Object,
            _auditService.Object,
            _logger.Object);

    [Fact]
    public async Task CreateAsync_ThrowsConflictWhenCodeAlreadyExists_BR2002()
    {
        _repository
            .Setup(repository => repository.ExistsCodeAsync("DupCode", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateService().CreateAsync(CreateValidRequest(code: "DupCode")));
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflictWhenNameAlreadyExists_BR2001()
    {
        _repository
            .Setup(repository => repository.ExistsCodeAsync(
                It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository
            .Setup(repository => repository.ExistsNameAsync(
                "Duplicate Name", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateService().CreateAsync(CreateValidRequest(name: "Duplicate Name")));
    }

    [Fact]
    public async Task CreateAsync_PersistsAndRecordsAudit()
    {
        Company? persisted = null;
        _repository
            .Setup(repository => repository.ExistsCodeAsync(
                It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository
            .Setup(repository => repository.ExistsNameAsync(
                It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository
            .Setup(repository => repository.AddAsync(It.IsAny<Company>(), It.IsAny<CancellationToken>()))
            .Callback<Company, CancellationToken>((company, _) => persisted = company)
            .ReturnsAsync((Company company, CancellationToken _) => company);

        var response = await CreateService().CreateAsync(CreateValidRequest());

        Assert.NotNull(persisted);
        Assert.Equal(CompanyStatus.Active, response.Status);
        _auditService.Verify(
            audit => audit.RecordSafeAsync(
                It.Is<AuditEventWriteRequest>(request =>
                    request.EntityType == AuditEntityTypes.Company
                    && request.EventType == AuditEventTypes.Created),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenDecisionProfileBelongsToAnotherCompany_BR2007()
    {
        var otherCompanyId = Guid.NewGuid();
        var profile = CreateProfile(otherCompanyId, defaultProfile: true);

        _repository
            .Setup(repository => repository.ExistsCodeAsync(
                It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository
            .Setup(repository => repository.ExistsNameAsync(
                It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _decisionProfileRepository
            .Setup(repository => repository.GetByIdAsync(profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var request = CreateValidRequest();
        request.DecisionProfileId = profile.Id;

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenPlanningTemplateBelongsToAnotherCompany_BR2008()
    {
        var otherCompanyId = Guid.NewGuid();
        var template = CreatePlanningTemplate(otherCompanyId);

        _repository
            .Setup(repository => repository.ExistsCodeAsync(
                It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository
            .Setup(repository => repository.ExistsNameAsync(
                It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _planningTemplateRepository
            .Setup(repository => repository.GetByIdAsync(template.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var request = CreateValidRequest();
        request.DefaultPlanningTemplateId = template.Id;

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().CreateAsync(request));
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundWhenMissing()
    {
        var companyId = Guid.NewGuid();
        _repository
            .Setup(repository => repository.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().UpdateAsync(companyId, CreateValidUpdateRequest()));
    }

    [Fact]
    public async Task UpdateAsync_ThrowsConflictWhenNameChangedToExisting_BR2001()
    {
        var company = CreateCompany(code: "CODE1", name: "Original Name");
        _repository
            .Setup(repository => repository.GetByIdAsync(company.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);
        _repository
            .Setup(repository => repository.ExistsNameAsync("New Name", company.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = CreateValidUpdateRequest();
        request.CompanyName = "New Name";

        await Assert.ThrowsAsync<ConflictException>(() => CreateService().UpdateAsync(company.Id, request));
    }

    [Fact]
    public async Task UpdateAsync_AcceptsAssignableDecisionProfileAndPlanningTemplate_BR2007_BR2008()
    {
        var company = CreateCompany(code: "CODE2", name: "Company Two");
        var profile = CreateProfile(company.Id, defaultProfile: true);
        var template = CreatePlanningTemplate(company.Id);

        _repository
            .Setup(repository => repository.GetByIdAsync(company.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);
        _decisionProfileRepository
            .Setup(repository => repository.GetByIdAsync(profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _planningTemplateRepository
            .Setup(repository => repository.GetByIdAsync(template.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _repository
            .Setup(repository => repository.UpdateAsync(company, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        var request = CreateValidUpdateRequest();
        request.DecisionProfileId = profile.Id;
        request.DefaultPlanningTemplateId = template.Id;

        var response = await CreateService().UpdateAsync(company.Id, request);

        Assert.Equal(profile.Id, response.DecisionProfileId);
        Assert.Equal(template.Id, response.DefaultPlanningTemplateId);
    }

    [Fact]
    public async Task ActivateAsync_ActivatesAndRecordsAudit_BR2003()
    {
        var company = CreateCompany(code: "ActCode", name: "Activate Co");
        company.Deactivate(DateTimeOffset.UtcNow);

        _repository
            .Setup(repository => repository.GetByIdAsync(company.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);
        _repository
            .Setup(repository => repository.UpdateAsync(company, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        await CreateService().ActivateAsync(company.Id);

        Assert.True(company.IsActive);
        _auditService.Verify(
            audit => audit.RecordSafeAsync(
                It.Is<AuditEventWriteRequest>(request => request.EventType == AuditEventTypes.Activated),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_DeactivatesAndRecordsAudit()
    {
        var company = CreateCompany(code: "DeactCode", name: "Deactivate Co");

        _repository
            .Setup(repository => repository.GetByIdAsync(company.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);
        _repository
            .Setup(repository => repository.UpdateAsync(company, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        await CreateService().DeactivateAsync(company.Id);

        Assert.True(company.IsInactive);
    }

    [Fact]
    public async Task ArchiveAsync_ArchivesAndRecordsAudit_BR2009()
    {
        var company = CreateCompany(code: "ArcCode", name: "Archive Co");

        _repository
            .Setup(repository => repository.GetByIdAsync(company.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);
        _repository
            .Setup(repository => repository.UpdateAsync(company, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        await CreateService().ArchiveAsync(company.Id);

        Assert.True(company.Archived);
        _auditService.Verify(
            audit => audit.RecordSafeAsync(
                It.Is<AuditEventWriteRequest>(request => request.EventType == AuditEventTypes.Archived),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_IncludesArchivedWhenRequested_BR2009()
    {
        var archived = CreateCompany(code: "ArcQuery", name: "Archived Query Co");
        archived.Archive(DateTimeOffset.UtcNow);

        _repository
            .Setup(repository => repository.GetAllAsync(
                It.Is<CompanyQueryParameters>(parameters => parameters.IncludeArchived),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([archived]);

        var results = await CreateService().GetAllAsync(new CompanyQueryParameters { IncludeArchived = true });

        Assert.Single(results);
        Assert.Equal(CompanyStatus.Archived, results[0].Status);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundWhenMissing()
    {
        var companyId = Guid.NewGuid();
        _repository
            .Setup(repository => repository.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CreateService().GetByIdAsync(companyId));
    }

    private static Company CreateCompany(string code, string name) =>
        Company.Create(
            Guid.NewGuid(),
            code,
            name,
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

    private static CreateCompanyRequest CreateValidRequest(
        string code = "ACME",
        string name = "Acme Agency") =>
        new()
        {
            CompanyCode = code,
            CompanyName = name,
            Timezone = "UTC"
        };

    private static UpdateCompanyRequest CreateValidUpdateRequest() =>
        new()
        {
            CompanyName = "Updated Name",
            Timezone = "UTC"
        };

    private static CompanyDecisionProfile CreateProfile(Guid companyId, bool defaultProfile) =>
        CompanyDecisionProfile.Create(
            companyId,
            "ProfileCode" + Guid.NewGuid().ToString("N")[..6],
            "Profile Name",
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

    private static PlanningTemplate CreatePlanningTemplate(Guid companyId) =>
        PlanningTemplate.Create(
            companyId,
            "Template" + Guid.NewGuid().ToString("N")[..6],
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            ResourceAvailabilityStrategies.RequireActiveConfiguration,
            7,
            0,
            PlanningTemplateCapacityRules.Default,
            null,
            DateTimeOffset.UtcNow);
}
