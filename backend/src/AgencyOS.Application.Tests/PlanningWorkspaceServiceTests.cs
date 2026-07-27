using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Moq;

namespace AgencyOS.Application.Tests;

public class PlanningWorkspaceServiceTests
{
    private readonly Mock<IPlanningOverviewService> _overviewService = new();
    private readonly Mock<IPlanningNavigationService> _navigationService = new();
    private readonly Mock<IPlanningHistoryService> _historyService = new();
    private readonly Mock<IPlanningTemplateService> _templateService = new();
    private readonly Mock<IPortfolioService> _portfolioService = new();
    private readonly Mock<ICapacityHistoryService> _capacityHistoryService = new();
    private readonly Mock<IWorkloadHistoryService> _workloadHistoryService = new();
    private readonly Mock<ICrossPortfolioPlanningService> _crossPortfolioPlanningService = new();
    private readonly Mock<IDashboardHealthCalculationService> _healthCalculationService = new();
    private readonly Mock<ICompanyRepository> _companyRepository = new();
    private readonly Mock<ICompanyContext> _companyContext = new();

    public PlanningWorkspaceServiceTests()
    {
        _companyContext.SetupProperty(context => context.CompanyId);
        _companyRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => CreateCompany(id));

        _overviewService
            .Setup(service => service.GetOverviewAsync(
                It.IsAny<Guid>(),
                It.IsAny<PlanningWorkspaceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, PlanningWorkspaceQueryParameters _, CancellationToken __) =>
                new PlanningOverviewResponse
                {
                    CompanyId = companyId,
                    Kpis = new PlanningKpiSummaryResponse()
                });

        _navigationService
            .Setup(service => service.GetNavigation(It.IsAny<Guid>()))
            .Returns((Guid companyId) => new PlanningNavigationResponse { CompanyId = companyId, Actions = [] });

        _historyService
            .Setup(service => service.GetHistoryAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken _) =>
                new PlanningHistoryResponse { CompanyId = companyId, From = from, To = to, Items = [] });

        _templateService
            .Setup(service => service.GetAllAsync(It.IsAny<PlanningTemplateQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlanningTemplateResponse>());

        _portfolioService
            .Setup(service => service.GetAllAsync(It.IsAny<PortfolioQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PortfolioResponse>());

        _capacityHistoryService
            .Setup(service => service.AggregateAsync(It.IsAny<CapacityHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CapacityHistoryAggregateResponse());
        _capacityHistoryService
            .Setup(service => service.QueryAsync(It.IsAny<CapacityHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CapacityHistoryResponse>());

        _workloadHistoryService
            .Setup(service => service.AggregateAsync(It.IsAny<WorkloadHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkloadHistoryAggregateResponse());
        _workloadHistoryService
            .Setup(service => service.QueryAsync(It.IsAny<WorkloadHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WorkloadHistoryResponse>());

        _crossPortfolioPlanningService
            .Setup(service => service.GetScenariosAsync(
                It.IsAny<CrossPortfolioPlanningQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CrossPortfolioScenarioListResponse { Scenarios = [] });

        _healthCalculationService
            .Setup(service => service.CalculateFromUtilization(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal?>()))
            .Returns(new HealthIndicator { Status = PortfolioHealth.Healthy, Label = "Healthy" });
        _healthCalculationService
            .Setup(service => service.CalculateOverall(It.IsAny<IReadOnlyCollection<string>>()))
            .Returns(new HealthIndicator { Status = PortfolioHealth.Healthy, Label = "Healthy" });
    }

    private PlanningWorkspaceService CreateService() =>
        new(
            _overviewService.Object,
            _navigationService.Object,
            _historyService.Object,
            _templateService.Object,
            _portfolioService.Object,
            _capacityHistoryService.Object,
            _workloadHistoryService.Object,
            _crossPortfolioPlanningService.Object,
            _healthCalculationService.Object,
            _companyRepository.Object,
            _companyContext.Object);

    [Fact]
    public async Task GetWorkspaceAsync_ResolvesCompanyId_FromParametersFirst()
    {
        var explicitCompanyId = Guid.NewGuid();
        _companyContext.Object.CompanyId = Guid.NewGuid();

        var workspace = await CreateService().GetWorkspaceAsync(
            new PlanningWorkspaceQueryParameters { CompanyId = explicitCompanyId });

        Assert.Equal(explicitCompanyId, workspace.CompanyId);
        Assert.True(workspace.Scenarios.RequiresHumanApproval);
        Assert.True(workspace.Scenarios.AdvisoryOnly);
    }

    [Fact]
    public async Task GetWorkspaceAsync_ResolvesCompanyId_FromCompanyContext_WhenParametersUnset()
    {
        var contextCompanyId = Guid.NewGuid();
        _companyContext.Object.CompanyId = contextCompanyId;

        var workspace = await CreateService().GetWorkspaceAsync(new PlanningWorkspaceQueryParameters());

        Assert.Equal(contextCompanyId, workspace.CompanyId);
    }

    [Fact]
    public async Task GetWorkspaceAsync_FallsBackToDefaultCompany_WhenUnset()
    {
        _companyContext.Object.CompanyId = null;

        var workspace = await CreateService().GetWorkspaceAsync(new PlanningWorkspaceQueryParameters());

        Assert.Equal(AgencyOSCompanies.DefaultCompanyId, workspace.CompanyId);
    }

    [Fact]
    public async Task GetWorkspaceAsync_ThrowsNotFound_WhenCompanyMissing()
    {
        _companyRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().GetWorkspaceAsync(new PlanningWorkspaceQueryParameters { CompanyId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task GetScenariosAsync_MarksAdvisoryAndRequiresHumanApproval()
    {
        var scenarios = await CreateService().GetScenariosAsync(new PlanningWorkspaceQueryParameters());

        Assert.True(scenarios.RequiresHumanApproval);
        Assert.True(scenarios.AdvisoryOnly);
        Assert.False(string.IsNullOrWhiteSpace(scenarios.AdvisoryDisclaimer));
    }

    [Fact]
    public async Task GetTemplatesAsync_NeverCallsTemplateWriteApis()
    {
        await CreateService().GetTemplatesAsync(new PlanningWorkspaceQueryParameters());

        _templateService.Verify(
            service => service.GetAllAsync(It.IsAny<PlanningTemplateQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _templateService.VerifyNoOtherCalls();
    }

    private static Company CreateCompany(Guid? id = null) =>
        Company.Create(
            id ?? AgencyOSCompanies.DefaultCompanyId,
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
}
