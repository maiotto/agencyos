using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class DeliveryStrategyRankingServiceTests
{
    private readonly Mock<IDeliveryStrategyEvaluatorService> _deliveryStrategyEvaluatorService = new();
    private readonly Mock<ICompanyDecisionProfileRepository> _companyDecisionProfileRepository = new();
    private readonly Mock<IRecommendationService> _recommendationService = new();
    private readonly Mock<ILogger<DeliveryStrategyRankingService>> _logger = new();

    private static readonly Guid DecisionProfileId = Guid.Parse("11111111-1111-1111-1111-111111111106");
    private static readonly Guid CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

    private DeliveryStrategyRankingService CreateService()
    {
        _recommendationService
            .Setup(service => service.PersistRankedStrategiesAsync(
                It.IsAny<RankDeliveryStrategyRequest>(),
                It.IsAny<RankDeliveryStrategyResponse>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RankDeliveryStrategyRequest _, RankDeliveryStrategyResponse ranking, CancellationToken _) =>
                ranking.RankedStrategies.Select(ranked =>
                    Recommendation.Create(
                        CompanyId,
                        ranking.MissionId,
                        ranking.ContractId,
                        ranked.EvaluatedStrategy.Strategy.StrategyId,
                        $"REC-{ranked.EvaluatedStrategy.Strategy.StrategyId:N}",
                        ranked.EvaluatedStrategy.Strategy.StrategyName,
                        "summary",
                        "reason",
                        ranked.FinalScore,
                        ranked.RankPosition,
                        1,
                        RecommendationVersions.CurrentDecisionEngineVersion,
                        null,
                        "{}",
                        "{}",
                        "{\"ok\":true}",
                        "decision-engine",
                        DateTimeOffset.UtcNow)).ToList());

        return new DeliveryStrategyRankingService(
            _deliveryStrategyEvaluatorService.Object,
            _companyDecisionProfileRepository.Object,
            _recommendationService.Object,
            _logger.Object);
    }

    [Fact]
    public async Task RankAsync_RanksEvaluatedStrategiesUsingDecisionProfile()
    {
        var contractId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var cheaperStrategyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
        var expensiveStrategyId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2");
        var request = CreateRequest(contractId, missionId);

        _companyDecisionProfileRepository
            .Setup(repository => repository.GetByIdAsync(DecisionProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateBalancedProfile());

        _deliveryStrategyEvaluatorService
            .Setup(service => service.EvaluateAsync(
                It.IsAny<EvaluateDeliveryStrategyRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EvaluateDeliveryStrategyResponse
            {
                ContractId = contractId,
                MissionId = missionId,
                EvaluatedStrategyCount = 2,
                EvaluatedStrategies =
                [
                    CreateEvaluatedStrategy(expensiveStrategyId, "Expensive Strategy", 2000m),
                    CreateEvaluatedStrategy(cheaperStrategyId, "Cheaper Strategy", 1000m)
                ]
            });

        var service = CreateService();
        var response = await service.RankAsync(request);

        Assert.Equal(2, response.RankedStrategyCount);
        Assert.Equal("Balanced Strategy", response.CompanyDecisionProfileName);
        Assert.Equal(cheaperStrategyId, response.RankedStrategies[0].EvaluatedStrategy.Strategy.StrategyId);
        Assert.Equal(1, response.RankedStrategies[0].RankPosition);
        Assert.NotEqual(Guid.Empty, response.RankedStrategies[0].RecommendationId);
        Assert.True(response.RankedStrategies[0].FinalScore > response.RankedStrategies[1].FinalScore);
        _recommendationService.Verify(
            service => service.PersistRankedStrategiesAsync(
                It.IsAny<RankDeliveryStrategyRequest>(),
                It.IsAny<RankDeliveryStrategyResponse>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RankAsync_ThrowsNotFoundWhenDecisionProfileMissing()
    {
        var request = CreateRequest(Guid.NewGuid(), Guid.NewGuid());

        _companyDecisionProfileRepository
            .Setup(repository => repository.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CompanyDecisionProfile?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.RankAsync(request));
    }

    [Fact]
    public async Task RankAsync_ThrowsBusinessRuleWhenNoEvaluatedStrategies()
    {
        var request = CreateRequest(Guid.NewGuid(), Guid.NewGuid());

        _companyDecisionProfileRepository
            .Setup(repository => repository.GetByIdAsync(DecisionProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateBalancedProfile());

        _deliveryStrategyEvaluatorService
            .Setup(service => service.EvaluateAsync(
                It.IsAny<EvaluateDeliveryStrategyRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EvaluateDeliveryStrategyResponse
            {
                EvaluatedStrategyCount = 0,
                EvaluatedStrategies = []
            });

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.RankAsync(request));
    }

    [Fact]
    public async Task RankAsync_PropagatesNotFoundFromEvaluator()
    {
        var request = CreateRequest(Guid.NewGuid(), Guid.NewGuid());

        _companyDecisionProfileRepository
            .Setup(repository => repository.GetByIdAsync(DecisionProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateBalancedProfile());

        _deliveryStrategyEvaluatorService
            .Setup(service => service.EvaluateAsync(
                It.IsAny<EvaluateDeliveryStrategyRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Contract not found."));

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.RankAsync(request));
    }

    private static RankDeliveryStrategyRequest CreateRequest(Guid contractId, Guid missionId)
    {
        return new RankDeliveryStrategyRequest
        {
            CompanyId = CompanyId,
            ContractId = contractId,
            MissionId = missionId,
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 31),
            CompanyDecisionProfileId = DecisionProfileId,
            GeneratedBy = "decision-engine"
        };
    }

    private static CompanyDecisionProfile CreateBalancedProfile() =>
        CompanyDecisionProfile.CreateRankingView(
            DecisionProfileId,
            "BalancedStrategy",
            "Balanced Strategy",
            [
                new DecisionProfileDimensionSetting
                {
                    Dimension = RankingDimension.EstimatedCost,
                    Weight = 1m,
                    PreferHigherValues = false
                }
            ]);

    private static EvaluatedDeliveryStrategyResponse CreateEvaluatedStrategy(
        Guid strategyId,
        string strategyName,
        decimal estimatedCost)
    {
        return new EvaluatedDeliveryStrategyResponse
        {
            Strategy = new DeliveryStrategyResponse
            {
                StrategyId = strategyId,
                StrategyName = strategyName,
                EstimatedCost = estimatedCost,
                EstimatedHours = 20m
            },
            EvaluationMetrics = new DeliveryStrategyEvaluationMetricsResponse
            {
                EstimatedCost = estimatedCost,
                EstimatedDurationHours = 20m
            }
        };
    }
}
