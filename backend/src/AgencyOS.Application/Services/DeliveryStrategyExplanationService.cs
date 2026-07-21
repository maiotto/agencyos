using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AgencyOS.Application.Services;

public class DeliveryStrategyExplanationService : IDeliveryStrategyExplanationService
{
    private readonly IDeliveryStrategyRankingService _deliveryStrategyRankingService;
    private readonly ILogger<DeliveryStrategyExplanationService> _logger;

    public DeliveryStrategyExplanationService(
        IDeliveryStrategyRankingService deliveryStrategyRankingService,
        ILogger<DeliveryStrategyExplanationService> logger)
    {
        _deliveryStrategyRankingService = deliveryStrategyRankingService;
        _logger = logger;
    }

    public async Task<DeliveryStrategyExplanationResponse> GetExplanationAsync(
        Guid strategyId,
        DeliveryStrategyExplanationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Delivery strategy explanation generation started for Strategy {StrategyId}, Contract {ContractId}, Mission {MissionId}, and Decision Profile {DecisionProfileId}",
            strategyId,
            parameters.ContractId,
            parameters.MissionId,
            parameters.CompanyDecisionProfileId);

        try
        {
            var rankingRequest = new RankDeliveryStrategyRequest
            {
                ContractId = parameters.ContractId,
                MissionId = parameters.MissionId,
                PeriodStartDate = parameters.PeriodStartDate,
                PeriodEndDate = parameters.PeriodEndDate,
                CompanyDecisionProfileId = parameters.CompanyDecisionProfileId
            };

            var rankingResponse = await _deliveryStrategyRankingService.RankAsync(
                rankingRequest,
                cancellationToken);

            var rankedStrategy = rankingResponse.RankedStrategies
                .FirstOrDefault(strategy =>
                    strategy.EvaluatedStrategy.Strategy.StrategyId == strategyId);

            if (rankedStrategy is null)
            {
                throw new NotFoundException("Delivery strategy not found.");
            }

            var explanation = DeliveryStrategyExplanationCalculation.BuildExplanation(
                rankedStrategy,
                rankingResponse);

            stopwatch.Stop();
            explanation.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Delivery strategy explanation generated for Strategy {StrategyId} at Rank {RankPosition} with {StrengthCount} strengths, {WeaknessCount} weaknesses, and {RiskCount} risks in {ElapsedMilliseconds} ms",
                strategyId,
                rankedStrategy.RankPosition,
                explanation.Strengths.Count,
                explanation.Weaknesses.Count,
                explanation.Risks.Count,
                stopwatch.ElapsedMilliseconds);

            return explanation;
        }
        catch (Exception ex) when (ex is not BusinessRuleException and not NotFoundException)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Delivery strategy explanation generation failed for Strategy {StrategyId}, Contract {ContractId}, Mission {MissionId}, and Decision Profile {DecisionProfileId} after {ElapsedMilliseconds} ms",
                strategyId,
                parameters.ContractId,
                parameters.MissionId,
                parameters.CompanyDecisionProfileId,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
