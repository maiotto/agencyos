using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Moq;

namespace AgencyOS.Application.Tests;

public class CrossPortfolioPlanningServiceTests
{
    private readonly Mock<IPortfolioRepository> _portfolioRepository = new();
    private readonly Mock<ICompanyRepository> _companyRepository = new();
    private readonly Mock<ICompanyContext> _companyContext = new();
    private readonly Mock<IEnterpriseCapacityService> _enterpriseCapacityService = new();
    private readonly Mock<IEnterpriseWorkloadService> _enterpriseWorkloadService = new();
    private readonly Mock<ICrossPortfolioConflictDetectionService> _conflictDetectionService = new();
    private readonly Mock<ICrossPortfolioBalancingService> _balancingService = new();
    private readonly Mock<ICrossPortfolioScenarioComparisonService> _scenarioComparisonService = new();
    private readonly Mock<ICrossPortfolioScenarioStore> _scenarioStore = new();
    private readonly Mock<IAuditService> _auditService = new();

    public CrossPortfolioPlanningServiceTests()
    {
        _companyContext.SetupProperty(context => context.CompanyId);
        _companyRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCompany());

        _portfolioRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<PortfolioQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Portfolio>());

        _enterpriseCapacityService
            .Setup(service => service.BuildAsync(
                It.IsAny<IReadOnlyList<Portfolio>>(), It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseCapacityResponse());
        _enterpriseWorkloadService
            .Setup(service => service.BuildAsync(
                It.IsAny<IReadOnlyList<Portfolio>>(), It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseWorkloadResponse());
        _conflictDetectionService
            .Setup(service => service.DetectAsync(
                It.IsAny<IReadOnlyList<Portfolio>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConflictSummaryResponse());
        _balancingService
            .Setup(service => service.BuildRecommendations(It.IsAny<IReadOnlyList<PortfolioParticipationResponse>>()))
            .Returns(Array.Empty<BalancingRecommendationResponse>());
    }

    private CrossPortfolioPlanningService CreateService() =>
        new(
            _portfolioRepository.Object,
            _companyRepository.Object,
            _companyContext.Object,
            new DashboardHealthCalculationService(),
            _enterpriseCapacityService.Object,
            _enterpriseWorkloadService.Object,
            _conflictDetectionService.Object,
            _balancingService.Object,
            _scenarioComparisonService.Object,
            _scenarioStore.Object,
            _auditService.Object);

    [Fact]
    public async Task GetOverviewAsync_ResolvesCompanyId_FromParametersFirst()
    {
        var explicitCompanyId = Guid.NewGuid();
        _companyContext.Object.CompanyId = Guid.NewGuid();

        var service = CreateService();

        var overview = await service.GetOverviewAsync(
            new CrossPortfolioPlanningQueryParameters { CompanyId = explicitCompanyId });

        Assert.Equal(explicitCompanyId, overview.CompanyId);
    }

    [Fact]
    public async Task GetOverviewAsync_ResolvesCompanyId_FromContext_WhenParametersUnset()
    {
        var contextCompanyId = Guid.NewGuid();
        _companyContext.Object.CompanyId = contextCompanyId;

        var service = CreateService();

        var overview = await service.GetOverviewAsync(new CrossPortfolioPlanningQueryParameters());

        Assert.Equal(contextCompanyId, overview.CompanyId);
    }

    [Fact]
    public async Task GetOverviewAsync_ResolvesCompanyId_FromDefault_WhenNothingSet()
    {
        var service = CreateService();

        var overview = await service.GetOverviewAsync(new CrossPortfolioPlanningQueryParameters());

        Assert.Equal(AgencyOSCompanies.DefaultCompanyId, overview.CompanyId);
    }

    [Fact]
    public async Task GetOverviewAsync_ThrowsNotFound_WhenCompanyMissing()
    {
        _companyRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetOverviewAsync(new CrossPortfolioPlanningQueryParameters()));
    }

    [Fact]
    public async Task GetOverviewAsync_ReturnsParticipationPerActivePortfolio_WithPrioritiesPreserved()
    {
        var missionOne = Guid.NewGuid();
        var missionTwo = Guid.NewGuid();
        var portfolio = CreatePortfolio([(missionTwo, 2), (missionOne, 1)], utilization: 55m, workload: 45m);

        _portfolioRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<PortfolioQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([portfolio]);

        var service = CreateService();

        var overview = await service.GetOverviewAsync(new CrossPortfolioPlanningQueryParameters());

        Assert.Equal(1, overview.PortfolioCount);
        var participation = Assert.Single(overview.Portfolios);
        Assert.Equal(portfolio.Id, participation.PortfolioId);
        Assert.Equal(55m, participation.UtilizationPercentage);
        Assert.Equal(2, participation.Missions.Count);
        Assert.Equal(missionOne, participation.Missions[0].MissionId);
        Assert.Equal(1, participation.Missions[0].Priority);
        Assert.Equal(missionTwo, participation.Missions[1].MissionId);
        Assert.True(overview.RequiresHumanApproval);
    }

    [Fact]
    public async Task GetScenariosAsync_ReturnsScenarios_FromStore_FilteredByCompany()
    {
        var companyId = Guid.NewGuid();
        var record = CreateScenarioRecord(companyId);
        _scenarioStore
            .Setup(store => store.GetAll(companyId))
            .Returns([record]);

        var service = CreateService();

        var result = await service.GetScenariosAsync(new CrossPortfolioPlanningQueryParameters { CompanyId = companyId });

        var scenario = Assert.Single(result.Scenarios);
        Assert.Equal(record.ScenarioId, scenario.ScenarioId);
    }

    [Fact]
    public async Task GetConflictsAsync_ThrowsNotFound_WhenPortfolioMissing()
    {
        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Portfolio?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetConflictsAsync(
            new CrossPortfolioSelectionQueryParameters { PortfolioIds = [Guid.NewGuid()] }));
    }

    [Fact]
    public async Task GetConflictsAsync_ThrowsBusinessRule_WhenPortfolioBelongsToDifferentCompany()
    {
        var portfolio = CreatePortfolio([(Guid.NewGuid(), 1)], companyId: Guid.NewGuid());
        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(portfolio.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(portfolio);

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.GetConflictsAsync(
            new CrossPortfolioSelectionQueryParameters
            {
                CompanyId = AgencyOSCompanies.DefaultCompanyId,
                PortfolioIds = [portfolio.Id]
            }));
    }

    [Fact]
    public async Task GetConflictsAsync_DelegatesToConflictDetectionService()
    {
        var portfolio = CreatePortfolio([(Guid.NewGuid(), 1)]);
        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(portfolio.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(portfolio);

        var expected = new ConflictSummaryResponse { PortfolioConflictCount = 3 };
        _conflictDetectionService
            .Setup(service => service.DetectAsync(
                It.IsAny<IReadOnlyList<Portfolio>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var service = CreateService();

        var result = await service.GetConflictsAsync(
            new CrossPortfolioSelectionQueryParameters { PortfolioIds = [portfolio.Id] });

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetBalanceAsync_ReturnsEnterpriseCapacityWorkloadAndRecommendations()
    {
        var portfolio = CreatePortfolio([(Guid.NewGuid(), 1)]);
        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(portfolio.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(portfolio);

        var capacity = new EnterpriseCapacityResponse { PortfolioCount = 1 };
        var workload = new EnterpriseWorkloadResponse { PortfolioCount = 1 };
        var recommendations = new List<BalancingRecommendationResponse>
        {
            new() { Category = "Balanced", Recommendation = "All good" }
        };

        _enterpriseCapacityService
            .Setup(service => service.BuildAsync(
                It.IsAny<IReadOnlyList<Portfolio>>(), It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(capacity);
        _enterpriseWorkloadService
            .Setup(service => service.BuildAsync(
                It.IsAny<IReadOnlyList<Portfolio>>(), It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workload);
        _balancingService
            .Setup(service => service.BuildRecommendations(It.IsAny<IReadOnlyList<PortfolioParticipationResponse>>()))
            .Returns(recommendations);

        var service = CreateService();

        var result = await service.GetBalanceAsync(
            new CrossPortfolioSelectionQueryParameters { PortfolioIds = [portfolio.Id] });

        Assert.Same(capacity, result.Capacity);
        Assert.Same(workload, result.Workload);
        Assert.Same(recommendations, result.Recommendations);
        Assert.Single(result.Portfolios);
        Assert.True(result.RequiresHumanApproval);
    }

    [Fact]
    public async Task SimulateAsync_ThrowsBusinessRule_WhenFewerThanTwoDistinctPortfolios()
    {
        var portfolioId = Guid.NewGuid();
        var service = CreateService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.SimulateAsync(
            new SimulateCrossPortfolioPlanRequest { PortfolioIds = [portfolioId, portfolioId] }));
    }

    [Fact]
    public async Task SimulateAsync_ThrowsNotFound_WhenAPortfolioDoesNotExist()
    {
        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Portfolio?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.SimulateAsync(
            new SimulateCrossPortfolioPlanRequest { PortfolioIds = [Guid.NewGuid(), Guid.NewGuid()] }));
    }

    [Fact]
    public async Task SimulateAsync_ThrowsBusinessRule_WhenPortfolioBelongsToDifferentCompany()
    {
        var portfolioOne = CreatePortfolio([(Guid.NewGuid(), 1)]);
        var portfolioTwo = CreatePortfolio([(Guid.NewGuid(), 1)], companyId: Guid.NewGuid());
        SetupPortfolioLookup(portfolioOne, portfolioTwo);

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.SimulateAsync(
            new SimulateCrossPortfolioPlanRequest
            {
                CompanyId = AgencyOSCompanies.DefaultCompanyId,
                PortfolioIds = [portfolioOne.Id, portfolioTwo.Id]
            }));
    }

    [Fact]
    public async Task SimulateAsync_ResolvesCompany_FromRequestThenContextThenDefault()
    {
        var contextCompanyId = Guid.NewGuid();
        var portfolioOne = CreatePortfolio([(Guid.NewGuid(), 1)], companyId: contextCompanyId);
        var portfolioTwo = CreatePortfolio([(Guid.NewGuid(), 1)], companyId: contextCompanyId);
        SetupPortfolioLookup(portfolioOne, portfolioTwo);

        _companyContext.Object.CompanyId = contextCompanyId;

        var service = CreateService();

        var result = await service.SimulateAsync(
            new SimulateCrossPortfolioPlanRequest { PortfolioIds = [portfolioOne.Id, portfolioTwo.Id] });

        Assert.Equal(contextCompanyId, result.CompanyId);
    }

    [Fact]
    public async Task SimulateAsync_StoresScenario_InScenarioStore()
    {
        var portfolioOne = CreatePortfolio([(Guid.NewGuid(), 1)]);
        var portfolioTwo = CreatePortfolio([(Guid.NewGuid(), 1)]);
        SetupPortfolioLookup(portfolioOne, portfolioTwo);

        CrossPortfolioScenarioRecord? stored = null;
        _scenarioStore
            .Setup(store => store.Add(It.IsAny<CrossPortfolioScenarioRecord>()))
            .Callback<CrossPortfolioScenarioRecord>(record => stored = record);

        var service = CreateService();

        var result = await service.SimulateAsync(
            new SimulateCrossPortfolioPlanRequest { PortfolioIds = [portfolioOne.Id, portfolioTwo.Id] });

        Assert.NotNull(stored);
        Assert.Equal(result.ScenarioId, stored!.ScenarioId);
        _scenarioStore.Verify(store => store.Add(It.IsAny<CrossPortfolioScenarioRecord>()), Times.Once);
    }

    [Fact]
    public async Task SimulateAsync_RecordsAuditEvent_WithCrossPortfolioPlanEntityType()
    {
        var portfolioOne = CreatePortfolio([(Guid.NewGuid(), 1)]);
        var portfolioTwo = CreatePortfolio([(Guid.NewGuid(), 1)]);
        SetupPortfolioLookup(portfolioOne, portfolioTwo);

        AuditEventWriteRequest? captured = null;
        _auditService
            .Setup(service => service.RecordSafeAsync(It.IsAny<AuditEventWriteRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEventWriteRequest, CancellationToken>((request, _) => captured = request)
            .Returns(Task.CompletedTask);

        var service = CreateService();

        var result = await service.SimulateAsync(
            new SimulateCrossPortfolioPlanRequest { PortfolioIds = [portfolioOne.Id, portfolioTwo.Id] });

        _auditService.Verify(
            service => service.RecordSafeAsync(It.IsAny<AuditEventWriteRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.NotNull(captured);
        Assert.Equal(AuditEntityTypes.CrossPortfolioPlan, captured!.EntityType);
        Assert.Equal(result.ScenarioId, captured.EntityId);
        Assert.Equal(AuditEventTypes.Simulated, captured.EventType);
        Assert.Equal("CrossPortfolioPlanning.Simulate", captured.Action);
        Assert.Contains("requiresHumanApproval", captured.Metadata);
    }

    [Fact]
    public async Task SimulateAsync_ReturnsRequiresHumanApproval_AndAdvisoryOnly()
    {
        var portfolioOne = CreatePortfolio([(Guid.NewGuid(), 1)]);
        var portfolioTwo = CreatePortfolio([(Guid.NewGuid(), 1)]);
        SetupPortfolioLookup(portfolioOne, portfolioTwo);

        var service = CreateService();

        var result = await service.SimulateAsync(
            new SimulateCrossPortfolioPlanRequest { PortfolioIds = [portfolioOne.Id, portfolioTwo.Id] });

        Assert.True(result.RequiresHumanApproval);
        Assert.True(result.AdvisoryOnly);
        Assert.True(result.Scenario.RequiresHumanApproval);
        Assert.True(result.Scenario.AdvisoryOnly);
        Assert.False(string.IsNullOrWhiteSpace(result.Scenario.AdvisoryDisclaimer));
    }

    [Fact]
    public async Task SimulateAsync_NeverCallsPortfolioRepositoryWriteMethods()
    {
        var portfolioOne = CreatePortfolio([(Guid.NewGuid(), 1)]);
        var portfolioTwo = CreatePortfolio([(Guid.NewGuid(), 1)]);
        SetupPortfolioLookup(portfolioOne, portfolioTwo);

        var service = CreateService();

        await service.SimulateAsync(
            new SimulateCrossPortfolioPlanRequest { PortfolioIds = [portfolioOne.Id, portfolioTwo.Id] });

        _portfolioRepository.Verify(
            repository => repository.UpdateAsync(It.IsAny<Portfolio>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _portfolioRepository.Verify(
            repository => repository.AddAsync(It.IsAny<Portfolio>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _portfolioRepository.Verify(
            repository => repository.DeleteAsync(It.IsAny<Portfolio>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SimulateAsync_DeduplicatesRepeatedPortfolioIds()
    {
        var portfolioOne = CreatePortfolio([(Guid.NewGuid(), 1)]);
        var portfolioTwo = CreatePortfolio([(Guid.NewGuid(), 1)]);
        SetupPortfolioLookup(portfolioOne, portfolioTwo);

        var service = CreateService();

        var result = await service.SimulateAsync(
            new SimulateCrossPortfolioPlanRequest
            {
                PortfolioIds = [portfolioOne.Id, portfolioOne.Id, portfolioTwo.Id]
            });

        Assert.Equal(2, result.Scenario.PortfolioIds.Count);
    }

    [Fact]
    public async Task CompareAsync_ThrowsNotFound_WhenLeftScenarioMissing()
    {
        _scenarioStore.Setup(store => store.Get(It.IsAny<Guid>())).Returns((CrossPortfolioScenarioRecord?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.CompareAsync(
            new CompareCrossPortfolioScenariosRequest { LeftScenarioId = Guid.NewGuid(), RightScenarioId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task CompareAsync_ThrowsBusinessRule_WhenScenarioBelongsToDifferentCompany()
    {
        var companyId = AgencyOSCompanies.DefaultCompanyId;
        var leftRecord = CreateScenarioRecord(companyId);
        var rightRecord = CreateScenarioRecord(Guid.NewGuid());

        _scenarioStore.Setup(store => store.Get(leftRecord.ScenarioId)).Returns(leftRecord);
        _scenarioStore.Setup(store => store.Get(rightRecord.ScenarioId)).Returns(rightRecord);

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CompareAsync(
            new CompareCrossPortfolioScenariosRequest
            {
                CompanyId = companyId,
                LeftScenarioId = leftRecord.ScenarioId,
                RightScenarioId = rightRecord.ScenarioId
            }));
    }

    [Fact]
    public async Task CompareAsync_DelegatesToScenarioComparisonService()
    {
        var companyId = AgencyOSCompanies.DefaultCompanyId;
        var leftRecord = CreateScenarioRecord(companyId);
        var rightRecord = CreateScenarioRecord(companyId);

        _scenarioStore.Setup(store => store.Get(leftRecord.ScenarioId)).Returns(leftRecord);
        _scenarioStore.Setup(store => store.Get(rightRecord.ScenarioId)).Returns(rightRecord);

        var expected = new ScenarioComparisonResponse();
        _scenarioComparisonService
            .Setup(service => service.Compare(leftRecord, rightRecord))
            .Returns(expected);

        var service = CreateService();

        var result = await service.CompareAsync(new CompareCrossPortfolioScenariosRequest
        {
            CompanyId = companyId,
            LeftScenarioId = leftRecord.ScenarioId,
            RightScenarioId = rightRecord.ScenarioId
        });

        Assert.Same(expected, result);
    }

    private void SetupPortfolioLookup(params Portfolio[] portfolios)
    {
        foreach (var portfolio in portfolios)
        {
            _portfolioRepository
                .Setup(repository => repository.GetByIdAsync(portfolio.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(portfolio);
        }
    }

    private static Company CreateCompany() =>
        Company.Create(
            AgencyOSCompanies.DefaultCompanyId,
            "AOS",
            "AgencyOS Default",
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

    private static Portfolio CreatePortfolio(
        IReadOnlyList<(Guid MissionId, int Priority)> missions,
        decimal utilization = 50m,
        decimal workload = 50m,
        Guid? companyId = null)
    {
        var portfolio = Portfolio.Create(
            companyId ?? AgencyOSCompanies.DefaultCompanyId,
            $"Portfolio {Guid.NewGuid():N}",
            null,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            null,
            missions,
            DateTimeOffset.UtcNow);

        portfolio.CalculateCapacity(
            $"{{\"overallUtilizationPercentage\":{utilization.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}",
            DateTimeOffset.UtcNow);
        portfolio.CalculateWorkload(
            $"{{\"overallWorkloadPercentage\":{workload.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}",
            DateTimeOffset.UtcNow);
        portfolio.CalculateHealth(utilization, workload, null, "{}", DateTimeOffset.UtcNow);

        return portfolio;
    }

    private static CrossPortfolioScenarioRecord CreateScenarioRecord(Guid companyId)
    {
        var scenarioId = Guid.NewGuid();
        return new CrossPortfolioScenarioRecord
        {
            ScenarioId = scenarioId,
            CompanyId = companyId,
            CreatedAt = DateTimeOffset.UtcNow,
            Scenario = new CrossPortfolioScenarioResponse
            {
                ScenarioId = scenarioId,
                CompanyId = companyId,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };
    }
}
