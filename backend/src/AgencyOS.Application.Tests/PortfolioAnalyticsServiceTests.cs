using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Moq;

namespace AgencyOS.Application.Tests;

public class PortfolioAnalyticsServiceTests
{
    private readonly Mock<IPortfolioRepository> _portfolioRepository = new();
    private readonly Mock<IRecommendationRepository> _recommendationRepository = new();
    private readonly Mock<IRecommendationHistoryRepository> _recommendationHistoryRepository = new();
    private readonly Mock<IDecisionRepository> _decisionRepository = new();
    private readonly Mock<ICapacityHistoryService> _capacityHistoryService = new();
    private readonly Mock<IWorkloadHistoryService> _workloadHistoryService = new();
    private readonly Mock<ICompanyRepository> _companyRepository = new();
    private readonly Mock<ICompanyContext> _companyContext = new();
    private readonly Mock<IPortfolioTrendAnalysisService> _trendAnalysisService = new();
    private readonly Mock<IPortfolioComparisonService> _comparisonService = new();
    private readonly Mock<IPortfolioHealthAnalyticsService> _healthAnalyticsService = new();
    private readonly Mock<IPortfolioRiskAnalyticsService> _riskAnalyticsService = new();

    public PortfolioAnalyticsServiceTests()
    {
        _companyContext.SetupProperty(context => context.CompanyId);
        _companyRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCompany());

