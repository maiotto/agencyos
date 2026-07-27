using AgencyOS.Application.DTOs;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class DeliveryStrategyRankingCalculationTests
{
    private static readonly CompanyDecisionProfile ProfitMaximizationProfile =
        CompanyDecisionProfile.CreateRankingView(
            Guid.Parse("11111111-1111-1111-1111-111111111101"),
            "ProfitMaximization",
            "Profit Maximization",
            [
                new DecisionProfileDimensionSetting
                {
                    Dimension = RankingDimension.EstimatedCost,
                    Weight = 1m,
                    PreferHigherValues = false
                }
            ]);

    [Fact]
    public void RankStrategies_OrdersByWeightedScoreDescending()
    {
        var cheaperStrategyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
        var expensiveStrategyId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2");

        var evaluatedStrategies = new List<EvaluatedDeliveryStrategyResponse>
        {
            CreateEvaluatedStrategy(expensiveStrategyId, "Expensive Strategy", 2000m, 20m),
            CreateEvaluatedStrategy(cheaperStrategyId, "Cheaper Strategy", 1000m, 20m)
        };

        var rankedStrategies = DeliveryStrategyRankingCalculation.RankStrategies(
            evaluatedStrategies,
            ProfitMaximizationProfile);

        Assert.Equal(2, rankedStrategies.Count);
        Assert.Equal(1, rankedStrategies[0].RankPosition);
        Assert.Equal(cheaperStrategyId, rankedStrategies[0].EvaluatedStrategy.Strategy.StrategyId);
        Assert.Equal(100m, rankedStrategies[0].FinalScore);
        Assert.Equal(2, rankedStrategies[1].RankPosition);
        Assert.Equal(expensiveStrategyId, rankedStrategies[1].EvaluatedStrategy.Strategy.StrategyId);
        Assert.Equal(0m, rankedStrategies[1].FinalScore);
    }

    [Fact]
    public void RankStrategies_ProducesDeterministicTieBreakerByStrategyName()
    {
        var alphaStrategyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
        var betaStrategyId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2");

        var evaluatedStrategies = new List<EvaluatedDeliveryStrategyResponse>
        {
            CreateEvaluatedStrategy(betaStrategyId, "Beta Strategy", 1000m, 20m),
            CreateEvaluatedStrategy(alphaStrategyId, "Alpha Strategy", 1000m, 20m)
        };

        var firstRanking = DeliveryStrategyRankingCalculation.RankStrategies(
            evaluatedStrategies,
            ProfitMaximizationProfile);
        var secondRanking = DeliveryStrategyRankingCalculation.RankStrategies(
            evaluatedStrategies,
            ProfitMaximizationProfile);

        Assert.Equal(firstRanking[0].EvaluatedStrategy.Strategy.StrategyId, secondRanking[0].EvaluatedStrategy.Strategy.StrategyId);
        Assert.Equal("Alpha Strategy", firstRanking[0].EvaluatedStrategy.Strategy.StrategyName);
        Assert.Equal(100m, firstRanking[0].FinalScore);
        Assert.Equal(100m, firstRanking[1].FinalScore);
    }

    [Fact]
    public void RankStrategies_ReturnsDecisionFactorsForActiveDimensions()
    {
        var strategyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
        var evaluatedStrategies = new List<EvaluatedDeliveryStrategyResponse>
        {
            CreateEvaluatedStrategy(strategyId, "Single Strategy", 1000m, 20m)
        };

        var rankedStrategies = DeliveryStrategyRankingCalculation.RankStrategies(
            evaluatedStrategies,
            ProfitMaximizationProfile);

        var factors = rankedStrategies[0].DecisionFactors.Factors;
        Assert.Single(factors);
        Assert.Equal(RankingDimension.EstimatedCost, factors[0].Dimension);
        Assert.Equal(1000m, factors[0].RawValue);
        Assert.Equal(100m, factors[0].NormalizedScore);
        Assert.Equal(1m, factors[0].Weight);
        Assert.Equal(100m, factors[0].WeightedContribution);
    }

    [Fact]
    public void RankStrategies_RespectsDecisionProfileWeights()
    {
        var aiStrategyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
        var humanStrategyId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2");

        var aiAdoptionProfile = CompanyDecisionProfile.CreateRankingView(
            Guid.Parse("11111111-1111-1111-1111-111111111104"),
            "AiAdoption",
            "AI Adoption",
            [
                new DecisionProfileDimensionSetting
                {
                    Dimension = RankingDimension.AiResourceUsage,
                    Weight = 1m,
                    PreferHigherValues = true
                }
            ]);

        var evaluatedStrategies = new List<EvaluatedDeliveryStrategyResponse>
        {
            CreateEvaluatedStrategy(humanStrategyId, "Human Strategy", 1000m, 20m, humanUtilization: 100m),
            CreateEvaluatedStrategy(aiStrategyId, "AI Strategy", 1000m, 20m, aiUtilization: 100m)
        };

        var rankedStrategies = DeliveryStrategyRankingCalculation.RankStrategies(
            evaluatedStrategies,
            aiAdoptionProfile);

        Assert.Equal(aiStrategyId, rankedStrategies[0].EvaluatedStrategy.Strategy.StrategyId);
        Assert.Equal(100m, rankedStrategies[0].FinalScore);
        Assert.Equal(0m, rankedStrategies[1].FinalScore);
    }

    private static EvaluatedDeliveryStrategyResponse CreateEvaluatedStrategy(
        Guid strategyId,
        string strategyName,
        decimal estimatedCost,
        decimal estimatedDurationHours,
        decimal humanUtilization = 0m,
        decimal aiUtilization = 0m)
    {
        return new EvaluatedDeliveryStrategyResponse
        {
            Strategy = new DeliveryStrategyResponse
            {
                StrategyId = strategyId,
                StrategyName = strategyName,
                EstimatedCost = estimatedCost,
                EstimatedHours = estimatedDurationHours
            },
            EvaluationMetrics = new DeliveryStrategyEvaluationMetricsResponse
            {
                EstimatedCost = estimatedCost,
                EstimatedDurationHours = estimatedDurationHours,
                HumanUtilizationPercentage = humanUtilization,
                AiUtilizationPercentage = aiUtilization
            }
        };
    }
}
