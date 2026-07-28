using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Moq;

namespace AgencyOS.Application.Tests;

public class PortfolioComparisonServiceTests
{
    private static readonly Guid CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

    private readonly Mock<IPortfolioRepository> _portfolioRepository = new();
    private readonly Mock<IRecommendationRepository> _recommendationRepository = new();
    private readonly Mock<IDecisionRepository> _decisionRepository = new();

    public PortfolioComparisonServiceTests()
    {
        _recommendationRepository
            .Setup(repository => repository.GetByMissionIdAsync(It.IsAny<Guid>(), It.IsAny<RecommendationQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Recommendation>());
        _decisionRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<DecisionQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Decision>());
    }

    private PortfolioComparisonService CreateService() =>
        new(
            _portfolioRepository.Object,
            _recommendationRepository.Object,
            _decisionRepository.Object,
            new DashboardHealthCalculationService());

    [Fact]
    public async Task CompareAsync_ThrowsBusinessRule_WhenIdsAreEqual()
    {
        var portfolioId = Guid.NewGuid();
        var service = CreateService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CompareAsync(
            CompanyId,
            new PortfolioCompareQueryParameters { LeftPortfolioId = portfolioId, RightPortfolioId = portfolioId }));
    }

    [Fact]
    public async Task CompareAsync_ThrowsBusinessRule_WhenEitherIdIsEmpty()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CompareAsync(
            CompanyId,
            new PortfolioCompareQueryParameters { LeftPortfolioId = Guid.Empty, RightPortfolioId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task CompareAsync_ThrowsNotFound_WhenLeftPortfolioMissing()
    {
        var left = Guid.NewGuid();
        var right = Guid.NewGuid();

        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(left, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Portfolio?)null);
        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(right, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePortfolio(Guid.NewGuid()));

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.CompareAsync(
            CompanyId,
            new PortfolioCompareQueryParameters { LeftPortfolioId = left, RightPortfolioId = right }));
    }

    [Fact]
    public async Task CompareAsync_ThrowsBusinessRule_WhenPortfolioBelongsToDifferentCompany()
    {
        var left = CreatePortfolio(Guid.NewGuid(), companyId: Guid.NewGuid());
        var right = CreatePortfolio(Guid.NewGuid());

        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(left.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(left);
        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(right.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(right);

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CompareAsync(
            CompanyId,
            new PortfolioCompareQueryParameters { LeftPortfolioId = left.Id, RightPortfolioId = right.Id }));
    }

    [Fact]
    public async Task CompareAsync_ReturnsBothSidesAndFieldDiffs()
    {
        var leftMissionId = Guid.NewGuid();
        var rightMissionId = Guid.NewGuid();
        var left = CreatePortfolio(leftMissionId, utilization: 60m, workload: 55m);
        var right = CreatePortfolio(rightMissionId, utilization: 90m, workload: 88m);

        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(left.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(left);
        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(right.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(right);

        var leftRecommendation = CreateRecommendation(leftMissionId, score: 70m);
        _recommendationRepository
            .Setup(repository => repository.GetByMissionIdAsync(leftMissionId, It.IsAny<RecommendationQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([leftRecommendation]);

        var service = CreateService();

        var comparison = await service.CompareAsync(
            CompanyId,
            new PortfolioCompareQueryParameters { LeftPortfolioId = left.Id, RightPortfolioId = right.Id });

        Assert.Equal(CompanyId, comparison.CompanyId);
        Assert.Equal(left.Id, comparison.Left.PortfolioId);
        Assert.Equal(right.Id, comparison.Right.PortfolioId);
        Assert.Equal(60m, comparison.Left.UtilizationPercentage);
        Assert.Equal(90m, comparison.Right.UtilizationPercentage);
        Assert.Equal(1, comparison.Left.RecommendationCount);
        Assert.Equal(0, comparison.Right.RecommendationCount);

        var utilizationDiff = Assert.Single(comparison.FieldDiffs, diff => diff.Field == "UtilizationPercentage");
        Assert.Equal(30m, utilizationDiff.Delta);
    }

    private static Portfolio CreatePortfolio(
        Guid missionId,
        decimal utilization = 50m,
        decimal workload = 50m,
        Guid? companyId = null)
    {
        var portfolio = Portfolio.Create(
            companyId ?? CompanyId,
            $"Portfolio {Guid.NewGuid():N}",
            null,
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime),
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddDays(30).UtcDateTime),
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
            CompanyId,
            missionId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            $"REC-{Guid.NewGuid():N}",
            "Comparison Recommendation",
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