        _portfolioRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<PortfolioQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Portfolio>());
        _recommendationRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<RecommendationQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Recommendation>());
        _recommendationRepository
            .Setup(repository => repository.GetByMissionIdAsync(It.IsAny<Guid>(), It.IsAny<RecommendationQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Recommendation>());
        _decisionRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<DecisionQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Decision>());
        _recommendationHistoryRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<RecommendationHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<RecommendationHistory>());
        _capacityHistoryService
            .Setup(service => service.AggregateAsync(It.IsAny<CapacityHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CapacityHistoryAggregateResponse());
        _workloadHistoryService
            .Setup(service => service.AggregateAsync(It.IsAny<WorkloadHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkloadHistoryAggregateResponse());
        _healthAnalyticsService
            .Setup(service => service.BuildHealthAnalytics(It.IsAny<Guid>(), It.IsAny<IReadOnlyList<PortfolioAnalyticsSnapshot>>()))
            .Returns(new PortfolioHealthAnalyticsResponse());
        _riskAnalyticsService
            .Setup(service => service.BuildRiskIndicators(It.IsAny<IReadOnlyList<PortfolioAnalyticsSnapshot>>()))
            .Returns(Array.Empty<PortfolioRiskIndicatorResponse>());
    }

    private PortfolioAnalyticsService CreateService() =>
        new(
            _portfolioRepository.Object,
            _recommendationRepository.Object,
            _recommendationHistoryRepository.Object,
            _decisionRepository.Object,
            _capacityHistoryService.Object,
            _workloadHistoryService.Object,
            _companyRepository.Object,
            _companyContext.Object,
            new DashboardHealthCalculationService(),
            _trendAnalysisService.Object,
            _comparisonService.Object,
            _healthAnalyticsService.Object,
            _riskAnalyticsService.Object);

    [Fact]
    public async Task GetOverviewAsync_ResolvesCompanyId_FromParametersFirst()
    {
        var explicitCompanyId = Guid.NewGuid();
        _companyContext.Object.CompanyId = Guid.NewGuid();

        var service = CreateService();

        var overview = await service.GetOverviewAsync(new PortfolioAnalyticsQueryParameters { CompanyId = explicitCompanyId });

        Assert.Equal(explicitCompanyId, overview.CompanyId);
    }

    [Fact]
    public async Task GetOverviewAsync_ResolvesCompanyId_FromContext_WhenParametersUnset()
    {
        var contextCompanyId = Guid.NewGuid();
        _companyContext.Object.CompanyId = contextCompanyId;

        var service = CreateService();

        var overview = await service.GetOverviewAsync(new PortfolioAnalyticsQueryParameters());

        Assert.Equal(contextCompanyId, overview.CompanyId);
    }

    [Fact]
    public async Task GetOverviewAsync_ResolvesCompanyId_FromDefault_WhenNothingSet()
    {
        var service = CreateService();

        var overview = await service.GetOverviewAsync(new PortfolioAnalyticsQueryParameters());

        Assert.Equal(AgencyOSCompanies.DefaultCompanyId, overview.CompanyId);
    }

    [Fact]
    public async Task GetOverviewAsync_ThrowsNotFound_WhenCompanyMissing()
    {
        _companyRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetOverviewAsync(new PortfolioAnalyticsQueryParameters()));
    }

    [Fact]
    public async Task GetOverviewAsync_ReturnsOneCardPerPortfolio_WithHealthAndCounts()
    {
        var missionId = Guid.NewGuid();
        var portfolio = CreatePortfolio(missionId, utilization: 60m, workload: 55m);
        _portfolioRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<PortfolioQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([portfolio]);

        var recommendation = CreateRecommendation(missionId);
        _recommendationRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<RecommendationQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([recommendation]);

        var service = CreateService();

        var overview = await service.GetOverviewAsync(new PortfolioAnalyticsQueryParameters());

        Assert.Equal(1, overview.PortfolioCount);
        var card = Assert.Single(overview.Portfolios);
        Assert.Equal(portfolio.Id, card.PortfolioId);
        Assert.Equal(60m, card.UtilizationPercentage);
        Assert.Equal(1, card.RecommendationCount);
        Assert.Equal(PortfolioHealth.Healthy, card.Health.Status);
        Assert.Equal($"/portfolios/{portfolio.Id}", card.DrillDownPath);
    }

    [Fact]
    public async Task GetDetailAsync_ThrowsNotFound_WhenPortfolioMissing()
    {
        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Portfolio?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetDetailAsync(
            Guid.NewGuid(),
            new PortfolioAnalyticsQueryParameters()));
    }

    [Fact]
    public async Task GetDetailAsync_ThrowsBusinessRule_WhenPortfolioBelongsToDifferentCompany()
    {
        var portfolio = CreatePortfolio(Guid.NewGuid(), companyId: Guid.NewGuid());
        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(portfolio.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(portfolio);

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.GetDetailAsync(
            portfolio.Id,
            new PortfolioAnalyticsQueryParameters { CompanyId = AgencyOSCompanies.DefaultCompanyId }));
    }

    [Fact]
    public async Task GetDetailAsync_ReturnsFullAnalysis_ForSinglePortfolio()
    {
        var missionId = Guid.NewGuid();
        var portfolio = CreatePortfolio(missionId, utilization: 70m, workload: 65m);
        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(portfolio.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(portfolio);

        var recommendation = CreateRecommendation(missionId, score: 80m);
        _recommendationRepository
            .Setup(repository => repository.GetByMissionIdAsync(missionId, It.IsAny<RecommendationQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([recommendation]);

        var decision = Decision.Create(recommendation.Id, portfolio.CompanyId, missionId, Guid.NewGuid(), "planner", DateTimeOffset.UtcNow);
        _decisionRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<DecisionQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([decision]);

        _capacityHistoryService
            .Setup(service => service.AggregateAsync(It.IsAny<CapacityHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CapacityHistoryAggregateResponse { RecordCount = 3, AverageUtilizationPercentage = 72m });
        _workloadHistoryService
            .Setup(service => service.AggregateAsync(It.IsAny<WorkloadHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkloadHistoryAggregateResponse { RecordCount = 3, AverageWorkloadPercentage = 68m });

        var service = CreateService();

        var detail = await service.GetDetailAsync(portfolio.Id, new PortfolioAnalyticsQueryParameters());

        Assert.Equal(portfolio.Id, detail.PortfolioId);
        Assert.Equal(portfolio.Name, detail.Name);
        Assert.Equal(70m, detail.UtilizationPercentage);
        Assert.Equal(65m, detail.WorkloadPercentage);
        Assert.Equal(72m, detail.HistoricalAverageUtilizationPercentage);
        Assert.Equal(68m, detail.HistoricalAverageWorkloadPercentage);
        Assert.Equal(1, detail.RecommendationCount);
        Assert.Equal(80m, detail.AverageRecommendationScore);
        Assert.Equal(1, detail.DecisionCount);
        Assert.Equal($"/portfolios/{portfolio.Id}", detail.DrillDownPath);
        Assert.Contains(missionId, detail.MissionIds);
    }

    [Fact]
    public async Task GetDetailAsync_ReturnsNullHistoricalAverages_WhenNoHistoryRecords()
    {
        var missionId = Guid.NewGuid();
        var portfolio = CreatePortfolio(missionId);
        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(portfolio.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(portfolio);

        var service = CreateService();

        var detail = await service.GetDetailAsync(portfolio.Id, new PortfolioAnalyticsQueryParameters());

        Assert.Null(detail.HistoricalAverageUtilizationPercentage);
        Assert.Null(detail.HistoricalAverageWorkloadPercentage);
    }

    [Fact]
    public async Task GetTrendsAsync_DelegatesToTrendAnalysisService()
    {
        var expected = new PortfolioTrendsResponse();
        _trendAnalysisService
            .Setup(service => service.BuildTrendsAsync(It.IsAny<Guid>(), It.IsAny<PortfolioAnalyticsQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var service = CreateService();

        var trends = await service.GetTrendsAsync(new PortfolioAnalyticsQueryParameters());

        Assert.Same(expected, trends);
    }

    [Fact]
    public async Task GetComparisonAsync_DelegatesToComparisonService()
    {
        var expected = new PortfolioComparisonResponse();
        _comparisonService
            .Setup(service => service.CompareAsync(It.IsAny<Guid>(), It.IsAny<PortfolioCompareQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var service = CreateService();

        var comparison = await service.GetComparisonAsync(new PortfolioCompareQueryParameters
        {
            LeftPortfolioId = Guid.NewGuid(),
            RightPortfolioId = Guid.NewGuid()
        });

        Assert.Same(expected, comparison);
    }

    [Fact]
    public async Task GetRankingAsync_RanksHealthierAndBetterUtilizedPortfoliosHigher()
    {
        var strongMissionId = Guid.NewGuid();
        var weakMissionId = Guid.NewGuid();
        var strongPortfolio = CreatePortfolio(strongMissionId, utilization: 75m, workload: 70m);
        var weakPortfolio = CreatePortfolio(weakMissionId, utilization: 99m, workload: 99m);

        _portfolioRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<PortfolioQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([weakPortfolio, strongPortfolio]);

        var service = CreateService();

        var ranking = await service.GetRankingAsync(new PortfolioAnalyticsQueryParameters());

        Assert.Equal(2, ranking.Items.Count);
        Assert.Equal(1, ranking.Items[0].Rank);
        Assert.Equal(strongPortfolio.Id, ranking.Items[0].PortfolioId);
        Assert.Equal(2, ranking.Items[1].Rank);
        Assert.Equal(weakPortfolio.Id, ranking.Items[1].PortfolioId);
        Assert.True(ranking.Items[0].Score >= ranking.Items[1].Score);
    }

    [Fact]
    public async Task GetHealthAnalyticsAsync_MergesHealthAndRiskIndicators()
    {
        var portfolioId = Guid.NewGuid();
        var healthIndicator = new PortfolioRiskIndicatorResponse
        {
            PortfolioId = portfolioId,
            Name = "Portfolio A",
            RiskLevel = "High",
            Reason = "Health based reason",
            DrillDownPath = $"/portfolios/{portfolioId}"
        };
        var riskIndicator = new PortfolioRiskIndicatorResponse
        {
            PortfolioId = portfolioId,
            Name = "Portfolio A",
            RiskLevel = "Critical",
            Reason = "Score based reason",
            DrillDownPath = $"/portfolios/{portfolioId}"
        };
        var otherIndicator = new PortfolioRiskIndicatorResponse
        {
            PortfolioId = Guid.NewGuid(),
            Name = "Portfolio B",
            RiskLevel = "Medium",
            Reason = "Other reason",
            DrillDownPath = "/portfolios/other"
        };

        _healthAnalyticsService
            .Setup(service => service.BuildHealthAnalytics(It.IsAny<Guid>(), It.IsAny<IReadOnlyList<PortfolioAnalyticsSnapshot>>()))
            .Returns(new PortfolioHealthAnalyticsResponse { RiskIndicators = [healthIndicator] });
        _riskAnalyticsService
            .Setup(service => service.BuildRiskIndicators(It.IsAny<IReadOnlyList<PortfolioAnalyticsSnapshot>>()))
            .Returns([riskIndicator, otherIndicator]);

        var service = CreateService();

        var health = await service.GetHealthAnalyticsAsync(new PortfolioAnalyticsQueryParameters());

        Assert.Equal(2, health.RiskIndicators.Count);
        var merged = health.RiskIndicators.Single(indicator => indicator.PortfolioId == portfolioId);
        Assert.Equal("Critical", merged.RiskLevel);
        Assert.Equal("Score based reason", merged.Reason);
    }

    [Fact]
    public async Task GetPerformanceAsync_ComputesAveragesAndRatesAcrossPortfolios()
    {
        var missionOne = Guid.NewGuid();
        var missionTwo = Guid.NewGuid();
        var portfolioOne = CreatePortfolio(missionOne, utilization: 60m, workload: 50m);
        var portfolioTwo = CreatePortfolio(missionTwo, utilization: 80m, workload: 70m);

        _portfolioRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<PortfolioQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([portfolioOne, portfolioTwo]);

        var recommendationOne = CreateRecommendation(missionOne, score: 60m);
        var recommendationTwo = CreateRecommendation(missionTwo, score: 80m);
        _recommendationRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<RecommendationQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([recommendationOne, recommendationTwo]);

        var completed = Decision.Create(recommendationOne.Id, portfolioOne.CompanyId, missionOne, Guid.NewGuid(), "planner", DateTimeOffset.UtcNow);
        completed.StartImplementation("planner", null, DateTimeOffset.UtcNow);
        completed.Complete("planner", null, DateTimeOffset.UtcNow);

        var cancelled = Decision.Create(recommendationTwo.Id, portfolioTwo.CompanyId, missionTwo, Guid.NewGuid(), "planner", DateTimeOffset.UtcNow);
        cancelled.Cancel("planner", null, DateTimeOffset.UtcNow);

        _decisionRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<DecisionQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([completed, cancelled]);

        var service = CreateService();

        var performance = await service.GetPerformanceAsync(new PortfolioAnalyticsQueryParameters());

        Assert.Equal(2, performance.PortfolioCount);
        Assert.Equal(70m, performance.AverageUtilizationPercentage);
        Assert.Equal(60m, performance.AverageWorkloadPercentage);
        Assert.Equal(70m, performance.AverageRecommendationScore);
        Assert.Equal(2, performance.RecommendationCount);
        Assert.Equal(2, performance.DecisionCount);
        Assert.Equal(50m, performance.DecisionCompletionRatePercentage);
        Assert.Equal(50m, performance.DecisionCancellationRatePercentage);
        Assert.Equal(50m, performance.RecommendationEffectivenessPercentage);
    }

    [Fact]
    public async Task GetPerformanceAsync_ReturnsZeroedRates_WhenNoDecisions()
    {
        var service = CreateService();

        var performance = await service.GetPerformanceAsync(new PortfolioAnalyticsQueryParameters());

        Assert.Equal(0, performance.PortfolioCount);
        Assert.Equal(0m, performance.DecisionCompletionRatePercentage);
        Assert.Equal(0m, performance.DecisionCancellationRatePercentage);
        Assert.Null(performance.AverageRecommendationScore);
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
        Guid missionId,
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
            [(missionId, 1)],
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

    private static Recommendation CreateRecommendation(Guid missionId, decimal? score = 75m) =>
        Recommendation.Create(
            AgencyOSCompanies.DefaultCompanyId,
            missionId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            $"REC-{Guid.NewGuid():N}",
            "Portfolio Analytics Recommendation",
            "Summary",
            "Reason",
            score,
            1,
            1,
            RecommendationVersions.CurrentDecisionEngineVersion,
            null,
            "{}",
            "{}",
            "{\"ok\":true}",
            "decision-engine",
            DateTimeOffset.UtcNow);
}
