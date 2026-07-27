using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using Moq;

namespace AgencyOS.Application.Tests;

public class ExecutiveAggregationServiceTests
{
    private readonly Mock<IEnterpriseDashboardService> _enterpriseDashboardService = new();
    private readonly ExecutiveAggregationService _service;

    public ExecutiveAggregationServiceTests()
    {
        _service = new ExecutiveAggregationService(_enterpriseDashboardService.Object);
    }

    [Fact]
    public async Task BuildEnterpriseAsync_EmbedsSummary_AndSetsDefaultDrillDownPath()
    {
        var companyId = Guid.NewGuid();
        var summary = new EnterpriseDashboardSummaryResponse { CompanyId = companyId, PortfolioCount = 5 };
        _enterpriseDashboardService
            .Setup(service => service.GetSummaryAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var result = await _service.BuildEnterpriseAsync(companyId, new ExecutiveWorkspaceQueryParameters());

        Assert.Equal(companyId, result.CompanyId);
        Assert.Same(summary, result.Summary);
        Assert.Equal("/enterprise-dashboard", result.DrillDownPath);
    }

    [Fact]
    public async Task BuildPortfoliosAsync_EmbedsPortfolio_AndDrillsIntoPortfolioAnalytics()
    {
        var companyId = Guid.NewGuid();
        var portfolio = new EnterpriseDashboardPortfolioResponse { PortfolioCount = 7 };
        _enterpriseDashboardService
            .Setup(service => service.GetPortfolioAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(portfolio);

        var result = await _service.BuildPortfoliosAsync(companyId, new ExecutiveWorkspaceQueryParameters());

        Assert.Same(portfolio, result.Portfolio);
        Assert.Equal("/portfolio-analytics", result.DrillDownPath);
    }

    [Fact]
    public async Task BuildRecommendationsAsync_EmbedsRecommendations_AndDrillsIntoRecommendationWorkspace()
    {
        var companyId = Guid.NewGuid();
        var recommendations = new EnterpriseDashboardRecommendationsResponse { TotalCount = 3 };
        _enterpriseDashboardService
            .Setup(service => service.GetRecommendationsAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendations);

        var result = await _service.BuildRecommendationsAsync(companyId, new ExecutiveWorkspaceQueryParameters());

        Assert.Same(recommendations, result.Recommendations);
        Assert.Equal("/recommendation-workspace", result.DrillDownPath);
    }

    [Fact]
    public async Task BuildDecisionsAsync_EmbedsDecisions_AndDrillsIntoDecisionWorkspace()
    {
        var companyId = Guid.NewGuid();
        var decisions = new EnterpriseDashboardDecisionsResponse { TotalCount = 4 };
        _enterpriseDashboardService
            .Setup(service => service.GetDecisionsAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(decisions);

        var result = await _service.BuildDecisionsAsync(companyId, new ExecutiveWorkspaceQueryParameters());

        Assert.Same(decisions, result.Decisions);
        Assert.Equal("/decision-workspace", result.DrillDownPath);
    }

    [Fact]
    public async Task BuildCapacityAsync_EmbedsCapacity_AndDrillsIntoCapacityHistory()
    {
        var companyId = Guid.NewGuid();
        var capacity = new EnterpriseDashboardCapacityResponse { RecordCount = 6 };
        _enterpriseDashboardService
            .Setup(service => service.GetCapacityAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(capacity);

        var result = await _service.BuildCapacityAsync(companyId, new ExecutiveWorkspaceQueryParameters());

        Assert.Same(capacity, result.Capacity);
        Assert.Equal("/capacity/history", result.DrillDownPath);
    }

    [Fact]
    public async Task BuildWorkloadAsync_EmbedsWorkload_AndDrillsIntoWorkloadHistory()
    {
        var companyId = Guid.NewGuid();
        var workload = new EnterpriseDashboardWorkloadResponse { RecordCount = 8 };
        _enterpriseDashboardService
            .Setup(service => service.GetWorkloadAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workload);

        var result = await _service.BuildWorkloadAsync(companyId, new ExecutiveWorkspaceQueryParameters());

        Assert.Same(workload, result.Workload);
        Assert.Equal("/workload/history", result.DrillDownPath);
    }

    [Fact]
    public async Task BuildAiAsync_EmbedsAi_AndDrillsIntoAiRecommendations()
    {
        var companyId = Guid.NewGuid();
        var ai = new EnterpriseDashboardAiResponse { AIRecommendationCount = 2 };
        _enterpriseDashboardService
            .Setup(service => service.GetAiAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ai);

        var result = await _service.BuildAiAsync(companyId, new ExecutiveWorkspaceQueryParameters());

        Assert.Same(ai, result.Ai);
        Assert.Equal("/ai-recommendations", result.DrillDownPath);
    }

    [Fact]
    public async Task BuildAuditAsync_EmbedsAudit_AndDrillsIntoAudit()
    {
        var companyId = Guid.NewGuid();
        var audit = new EnterpriseDashboardAuditResponse { EventCount = 9 };
        _enterpriseDashboardService
            .Setup(service => service.GetAuditAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(audit);

        var result = await _service.BuildAuditAsync(companyId, new ExecutiveWorkspaceQueryParameters());

        Assert.Same(audit, result.Audit);
        Assert.Equal("/audit", result.DrillDownPath);
    }

    [Fact]
    public async Task BuildEnterpriseAsync_MapsCompanyIdAndWindow_OntoDashboardParameters()
    {
        var companyId = Guid.NewGuid();
        var from = DateTimeOffset.UtcNow.AddDays(-10);
        var to = DateTimeOffset.UtcNow;
        EnterpriseDashboardQueryParameters? captured = null;

        _enterpriseDashboardService
            .Setup(service => service.GetSummaryAsync(It.IsAny<EnterpriseDashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .Callback<EnterpriseDashboardQueryParameters, CancellationToken>((parameters, _) => captured = parameters)
            .ReturnsAsync(new EnterpriseDashboardSummaryResponse());

        await _service.BuildEnterpriseAsync(companyId, new ExecutiveWorkspaceQueryParameters { From = from, To = to });

        Assert.NotNull(captured);
        Assert.Equal(companyId, captured!.CompanyId);
        Assert.Equal(from, captured.From);
        Assert.Equal(to, captured.To);
    }
}
