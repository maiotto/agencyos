using AgencyOS.Application.DTOs;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class DeliveryStrategyExplanationCalculationTests
{
    private static readonly Guid ContractId = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc1");
    private static readonly Guid MissionId = Guid.Parse("dddddddd-dddd-dddd-dddd-ddddddddddd2");
    private static readonly Guid DecisionProfileId = Guid.Parse("11111111-1111-1111-1111-111111111106");
    private static readonly Guid StrongStrategyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
    private static readonly Guid WeakStrategyId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2");

    [Fact]
    public void BuildExplanation_IdentifiesStrengthsFromHighDecisionFactorScores()
    {
        var rankingResponse = CreateRankingResponse(
            CreateRankedStrategy(StrongStrategyId, "Strong Strategy", 100m, 1000m, 20m),
            CreateRankedStrategy(WeakStrategyId, "Weak Strategy", 0m, 2000m, 20m));

        var explanation = DeliveryStrategyExplanationCalculation.BuildExplanation(
            rankingResponse.RankedStrategies[0],
            rankingResponse);

        Assert.Single(explanation.Strengths);
        Assert.Equal(RankingDimension.EstimatedCost, explanation.Strengths[0].Dimension);
        Assert.Equal(DeliveryStrategyExplanationCalculation.ReasonStrengthHighDecisionFactorScore, explanation.Strengths[0].ReasonCode);
        Assert.Equal(100m, explanation.Strengths[0].NormalizedScore);
    }

    [Fact]
    public void BuildExplanation_IdentifiesWeaknessesFromLowDecisionFactorScores()
    {
        var rankingResponse = CreateRankingResponse(
            CreateRankedStrategy(StrongStrategyId, "Strong Strategy", 100m, 1000m, 20m),
            CreateRankedStrategy(WeakStrategyId, "Weak Strategy", 0m, 2000m, 20m));

        var explanation = DeliveryStrategyExplanationCalculation.BuildExplanation(
            rankingResponse.RankedStrategies[1],
            rankingResponse);

        Assert.Single(explanation.Weaknesses);
        Assert.Equal(RankingDimension.EstimatedCost, explanation.Weaknesses[0].Dimension);
        Assert.Equal(DeliveryStrategyExplanationCalculation.ReasonWeaknessLowDecisionFactorScore, explanation.Weaknesses[0].ReasonCode);
        Assert.Equal(0m, explanation.Weaknesses[0].NormalizedScore);
    }

    [Fact]
    public void BuildExplanation_IdentifiesRisksFromEvaluationMetrics()
    {
        var rankedStrategy = CreateRankedStrategy(StrongStrategyId, "Risky Strategy", 100m, 1000m, 20m);
        rankedStrategy.EvaluatedStrategy.EvaluationMetrics.OperationalRiskScore = 75m;
        rankedStrategy.EvaluatedStrategy.EvaluationMetrics.CapacityUtilizationPercentage = 90m;
        rankedStrategy.EvaluatedStrategy.EvaluationMetrics.WorkloadImpactPercentage = 85m;
        rankedStrategy.EvaluatedStrategy.EvaluationMetrics.AvailabilityImpactPercentage = 82m;

        var rankingResponse = CreateRankingResponse(rankedStrategy);

        var explanation = DeliveryStrategyExplanationCalculation.BuildExplanation(
            rankedStrategy,
            rankingResponse);

        Assert.Equal(4, explanation.Risks.Count);
        Assert.Contains(explanation.Risks, risk => risk.ReasonCode == DeliveryStrategyExplanationCalculation.ReasonRiskHighOperationalRisk);
        Assert.Contains(explanation.Risks, risk => risk.ReasonCode == DeliveryStrategyExplanationCalculation.ReasonRiskHighCapacityUtilization);
        Assert.Contains(explanation.Risks, risk => risk.ReasonCode == DeliveryStrategyExplanationCalculation.ReasonRiskHighWorkloadImpact);
        Assert.Contains(explanation.Risks, risk => risk.ReasonCode == DeliveryStrategyExplanationCalculation.ReasonRiskHighAvailabilityImpact);
    }

    [Fact]
    public void BuildExplanation_SummarizesResourceCompositionAndCapacityImpact()
    {
        var rankedStrategy = CreateRankedStrategy(StrongStrategyId, "Balanced Strategy", 100m, 1500m, 24m);
        rankedStrategy.EvaluatedStrategy.Strategy.ResourceMix = new DeliveryStrategyResourceMixResponse
        {
            Items =
            [
                new DeliveryStrategyResourceMixItemResponse
                {
                    ResourceType = "Internal Human",
                    ResourceCount = 2,
                    PlannedHours = 16m,
                    PercentageOfHours = 66.67m
                },
                new DeliveryStrategyResourceMixItemResponse
                {
                    ResourceType = "AI",
                    ResourceCount = 1,
                    PlannedHours = 8m,
                    PercentageOfHours = 33.33m
                }
            ]
        };
        rankedStrategy.EvaluatedStrategy.EvaluationMetrics.HumanUtilizationPercentage = 66.67m;
        rankedStrategy.EvaluatedStrategy.EvaluationMetrics.AiUtilizationPercentage = 33.33m;
        rankedStrategy.EvaluatedStrategy.EvaluationMetrics.CapacityUtilizationPercentage = 55m;
        rankedStrategy.EvaluatedStrategy.EvaluationMetrics.WorkloadImpactPercentage = 45m;
        rankedStrategy.EvaluatedStrategy.EvaluationMetrics.AvailabilityImpactPercentage = 35m;
        rankedStrategy.EvaluatedStrategy.EvaluationMetrics.ResourceUtilizationPercentage = 50m;

        var rankingResponse = CreateRankingResponse(rankedStrategy);

        var explanation = DeliveryStrategyExplanationCalculation.BuildExplanation(
            rankedStrategy,
            rankingResponse);

        Assert.Equal(2, explanation.ResourceComposition.Items.Count);
        Assert.Equal(66.67m, explanation.ResourceComposition.HumanUtilizationPercentage);
        Assert.Equal(33.33m, explanation.ResourceComposition.AiUtilizationPercentage);
        Assert.Equal(55m, explanation.CapacityImpact.CapacityUtilizationPercentage);
        Assert.Equal(45m, explanation.CapacityImpact.WorkloadImpactPercentage);
        Assert.Equal(35m, explanation.CapacityImpact.AvailabilityImpactPercentage);
        Assert.Equal(50m, explanation.CapacityImpact.ResourceUtilizationPercentage);
        Assert.Equal(1500m, explanation.EstimatedCost);
        Assert.Equal(24m, explanation.EstimatedDurationHours);
    }

    [Fact]
    public void BuildExplanation_ProducesDeterministicOutput()
    {
        var rankingResponse = CreateRankingResponse(
            CreateRankedStrategy(StrongStrategyId, "Strong Strategy", 100m, 1000m, 20m),
            CreateRankedStrategy(WeakStrategyId, "Weak Strategy", 0m, 2000m, 20m));

        var firstExplanation = DeliveryStrategyExplanationCalculation.BuildExplanation(
            rankingResponse.RankedStrategies[0],
            rankingResponse);
        var secondExplanation = DeliveryStrategyExplanationCalculation.BuildExplanation(
            rankingResponse.RankedStrategies[0],
            rankingResponse);

        Assert.Equal(firstExplanation.Strengths.Count, secondExplanation.Strengths.Count);
        Assert.Equal(firstExplanation.Weaknesses.Count, secondExplanation.Weaknesses.Count);
        Assert.Equal(firstExplanation.Risks.Count, secondExplanation.Risks.Count);
        Assert.Equal(firstExplanation.EstimatedCost, secondExplanation.EstimatedCost);
        Assert.Equal(firstExplanation.StrategySummary.FinalScore, secondExplanation.StrategySummary.FinalScore);
    }

    private static RankDeliveryStrategyResponse CreateRankingResponse(params RankedDeliveryStrategyResponse[] rankedStrategies)
    {
        for (var index = 0; index < rankedStrategies.Length; index++)
        {
            rankedStrategies[index].RankPosition = index + 1;
        }

        return new RankDeliveryStrategyResponse
        {
            ContractId = ContractId,
            MissionId = MissionId,
            CompanyDecisionProfileId = DecisionProfileId,
            CompanyDecisionProfileName = "Balanced Strategy",
            RankedStrategies = rankedStrategies,
            RankedStrategyCount = rankedStrategies.Length
        };
    }

    private static RankedDeliveryStrategyResponse CreateRankedStrategy(
        Guid strategyId,
        string strategyName,
        decimal normalizedScore,
        decimal estimatedCost,
        decimal estimatedDurationHours)
    {
        return new RankedDeliveryStrategyResponse
        {
            FinalScore = normalizedScore,
            EvaluatedStrategy = new EvaluatedDeliveryStrategyResponse
            {
                Strategy = new DeliveryStrategyResponse
                {
                    StrategyId = strategyId,
                    ContractId = ContractId,
                    MissionId = MissionId,
                    StrategyName = strategyName,
                    EstimatedCost = estimatedCost,
                    EstimatedHours = estimatedDurationHours
                },
                EvaluationMetrics = new DeliveryStrategyEvaluationMetricsResponse
                {
                    EstimatedCost = estimatedCost,
                    EstimatedDurationHours = estimatedDurationHours
                }
            },
            DecisionFactors = new DeliveryStrategyDecisionFactorsResponse
            {
                Factors =
                [
                    new DeliveryStrategyDecisionFactorResponse
                    {
                        Dimension = RankingDimension.EstimatedCost,
                        RawValue = estimatedCost,
                        NormalizedScore = normalizedScore,
                        Weight = 1m,
                        WeightedContribution = normalizedScore
                    }
                ]
            }
        };
    }
}
