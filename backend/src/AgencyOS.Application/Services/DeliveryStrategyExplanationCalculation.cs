using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

public static class DeliveryStrategyExplanationCalculation
{
    public const string CategoryDecisionFactor = "DecisionFactor";

    public const string ReasonStrengthHighDecisionFactorScore = "STRENGTH_HIGH_DECISION_FACTOR_SCORE";

    public const string ReasonWeaknessLowDecisionFactorScore = "WEAKNESS_LOW_DECISION_FACTOR_SCORE";

    public const string ReasonRiskHighOperationalRisk = "RISK_HIGH_OPERATIONAL_RISK";

    public const string ReasonRiskHighCapacityUtilization = "RISK_HIGH_CAPACITY_UTILIZATION";

    public const string ReasonRiskHighWorkloadImpact = "RISK_HIGH_WORKLOAD_IMPACT";

    public const string ReasonRiskHighAvailabilityImpact = "RISK_HIGH_AVAILABILITY_IMPACT";

    private const decimal StrongNormalizedScoreThreshold = 70m;

    private const decimal WeakNormalizedScoreThreshold = 40m;

    private const decimal HighOperationalRiskThreshold = 50m;

    private const decimal HighCapacityUtilizationThreshold = 80m;

    private const decimal HighImpactThreshold = 80m;

    public static DeliveryStrategyExplanationResponse BuildExplanation(
        RankedDeliveryStrategyResponse rankedStrategy,
        RankDeliveryStrategyResponse rankingResponse)
    {
        var strategy = rankedStrategy.EvaluatedStrategy.Strategy;
        var metrics = rankedStrategy.EvaluatedStrategy.EvaluationMetrics;

        return new DeliveryStrategyExplanationResponse
        {
            StrategyId = strategy.StrategyId,
            ContractId = rankingResponse.ContractId,
            MissionId = rankingResponse.MissionId,
            CompanyDecisionProfileId = rankingResponse.CompanyDecisionProfileId,
            StrategySummary = new DeliveryStrategySummaryResponse
            {
                StrategyId = strategy.StrategyId,
                StrategyName = strategy.StrategyName,
                RankPosition = rankedStrategy.RankPosition,
                TotalRankedStrategies = rankingResponse.RankedStrategyCount,
                FinalScore = rankedStrategy.FinalScore,
                CompanyDecisionProfileName = rankingResponse.CompanyDecisionProfileName
            },
            DecisionFactors = rankedStrategy.DecisionFactors,
            Strengths = IdentifyStrengths(rankedStrategy.DecisionFactors.Factors),
            Weaknesses = IdentifyWeaknesses(rankedStrategy.DecisionFactors.Factors),
            Risks = IdentifyRisks(metrics),
            ResourceComposition = BuildResourceComposition(strategy, metrics),
            CapacityImpact = BuildCapacityImpact(metrics),
            EstimatedCost = metrics.EstimatedCost,
            EstimatedDurationHours = metrics.EstimatedDurationHours
        };
    }

    private static IReadOnlyList<DeliveryStrategyExplanationInsightResponse> IdentifyStrengths(
        IReadOnlyList<DeliveryStrategyDecisionFactorResponse> factors)
    {
        return factors
            .Where(factor => factor.NormalizedScore >= StrongNormalizedScoreThreshold)
            .OrderByDescending(factor => factor.WeightedContribution)
            .ThenBy(factor => factor.Dimension, StringComparer.OrdinalIgnoreCase)
            .Select(factor => new DeliveryStrategyExplanationInsightResponse
            {
                Category = CategoryDecisionFactor,
                Dimension = factor.Dimension,
                NormalizedScore = factor.NormalizedScore,
                RawValue = factor.RawValue,
                ReasonCode = ReasonStrengthHighDecisionFactorScore
            })
            .ToList();
    }

    private static IReadOnlyList<DeliveryStrategyExplanationInsightResponse> IdentifyWeaknesses(
        IReadOnlyList<DeliveryStrategyDecisionFactorResponse> factors)
    {
        return factors
            .Where(factor => factor.NormalizedScore < WeakNormalizedScoreThreshold)
            .OrderBy(factor => factor.NormalizedScore)
            .ThenBy(factor => factor.Dimension, StringComparer.OrdinalIgnoreCase)
            .Select(factor => new DeliveryStrategyExplanationInsightResponse
            {
                Category = CategoryDecisionFactor,
                Dimension = factor.Dimension,
                NormalizedScore = factor.NormalizedScore,
                RawValue = factor.RawValue,
                ReasonCode = ReasonWeaknessLowDecisionFactorScore
            })
            .ToList();
    }

    private static IReadOnlyList<DeliveryStrategyExplanationRiskResponse> IdentifyRisks(
        DeliveryStrategyEvaluationMetricsResponse metrics)
    {
        var risks = new List<DeliveryStrategyExplanationRiskResponse>();

        if (metrics.OperationalRiskScore >= HighOperationalRiskThreshold)
        {
            risks.Add(new DeliveryStrategyExplanationRiskResponse
            {
                Metric = RankingDimension.OperationalRisk,
                Value = metrics.OperationalRiskScore,
                ReasonCode = ReasonRiskHighOperationalRisk
            });
        }

        if (metrics.CapacityUtilizationPercentage >= HighCapacityUtilizationThreshold)
        {
            risks.Add(new DeliveryStrategyExplanationRiskResponse
            {
                Metric = RankingDimension.CapacityUtilization,
                Value = metrics.CapacityUtilizationPercentage,
                ReasonCode = ReasonRiskHighCapacityUtilization
            });
        }

        if (metrics.WorkloadImpactPercentage >= HighImpactThreshold)
        {
            risks.Add(new DeliveryStrategyExplanationRiskResponse
            {
                Metric = "WorkloadImpact",
                Value = metrics.WorkloadImpactPercentage,
                ReasonCode = ReasonRiskHighWorkloadImpact
            });
        }

        if (metrics.AvailabilityImpactPercentage >= HighImpactThreshold)
        {
            risks.Add(new DeliveryStrategyExplanationRiskResponse
            {
                Metric = "AvailabilityImpact",
                Value = metrics.AvailabilityImpactPercentage,
                ReasonCode = ReasonRiskHighAvailabilityImpact
            });
        }

        return risks
            .OrderBy(risk => risk.Metric, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static DeliveryStrategyResourceCompositionExplanationResponse BuildResourceComposition(
        DeliveryStrategyResponse strategy,
        DeliveryStrategyEvaluationMetricsResponse metrics)
    {
        return new DeliveryStrategyResourceCompositionExplanationResponse
        {
            Items = strategy.ResourceMix.Items,
            HumanUtilizationPercentage = metrics.HumanUtilizationPercentage,
            AiUtilizationPercentage = metrics.AiUtilizationPercentage,
            ExternalResourceUtilizationPercentage = metrics.ExternalResourceUtilizationPercentage,
            AutomationUtilizationPercentage = metrics.AutomationUtilizationPercentage
        };
    }

    private static DeliveryStrategyCapacityImpactExplanationResponse BuildCapacityImpact(
        DeliveryStrategyEvaluationMetricsResponse metrics)
    {
        return new DeliveryStrategyCapacityImpactExplanationResponse
        {
            CapacityUtilizationPercentage = metrics.CapacityUtilizationPercentage,
            WorkloadImpactPercentage = metrics.WorkloadImpactPercentage,
            AvailabilityImpactPercentage = metrics.AvailabilityImpactPercentage,
            ResourceUtilizationPercentage = metrics.ResourceUtilizationPercentage
        };
    }
}
