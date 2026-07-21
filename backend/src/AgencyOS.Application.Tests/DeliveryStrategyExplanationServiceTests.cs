using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class DeliveryStrategyExplanationServiceTests
{
    private readonly Mock<IDeliveryStrategyRankingService> _deliveryStrategyRankingService = new();
    private readonly Mock<ILogger<DeliveryStrategyExplanationService>> _logger = new();

    private static readonly Guid DecisionProfileId = Guid.Parse("11111111-1111-1111-1111-111111111106");

    private DeliveryStrategyExplanationService CreateService()
    {
        return new DeliveryStrategyExplanationService(
            _deliveryStrategyRankingService.Object,
            _logger.Object);
    }

    [Fact]
    public async Task GetExplanationAsync_ReturnsStructuredExplanationForRankedStrategy()
    {
        var contractId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var strategyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
        var parameters = CreateParameters(contractId, missionId);

        _deliveryStrategyRankingService
            .Setup(service => service.RankAsync(
                It.IsAny<RankDeliveryStrategyRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateRankingResponse(contractId, missionId, strategyId));

        var service = CreateService();
        var response = await service.GetExplanationAsync(strategyId, parameters);

        Assert.Equal(strategyId, response.StrategyId);
        Assert.Equal(contractId, response.ContractId);
        Assert.Equal(missionId, response.MissionId);
        Assert.Equal(1, response.StrategySummary.RankPosition);
        Assert.Single(response.DecisionFactors.Factors);
        Assert.Single(response.Strengths);
        Assert.True(response.ElapsedMilliseconds >= 0);
    }

    [Fact]
    public async Task GetExplanationAsync_ThrowsNotFoundWhenStrategyMissingFromRanking()
    {
        var parameters = CreateParameters(Guid.NewGuid(), Guid.NewGuid());
        var missingStrategyId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        _deliveryStrategyRankingService
            .Setup(service => service.RankAsync(
                It.IsAny<RankDeliveryStrategyRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateRankingResponse(
                parameters.ContractId,
                parameters.MissionId,
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1")));

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetExplanationAsync(missingStrategyId, parameters));
    }

    [Fact]
    public async Task GetExplanationAsync_PropagatesNotFoundFromRanking()
    {
        var parameters = CreateParameters(Guid.NewGuid(), Guid.NewGuid());

        _deliveryStrategyRankingService
            .Setup(service => service.RankAsync(
                It.IsAny<RankDeliveryStrategyRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Company Decision Profile not found."));

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetExplanationAsync(Guid.NewGuid(), parameters));
    }

    private static DeliveryStrategyExplanationQueryParameters CreateParameters(
        Guid contractId,
        Guid missionId)
    {
        return new DeliveryStrategyExplanationQueryParameters
        {
            ContractId = contractId,
            MissionId = missionId,
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 31),
            CompanyDecisionProfileId = DecisionProfileId
        };
    }

    private static RankDeliveryStrategyResponse CreateRankingResponse(
        Guid contractId,
        Guid missionId,
        Guid strategyId)
    {
        return new RankDeliveryStrategyResponse
        {
            ContractId = contractId,
            MissionId = missionId,
            CompanyDecisionProfileId = DecisionProfileId,
            CompanyDecisionProfileName = "Balanced Strategy",
            RankedStrategyCount = 1,
            RankedStrategies =
            [
                new RankedDeliveryStrategyResponse
                {
                    RankPosition = 1,
                    FinalScore = 100m,
                    EvaluatedStrategy = new EvaluatedDeliveryStrategyResponse
                    {
                        Strategy = new DeliveryStrategyResponse
                        {
                            StrategyId = strategyId,
                            StrategyName = "Ranked Strategy",
                            EstimatedCost = 1000m,
                            EstimatedHours = 20m
                        },
                        EvaluationMetrics = new DeliveryStrategyEvaluationMetricsResponse
                        {
                            EstimatedCost = 1000m,
                            EstimatedDurationHours = 20m
                        }
                    },
                    DecisionFactors = new DeliveryStrategyDecisionFactorsResponse
                    {
                        Factors =
                        [
                            new DeliveryStrategyDecisionFactorResponse
                            {
                                Dimension = "EstimatedCost",
                                RawValue = 1000m,
                                NormalizedScore = 100m,
                                Weight = 1m,
                                WeightedContribution = 100m
                            }
                        ]
                    }
                }
            ]
        };
    }
}
