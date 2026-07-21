using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

public static class DeliveryStrategyRankingCalculation
{
    public static IReadOnlyList<RankedDeliveryStrategyResponse> RankStrategies(
        IReadOnlyList<EvaluatedDeliveryStrategyResponse> evaluatedStrategies,
        CompanyDecisionProfile decisionProfile)
    {
        var activeDimensions = decisionProfile.Dimensions
            .Where(dimension => dimension.Weight > 0)
            .ToList();

        if (activeDimensions.Count == 0)
        {
            throw new InvalidOperationException(
                "Company Decision Profile must define at least one dimension with a positive weight.");
        }

        var normalizedScoresByStrategy = evaluatedStrategies
            .ToDictionary(
                strategy => strategy.Strategy.StrategyId,
                strategy => new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase));

        foreach (var dimensionSetting in activeDimensions)
        {
            var rawValues = evaluatedStrategies
                .Select(strategy => new
                {
                    StrategyId = strategy.Strategy.StrategyId,
                    Value = GetDimensionValue(strategy.EvaluationMetrics, dimensionSetting.Dimension)
                })
                .ToList();

            var minValue = rawValues.Min(entry => entry.Value);
            var maxValue = rawValues.Max(entry => entry.Value);
            var valueRange = maxValue - minValue;

            foreach (var entry in rawValues)
            {
                var normalizedScore = valueRange <= 0
                    ? 100m
                    : dimensionSetting.PreferHigherValues
                        ? (entry.Value - minValue) / valueRange * 100m
                        : (maxValue - entry.Value) / valueRange * 100m;

                normalizedScoresByStrategy[entry.StrategyId][dimensionSetting.Dimension] = decimal.Round(
                    normalizedScore,
                    2,
                    MidpointRounding.AwayFromZero);
            }
        }

        var rankedStrategies = evaluatedStrategies
            .Select(strategy =>
            {
                var factors = activeDimensions
                    .Select(dimensionSetting =>
                    {
                        var rawValue = GetDimensionValue(
                            strategy.EvaluationMetrics,
                            dimensionSetting.Dimension);
                        var normalizedScore = normalizedScoresByStrategy[strategy.Strategy.StrategyId][
                            dimensionSetting.Dimension];
                        var weightedContribution = decimal.Round(
                            normalizedScore * dimensionSetting.Weight,
                            2,
                            MidpointRounding.AwayFromZero);

                        return new DeliveryStrategyDecisionFactorResponse
                        {
                            Dimension = dimensionSetting.Dimension,
                            RawValue = rawValue,
                            NormalizedScore = normalizedScore,
                            Weight = dimensionSetting.Weight,
                            WeightedContribution = weightedContribution
                        };
                    })
                    .ToList();

                var totalWeight = activeDimensions.Sum(dimension => dimension.Weight);
                var finalScore = totalWeight <= 0
                    ? 0m
                    : decimal.Round(
                        factors.Sum(factor => factor.WeightedContribution) / totalWeight,
                        2,
                        MidpointRounding.AwayFromZero);

                return new RankedDeliveryStrategyResponse
                {
                    FinalScore = finalScore,
                    EvaluatedStrategy = strategy,
                    DecisionFactors = new DeliveryStrategyDecisionFactorsResponse
                    {
                        Factors = factors
                    }
                };
            })
            .OrderByDescending(result => result.FinalScore)
            .ThenBy(result => result.EvaluatedStrategy.Strategy.StrategyName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(result => result.EvaluatedStrategy.Strategy.StrategyId)
            .ToList();

        for (var index = 0; index < rankedStrategies.Count; index++)
        {
            rankedStrategies[index].RankPosition = index + 1;
        }

        return rankedStrategies;
    }

    private static decimal GetDimensionValue(
        DeliveryStrategyEvaluationMetricsResponse metrics,
        string dimension)
    {
        return dimension switch
        {
            RankingDimension.EstimatedCost => metrics.EstimatedCost,
            RankingDimension.EstimatedDuration => metrics.EstimatedDurationHours,
            RankingDimension.CapacityUtilization => metrics.CapacityUtilizationPercentage,
            RankingDimension.OperationalRisk => metrics.OperationalRiskScore,
            RankingDimension.HumanResourceUsage => metrics.HumanUtilizationPercentage,
            RankingDimension.AiResourceUsage => metrics.AiUtilizationPercentage,
            RankingDimension.ExternalResourceUsage => metrics.ExternalResourceUtilizationPercentage,
            RankingDimension.AutomationUsage => metrics.AutomationUtilizationPercentage,
            _ => throw new ArgumentOutOfRangeException(
                nameof(dimension),
                dimension,
                "Unsupported ranking dimension.")
        };
    }
}
