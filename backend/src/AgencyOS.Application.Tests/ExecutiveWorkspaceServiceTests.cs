using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Moq;

namespace AgencyOS.Application.Tests;

public class ExecutiveWorkspaceServiceTests
{
    private readonly Mock<IExecutiveOverviewService> _overviewService = new();
    private readonly Mock<IExecutiveAggregationService> _aggregationService = new();
    private readonly Mock<IExecutiveNavigationService> _navigationService = new();
    private readonly Mock<ICompanyRepository> _companyRepository = new();
    private readonly Mock<ICompanyContext> _companyContext = new();

    public ExecutiveWorkspaceServiceTests()
    {
        _companyContext.SetupProperty(context => context.CompanyId);
        _companyRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => CreateCompany(id));

        _overviewService
            .Setup(service => service.GetOverviewAsync(
                It.IsAny<Guid>(),
                It.IsAny<ExecutiveWorkspaceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, ExecutiveWorkspaceQueryParameters _, CancellationToken __) =>
                new ExecutiveOverviewResponse { CompanyId = companyId, Kpis = new ExecutiveKpiSummaryResponse() });

        _navigationService
            .Setup(service => service.GetNavigation(It.IsAny<Guid>()))
            .Returns((Guid companyId) => new ExecutiveNavigationResponse { CompanyId = companyId, Actions = [] });

