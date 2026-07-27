using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Moq;

namespace AgencyOS.Application.Tests;

public class PortfolioTrendAnalysisServiceTests
{
    private static readonly Guid CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

    private readonly Mock<IPortfolioRepository> _portfolioRepository = new();
    private readonly Mock<IRecommendationRepository> _recommendationRepository = new();
    private readonly Mock<IDecisionRepository> _decisionRepository = new();
    private readonly Mock<ICapacityHistoryRepository> _capacityHistoryRepository = new();
    private readonly Mock<IWorkloadHistoryRepository> _workloadHistoryRepository = new();

    public PortfolioTrendAnalysisServiceTests()
    {
        _recommendationRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<RecommendationQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Recommendation>());
        _decisionRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<DecisionQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Decision>());
        _capacityHistoryRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<CapacityHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CapacityHistory>());
        _workloadHistoryRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<WorkloadHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<WorkloadHistory>());
    }

    private PortfolioTrendAnalysisService CreateService() =>
        new(
            _portfolioRepository.Object,
            _recommendationRepository.Object,
            _decisionRepository.Object,
            _capacityHistoryRepository.Object,
            _workloadHistoryRepository.Object);

    [Fact]
    public async Task BuildTrendsAsync_BucketsHistoryByCalendarMonth()
    {
        var periodStart = new DateOnly(2026, 1, 1);
        var periodEnd = new DateOnly(2026, 3, 31);

        var capacityJan = CreateCapacityHistory(new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero), 60m);
        var capacityFeb = CreateCapacityHistory(new DateTimeOffset(2026, 2, 10, 0, 0, 0, TimeSpan.Zero), 80m);

        _capacityHistoryRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<CapacityHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([capacityJan, capacityFeb]);

        var service = CreateService();

        var trends = await service.BuildTrendsAsync(
            CompanyId,
            new PortfolioAnalyticsQueryParameters { PeriodStart = periodStart, PeriodEnd = periodEnd });

        Assert.Equal(3, trends.Points.Count);
        Assert.Equal("2026-01", trends.Points[0].PeriodLabel);
        Assert.Equal(60m, trends.Points[0].CapacityUtilization);
        Assert.Equal("2026-02", trends.Points[1].PeriodLabel);
        Assert.Equal(80m, trends.Points[1].CapacityUtilization);
        Assert.Equal("2026-03", trends.Points[2].PeriodLabel);
        Assert.Null(trends.Points[2].CapacityUtilization);
    }

    [Fact]
    public async Task BuildTrendsAsync_AveragesMultipleHistoryRecordsInSameMonth()
    {
        var periodStart = new DateOnly(2026, 1, 1);
        var periodEnd = new DateOnly(2026, 1, 31);

        var first = CreateCapacityHistory(new DateTimeOffset(2026, 1, 5, 0, 0, 0, TimeSpan.Zero), 60m);
        var second = CreateCapacityHistory(new DateTimeOffset(2026, 1, 20, 0, 0, 0, TimeSpan.Zero), 80m);

        _capacityHistoryRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<CapacityHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([first, second]);

        var service = CreateService();

        var trends = await service.BuildTrendsAsync(
            CompanyId,
            new PortfolioAnalyticsQueryParameters { PeriodStart = periodStart, PeriodEnd = periodEnd });

        var point = Assert.Single(trends.Points);
        Assert.Equal(70m, point.CapacityUtilization);
    }

    [Fact]
    public async Task BuildTrendsAsync_DerivesHealthStatus_FromCapacityAndWorkload()
    {
        var periodStart = new DateOnly(2026, 1, 1);
        var periodEnd = new DateOnly(2026, 1, 31);

        var capacity = CreateCapacityHistory(new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero), 95m);
        var workload = CreateWorkloadHistory(new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero), 95m);

        _capacityHistoryRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<CapacityHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([capacity]);
        _workloadHistoryRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<WorkloadHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([workload]);

        var service = CreateService();

        var trends = await service.BuildTrendsAsync(
            CompanyId,
            new PortfolioAnalyticsQueryParameters { PeriodStart = periodStart, PeriodEnd = periodEnd });

        var point = Assert.Single(trends.Points);
        Assert.Equal(PortfolioHealth.AtRisk, point.HealthStatus);
    }

    [Fact]
    public async Task BuildTrendsAsync_ScopesToPortfolioMissions_WhenPortfolioIdProvided()
    {
        var missionId = Guid.NewGuid();
        var portfolio = Portfolio.Create(
            CompanyId,
            "Scoped Portfolio",
            null,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            null,
            [(missionId, 1)],
            DateTimeOffset.UtcNow);

        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(portfolio.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(portfolio);

        var missionRecommendation = CreateRecommendation(missionId);
        _recommendationRepository
            .Setup(repository => repository.GetByMissionIdAsync(missionId, It.IsAny<RecommendationQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([missionRecommendation]);

        var service = CreateService();

        var trends = await service.BuildTrendsAsync(
            CompanyId,
            new PortfolioAnalyticsQueryParameters
            {
                PortfolioId = portfolio.Id,
                PeriodStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
                PeriodEnd = DateOnly.FromDateTime(DateTime.UtcNow)
            });

        Assert.Equal(portfolio.Id, trends.PortfolioId);
        Assert.Equal($"/portfolios/{portfolio.Id}", trends.DrillDownPath);
        Assert.Equal(1, trends.Points.Sum(point => point.RecommendationCount));
        _recommendationRepository.Verify(
            repository => repository.QueryAsync(It.IsAny<RecommendationQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task BuildTrendsAsync_ThrowsNotFound_WhenScopedPortfolioMissing()
    {
        var service = CreateService();
        var missingPortfolioId = Guid.NewGuid();

        await Assert.ThrowsAsync<NotFoundException>(() => service.BuildTrendsAsync(
            CompanyId,
            new PortfolioAnalyticsQueryParameters { PortfolioId = missingPortfolioId }));
    }

    [Fact]
    public async Task BuildTrendsAsync_ThrowsBusinessRule_WhenPortfolioBelongsToDifferentCompany()
    {
        var missionId = Guid.NewGuid();
        var portfolio = Portfolio.Create(
            Guid.NewGuid(),
            "Other Company Portfolio",
            null,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            null,
            [(missionId, 1)],
            DateTimeOffset.UtcNow);

        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(portfolio.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(portfolio);

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.BuildTrendsAsync(
            CompanyId,
            new PortfolioAnalyticsQueryParameters { PortfolioId = portfolio.Id }));
    }

    private static CapacityHistory CreateCapacityHistory(DateTimeOffset calculationDate, decimal utilization) =>
        CapacityHistory.Create(
            Guid.NewGuid(),
            CompanyId,
            DateOnly.FromDateTime(calculationDate.UtcDateTime.AddDays(-30)),
            DateOnly.FromDateTime(calculationDate.UtcDateTime),
            20,
            2,
            18,
            160m,
            160m,
            160m,
            160m * utilization / 100m,
            utilization,
            CapacityHistoryVersions.Current,
            "{}",
            calculationDate);

    private static WorkloadHistory CreateWorkloadHistory(DateTimeOffset calculationDate, decimal workload) =>
        WorkloadHistory.Create(
            Guid.NewGuid(),
            CompanyId,
            DateOnly.FromDateTime(calculationDate.UtcDateTime.AddDays(-30)),
            DateOnly.FromDateTime(calculationDate.UtcDateTime),
            160m * workload / 100m,
            160m,
            workload,
            20,
            2,
            18,
            WorkloadHistoryVersions.Current,
            "{}",
            calculationDate);

    private static Recommendation CreateRecommendation(Guid missionId, decimal? score = 75m) =>
        Recommendation.Create(
            CompanyId,
            missionId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            $"REC-{Guid.NewGuid():N}",
            "Trend Recommendation",
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
