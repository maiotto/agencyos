using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Moq;

namespace AgencyOS.Application.Tests;

public class EnterpriseDashboardServiceTests
{
    private readonly Mock<IDashboardAggregationService> _aggregationService = new();
    private readonly Mock<IDashboardTrendService> _trendService = new();
    private readonly Mock<ICompanyRepository> _companyRepository = new();
    private readonly Mock<ICompanyDecisionProfileRepository> _companyDecisionProfileRepository = new();
    private readonly Mock<ICompanyContext> _companyContext = new();

    public EnterpriseDashboardServiceTests()
    {
        _companyContext.SetupProperty(context => context.CompanyId);
        _companyRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCompany());

        _aggregationService
            .Setup(service => service.BuildSummaryAsync(
                It.IsAny<Guid>(),
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardSummaryResponse());
        _aggregationService
            .Setup(service => service.BuildPlanningAsync(
                It.IsAny<Guid>(),
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardPlanningResponse());
        _aggregationService
            .Setup(service => service.BuildPortfolioAsync(
                It.IsAny<Guid>(),
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardPortfolioResponse());
        _aggregationService
            .Setup(service => service.BuildCapacityAsync(
                It.IsAny<Guid>(),
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardCapacityResponse());
        _aggregationService
            .Setup(service => service.BuildWorkloadAsync(
                It.IsAny<Guid>(),
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardWorkloadResponse());
        _aggregationService
            .Setup(service => service.BuildRecommendationsAsync(
                It.IsAny<Guid>(),
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardRecommendationsResponse());
        _aggregationService
            .Setup(service => service.BuildDecisionsAsync(
                It.IsAny<Guid>(),
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardDecisionsResponse());
        _aggregationService
            .Setup(service => service.BuildAiAsync(
                It.IsAny<Guid>(),
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardAiResponse());
        _aggregationService
            .Setup(service => service.BuildAuditAsync(
                It.IsAny<Guid>(),
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardAuditResponse());

        _trendService
            .Setup(service => service.Calculate(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new TrendIndicator { Direction = "Flat", DeltaPercent = 0m });
        _trendService
            .Setup(service => service.Calculate(It.IsAny<decimal>(), It.IsAny<decimal>()))
            .Returns(new TrendIndicator { Direction = "Flat", DeltaPercent = 0m });
    }

    private EnterpriseDashboardService CreateService() =>
        new(
            _aggregationService.Object,
            _trendService.Object,
            _companyRepository.Object,
            _companyDecisionProfileRepository.Object,
            _companyContext.Object);

    [Fact]
    public async Task GetDashboardAsync_ResolvesCompanyId_FromParametersFirst()
    {
        var explicitCompanyId = Guid.NewGuid();
        _companyContext.Object.CompanyId = Guid.NewGuid();

        var service = CreateService();

        var dashboard = await service.GetDashboardAsync(
            new EnterpriseDashboardQueryParameters { CompanyId = explicitCompanyId });

        Assert.Equal(explicitCompanyId, dashboard.CompanyId);
    }

    [Fact]
    public async Task GetDashboardAsync_ResolvesCompanyId_FromCompanyContext_WhenParametersUnset()
    {
        var contextCompanyId = Guid.NewGuid();
        _companyContext.Object.CompanyId = contextCompanyId;

        var service = CreateService();

        var dashboard = await service.GetDashboardAsync(new EnterpriseDashboardQueryParameters());

        Assert.Equal(contextCompanyId, dashboard.CompanyId);
    }

    [Fact]
    public async Task GetDashboardAsync_ResolvesCompanyId_FromDefault_WhenNothingSet()
    {
        var service = CreateService();

        var dashboard = await service.GetDashboardAsync(new EnterpriseDashboardQueryParameters());

        Assert.Equal(AgencyOSCompanies.DefaultCompanyId, dashboard.CompanyId);
    }

    [Fact]
    public async Task GetDashboardAsync_ThrowsNotFound_WhenCompanyMissing()
    {
        _companyRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetDashboardAsync(new EnterpriseDashboardQueryParameters()));
    }

    [Fact]
    public async Task GetDashboardAsync_PopulatesAllSections()
    {
        var service = CreateService();

        var dashboard = await service.GetDashboardAsync(new EnterpriseDashboardQueryParameters());

        Assert.NotNull(dashboard.Summary);
        Assert.NotNull(dashboard.Planning);
        Assert.NotNull(dashboard.Portfolio);
        Assert.NotNull(dashboard.Capacity);
        Assert.NotNull(dashboard.Workload);
        Assert.NotNull(dashboard.Recommendations);
        Assert.NotNull(dashboard.Decisions);
        Assert.NotNull(dashboard.Ai);
        Assert.NotNull(dashboard.Audit);
        Assert.True(dashboard.From.HasValue);
        Assert.True(dashboard.To.HasValue);
        Assert.True(dashboard.PeriodStart.HasValue);
        Assert.True(dashboard.PeriodEnd.HasValue);
    }

    [Fact]
    public async Task GetDashboardAsync_DefaultsDateWindow_WhenNotProvided()
    {
        var service = CreateService();

        var dashboard = await service.GetDashboardAsync(new EnterpriseDashboardQueryParameters());

        Assert.True(dashboard.To!.Value >= dashboard.From!.Value);
        Assert.True(dashboard.PeriodEnd!.Value >= dashboard.PeriodStart!.Value);
    }

    [Fact]
    public async Task GetSummaryAsync_SetsDefaultDecisionProfileName_FromRepository()
    {
        _companyDecisionProfileRepository
            .Setup(repository => repository.GetDefaultActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProfile());

        var service = CreateService();

        var summary = await service.GetSummaryAsync(new EnterpriseDashboardQueryParameters());

        Assert.Equal("Balanced Strategy", summary.DefaultDecisionProfileName);
    }

    [Fact]
    public async Task GetSummaryAsync_LeavesDefaultDecisionProfileNameNull_WhenNoneConfigured()
    {
        _companyDecisionProfileRepository
            .Setup(repository => repository.GetDefaultActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CompanyDecisionProfile?)null);

        var service = CreateService();

        var summary = await service.GetSummaryAsync(new EnterpriseDashboardQueryParameters());

        Assert.Null(summary.DefaultDecisionProfileName);
    }

    [Fact]
    public async Task GetSummaryAsync_AppliesTrendFromAggregation()
    {
        _aggregationService
            .SetupSequence(service => service.BuildSummaryAsync(
                It.IsAny<Guid>(),
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardSummaryResponse { RecommendationCount = 10, DecisionCount = 5 })
            .ReturnsAsync(new EnterpriseDashboardSummaryResponse { RecommendationCount = 5, DecisionCount = 5 });

        _trendService
            .Setup(service => service.Calculate(10, 5))
            .Returns(new TrendIndicator { Direction = "Up", DeltaPercent = 100m, CurrentValue = 10, PreviousValue = 5 });
        _trendService
            .Setup(service => service.Calculate(5, 5))
            .Returns(new TrendIndicator { Direction = "Flat", DeltaPercent = 0m, CurrentValue = 5, PreviousValue = 5 });

        var service = CreateService();

        var summary = await service.GetSummaryAsync(new EnterpriseDashboardQueryParameters());

        Assert.Equal("Up", summary.RecommendationTrend.Direction);
        Assert.Equal("Flat", summary.DecisionTrend.Direction);
    }

    [Theory]
    [InlineData(nameof(EnterpriseDashboardService.GetPlanningAsync))]
    [InlineData(nameof(EnterpriseDashboardService.GetPortfolioAsync))]
    [InlineData(nameof(EnterpriseDashboardService.GetCapacityAsync))]
    [InlineData(nameof(EnterpriseDashboardService.GetWorkloadAsync))]
    [InlineData(nameof(EnterpriseDashboardService.GetRecommendationsAsync))]
    [InlineData(nameof(EnterpriseDashboardService.GetDecisionsAsync))]
    [InlineData(nameof(EnterpriseDashboardService.GetAiAsync))]
    [InlineData(nameof(EnterpriseDashboardService.GetAuditAsync))]
    public async Task GetSectionAsync_ResolvesCompanyBeforeDelegating(string methodName)
    {
        var service = CreateService();
        var parameters = new EnterpriseDashboardQueryParameters();

        var method = typeof(EnterpriseDashboardService).GetMethod(methodName);
        Assert.NotNull(method);

        var task = (Task)method!.Invoke(service, [parameters, CancellationToken.None])!;
        await task;

        _companyRepository.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
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

    private static CompanyDecisionProfile CreateProfile() =>
        CompanyDecisionProfile.Create(
            AgencyOSCompanies.DefaultCompanyId,
            "BALANCED",
            "Balanced Strategy",
            null,
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
            ]),
            0.2m,
            0.2m,
            0.2m,
            0.2m,
            0.2m,
            null,
            null,
            null,
            true,
            DateTimeOffset.UtcNow);
}