        _aggregationService
            .Setup(service => service.BuildEnterpriseAsync(
                It.IsAny<Guid>(), It.IsAny<ExecutiveWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, ExecutiveWorkspaceQueryParameters _, CancellationToken __) =>
                new ExecutiveEnterpriseSectionResponse { CompanyId = companyId });
        _aggregationService
            .Setup(service => service.BuildPortfoliosAsync(
                It.IsAny<Guid>(), It.IsAny<ExecutiveWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, ExecutiveWorkspaceQueryParameters _, CancellationToken __) =>
                new ExecutivePortfoliosSectionResponse { CompanyId = companyId });
        _aggregationService
            .Setup(service => service.BuildRecommendationsAsync(
                It.IsAny<Guid>(), It.IsAny<ExecutiveWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, ExecutiveWorkspaceQueryParameters _, CancellationToken __) =>
                new ExecutiveRecommendationsSectionResponse { CompanyId = companyId });
        _aggregationService
            .Setup(service => service.BuildDecisionsAsync(
                It.IsAny<Guid>(), It.IsAny<ExecutiveWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, ExecutiveWorkspaceQueryParameters _, CancellationToken __) =>
                new ExecutiveDecisionsSectionResponse { CompanyId = companyId });
        _aggregationService
            .Setup(service => service.BuildCapacityAsync(
                It.IsAny<Guid>(), It.IsAny<ExecutiveWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, ExecutiveWorkspaceQueryParameters _, CancellationToken __) =>
                new ExecutiveCapacitySectionResponse { CompanyId = companyId });
        _aggregationService
            .Setup(service => service.BuildWorkloadAsync(
                It.IsAny<Guid>(), It.IsAny<ExecutiveWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, ExecutiveWorkspaceQueryParameters _, CancellationToken __) =>
                new ExecutiveWorkloadSectionResponse { CompanyId = companyId });
        _aggregationService
            .Setup(service => service.BuildAiAsync(
                It.IsAny<Guid>(), It.IsAny<ExecutiveWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, ExecutiveWorkspaceQueryParameters _, CancellationToken __) =>
                new ExecutiveAiSectionResponse { CompanyId = companyId });
        _aggregationService
            .Setup(service => service.BuildAuditAsync(
                It.IsAny<Guid>(), It.IsAny<ExecutiveWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, ExecutiveWorkspaceQueryParameters _, CancellationToken __) =>
                new ExecutiveAuditSectionResponse { CompanyId = companyId });
    }

    private ExecutiveWorkspaceService CreateService() =>
        new(
            _overviewService.Object,
            _aggregationService.Object,
            _navigationService.Object,
            _companyRepository.Object,
            _companyContext.Object);

    [Fact]
    public async Task GetWorkspaceAsync_ResolvesCompanyId_FromParametersFirst()
    {
        var explicitCompanyId = Guid.NewGuid();
        _companyContext.Object.CompanyId = Guid.NewGuid();

        var workspace = await CreateService().GetWorkspaceAsync(
            new ExecutiveWorkspaceQueryParameters { CompanyId = explicitCompanyId });

        Assert.Equal(explicitCompanyId, workspace.CompanyId);
    }

    [Fact]
    public async Task GetWorkspaceAsync_ResolvesCompanyId_FromCompanyContext_WhenParametersUnset()
    {
        var contextCompanyId = Guid.NewGuid();
        _companyContext.Object.CompanyId = contextCompanyId;

        var workspace = await CreateService().GetWorkspaceAsync(new ExecutiveWorkspaceQueryParameters());

        Assert.Equal(contextCompanyId, workspace.CompanyId);
    }

    [Fact]
    public async Task GetWorkspaceAsync_FallsBackToDefaultCompany_WhenUnset()
    {
        _companyContext.Object.CompanyId = null;

        var workspace = await CreateService().GetWorkspaceAsync(new ExecutiveWorkspaceQueryParameters());

        Assert.Equal(AgencyOSCompanies.DefaultCompanyId, workspace.CompanyId);
    }

    [Fact]
    public async Task GetWorkspaceAsync_ThrowsNotFound_WhenCompanyMissing()
    {
        _companyRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().GetWorkspaceAsync(new ExecutiveWorkspaceQueryParameters { CompanyId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task GetWorkspaceAsync_DefaultsToTrailingThirtyDayWindow()
    {
        var workspace = await CreateService().GetWorkspaceAsync(new ExecutiveWorkspaceQueryParameters());

        Assert.NotNull(workspace.From);
        Assert.NotNull(workspace.To);
        Assert.True((workspace.To!.Value - workspace.From!.Value).TotalDays is > 29 and < 31);
        Assert.NotNull(workspace.PeriodStart);
        Assert.NotNull(workspace.PeriodEnd);
        Assert.True((workspace.PeriodEnd!.Value.DayNumber - workspace.PeriodStart!.Value.DayNumber) is > 29 and < 31);
    }

    [Fact]
    public async Task GetWorkspaceAsync_AssemblesEverySectionAndNavigation()
    {
        var workspace = await CreateService().GetWorkspaceAsync(new ExecutiveWorkspaceQueryParameters());

        Assert.Equal(workspace.CompanyId, workspace.Enterprise.CompanyId);
        Assert.Equal(workspace.CompanyId, workspace.Portfolios.CompanyId);
        Assert.Equal(workspace.CompanyId, workspace.Recommendations.CompanyId);
        Assert.Equal(workspace.CompanyId, workspace.Decisions.CompanyId);
        Assert.Equal(workspace.CompanyId, workspace.Capacity.CompanyId);
        Assert.Equal(workspace.CompanyId, workspace.Workload.CompanyId);
        Assert.Equal(workspace.CompanyId, workspace.Ai.CompanyId);
        Assert.Equal(workspace.CompanyId, workspace.Audit.CompanyId);
        Assert.Equal(workspace.CompanyId, workspace.Navigation.CompanyId);
        Assert.Equal(workspace.Overview.Kpis, workspace.Kpis);
    }

    [Fact]
    public async Task GetEnterpriseAsync_NeverCallsOtherAggregationMethods()
    {
        await CreateService().GetEnterpriseAsync(new ExecutiveWorkspaceQueryParameters());

        _aggregationService.Verify(
            service => service.BuildEnterpriseAsync(
                It.IsAny<Guid>(), It.IsAny<ExecutiveWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _aggregationService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetOverviewAsync_DelegatesToOverviewService()
    {
        var companyId = Guid.NewGuid();
        var overview = await CreateService().GetOverviewAsync(
            new ExecutiveWorkspaceQueryParameters { CompanyId = companyId });

        Assert.Equal(companyId, overview.CompanyId);
        _overviewService.Verify(
            service => service.GetOverviewAsync(companyId, It.IsAny<ExecutiveWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.Once);
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

/// <summary>
/// End-to-end verification that the Executive Workspace never calls a write method on
/// <see cref="IEnterpriseDashboardService"/> — wires the real
/// <see cref="ExecutiveOverviewService"/>, <see cref="ExecutiveAggregationService"/>,
/// <see cref="ExecutiveKpiService"/>, and <see cref="ExecutiveNavigationService"/> against a
/// mocked <see cref="IEnterpriseDashboardService"/> and <see cref="IDashboardHealthCalculationService"/>
/// (DEC-505-001).
/// </summary>
public class ExecutiveWorkspaceServiceReadOnlyTests
{
    [Fact]
    public async Task GetWorkspaceAsync_NeverCallsAnyWriteMethod_EndToEnd()
    {
        var companyId = Guid.NewGuid();

        var enterpriseDashboardService = new Mock<IEnterpriseDashboardService>();
        enterpriseDashboardService
            .Setup(service => service.GetSummaryAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardSummaryResponse());
        enterpriseDashboardService
            .Setup(service => service.GetPortfolioAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardPortfolioResponse());
        enterpriseDashboardService
            .Setup(service => service.GetCapacityAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardCapacityResponse());
        enterpriseDashboardService
            .Setup(service => service.GetWorkloadAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardWorkloadResponse());
        enterpriseDashboardService
            .Setup(service => service.GetRecommendationsAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardRecommendationsResponse());
        enterpriseDashboardService
            .Setup(service => service.GetDecisionsAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardDecisionsResponse());
        enterpriseDashboardService
            .Setup(service => service.GetAiAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardAiResponse());
        enterpriseDashboardService
            .Setup(service => service.GetAuditAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardAuditResponse());

        var healthCalculationService = new Mock<IDashboardHealthCalculationService>();
        healthCalculationService
            .Setup(service => service.CalculateOverall(It.IsAny<IReadOnlyCollection<string>>()))
            .Returns(new HealthIndicator { Status = "Healthy" });

        var kpiService = new ExecutiveKpiService();
        var overviewService = new ExecutiveOverviewService(enterpriseDashboardService.Object, kpiService, healthCalculationService.Object);
        var aggregationService = new ExecutiveAggregationService(enterpriseDashboardService.Object);
        var navigationService = new ExecutiveNavigationService();

        var companyRepository = new Mock<ICompanyRepository>();
        companyRepository
            .Setup(repository => repository.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Company.Create(
                companyId,
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
                DateTimeOffset.UtcNow));

        var companyContext = new Mock<ICompanyContext>();

        var workspaceService = new ExecutiveWorkspaceService(
            overviewService,
            aggregationService,
            navigationService,
            companyRepository.Object,
            companyContext.Object);

        await workspaceService.GetWorkspaceAsync(new ExecutiveWorkspaceQueryParameters { CompanyId = companyId });

        enterpriseDashboardService.Verify(
            service => service.GetSummaryAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
        enterpriseDashboardService.Verify(
            service => service.GetDashboardAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.Never);
        enterpriseDashboardService.Verify(
            service => service.GetPlanningAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
