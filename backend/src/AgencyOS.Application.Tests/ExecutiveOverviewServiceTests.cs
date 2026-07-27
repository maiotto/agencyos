using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using Moq;

namespace AgencyOS.Application.Tests;

public class ExecutiveOverviewServiceTests
{
    private readonly Mock<IEnterpriseDashboardService> _enterpriseDashboardService = new();
    private readonly Mock<IExecutiveKpiService> _executiveKpiService = new();
    private readonly Mock<IDashboardHealthCalculationService> _dashboardHealthCalculationService = new();
    private readonly ExecutiveOverviewService _service;

    public ExecutiveOverviewServiceTests()
    {
        _service = new ExecutiveOverviewService(
            _enterpriseDashboardService.Object,
            _executiveKpiService.Object,
            _dashboardHealthCalculationService.Object);

        _enterpriseDashboardService
            .Setup(service => service.GetSummaryAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardSummaryResponse
            {
                PortfolioCount = 10,
                ActivePortfolioCount = 8,
                RecommendationCount = 15,
                DecisionCount = 6,
                PendingDecisionCount = 2,
                OverallHealth = new HealthIndicator { Status = "Healthy" }
            });
        _enterpriseDashboardService
            .Setup(service => service.GetPortfolioAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardPortfolioResponse { OverallHealth = new HealthIndicator { Status = "Healthy" } });
        _enterpriseDashboardService
            .Setup(service => service.GetCapacityAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardCapacityResponse { AverageUtilizationPercentage = 55m });
        _enterpriseDashboardService
            .Setup(service => service.GetWorkloadAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardWorkloadResponse { AverageWorkloadPercentage = 47m });
        _enterpriseDashboardService
            .Setup(service => service.GetAuditAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardAuditResponse { EventCount = 13 });

        _dashboardHealthCalculationService
            .Setup(service => service.CalculateOverall(It.IsAny<IReadOnlyCollection<string>>()))
            .Returns(new HealthIndicator { Status = "Healthy", Label = "Healthy" });

        _executiveKpiService
            .Setup(service => service.BuildKpiSummary(
                It.IsAny<EnterpriseDashboardSummaryResponse>(),
                It.IsAny<EnterpriseDashboardCapacityResponse?>(),
                It.IsAny<EnterpriseDashboardWorkloadResponse?>(),
                It.IsAny<EnterpriseDashboardAuditResponse?>(),
                It.IsAny<HealthIndicator?>()))
            .Returns(new ExecutiveKpiSummaryResponse { PortfolioCount = 10 });
    }

    [Fact]
    public async Task GetOverviewAsync_SetsCompanyIdAndGeneratedAt()
    {
        var companyId = Guid.NewGuid();
        var overview = await _service.GetOverviewAsync(companyId, new ExecutiveWorkspaceQueryParameters());

        Assert.Equal(companyId, overview.CompanyId);
        Assert.True(overview.GeneratedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task GetOverviewAsync_CombinesHealth_ViaDashboardHealthCalculationService()
    {
        var overview = await _service.GetOverviewAsync(Guid.NewGuid(), new ExecutiveWorkspaceQueryParameters());

        Assert.Equal("Healthy", overview.OverallHealth.Status);
        _dashboardHealthCalculationService.Verify(
            service => service.CalculateOverall(It.IsAny<IReadOnlyCollection<string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOverviewAsync_DelegatesKpiComputation_ToKpiService()
    {
        var overview = await _service.GetOverviewAsync(Guid.NewGuid(), new ExecutiveWorkspaceQueryParameters());

        Assert.Equal(10, overview.Kpis.PortfolioCount);
        _executiveKpiService.Verify(
            service => service.BuildKpiSummary(
                It.IsAny<EnterpriseDashboardSummaryResponse>(),
                It.IsAny<EnterpriseDashboardCapacityResponse?>(),
                It.IsAny<EnterpriseDashboardWorkloadResponse?>(),
                It.IsAny<EnterpriseDashboardAuditResponse?>(),
                It.IsAny<HealthIndicator?>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOverviewAsync_BuildsNonEmptyNarrativeAndNavigationTip()
    {
        var overview = await _service.GetOverviewAsync(Guid.NewGuid(), new ExecutiveWorkspaceQueryParameters());

        Assert.False(string.IsNullOrWhiteSpace(overview.Narrative));
        Assert.False(string.IsNullOrWhiteSpace(overview.NavigationTip));
        Assert.Contains("10 portfolios", overview.Narrative);
    }

    [Fact]
    public async Task GetOverviewAsync_NeverCallsAnyWriteMethod_OnEnterpriseDashboardService()
    {
        await _service.GetOverviewAsync(Guid.NewGuid(), new ExecutiveWorkspaceQueryParameters());

        _enterpriseDashboardService.Verify(
            service => service.GetSummaryAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _enterpriseDashboardService.Verify(
            service => service.GetPortfolioAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _enterpriseDashboardService.Verify(
            service => service.GetCapacityAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _enterpriseDashboardService.Verify(
            service => service.GetWorkloadAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _enterpriseDashboardService.Verify(
            service => service.GetAuditAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _enterpriseDashboardService.VerifyNoOtherCalls();
    }
}
