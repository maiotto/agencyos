using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AgencyOS.Application.Services;

public class DeliveryStrategyRankingService : IDeliveryStrategyRankingService
{
    private readonly IDeliveryStrategyEvaluatorService _deliveryStrategyEvaluatorService;
    private readonly ICompanyDecisionProfileRepository _companyDecisionProfileRepository;
    private readonly IRecommendationService _recommendationService;
    private readonly ILogger<DeliveryStrategyRankingService> _logger;

    public DeliveryStrategyRankingService(
        IDeliveryStrategyEvaluatorService deliveryStrategyEvaluatorService,
        ICompanyDecisionProfileRepository companyDecisionProfileRepository,
        IRecommendationService recommendationService,
        ILogger<DeliveryStrategyRankingService> logger)
    {
        _deliveryStrategyEvaluatorService = deliveryStrategyEvaluatorService;
        _companyDecisionProfileRepository = companyDecisionProfileRepository;
        _recommendationService = recommendationService;
        _logger = logger;
    }

    public async Task<RankDeliveryStrategyResponse> RankAsync(
        RankDeliveryStrategyRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Delivery strategy ranking started for Contract {ContractId}, Mission {MissionId}, and Decision Profile {DecisionProfileId}",
            request.ContractId,
            request.MissionId,
            request.CompanyDecisionProfileId);

        try
        {
            var decisionProfile = await _companyDecisionProfileRepository.GetByIdAsync(
                request.CompanyDecisionProfileId,
                cancellationToken);

            if (decisionProfile is null)
            {
                throw new NotFoundException("Company Decision Profile not found.");
            }

            if (!decisionProfile.IsActive)
            {
                throw new BusinessRuleException(
                    "Company Decision Profile must be Active to be used for ranking (BR-1904).");
            }

            var evaluationRequest = new EvaluateDeliveryStrategyRequest
            {
                ContractId = request.ContractId,
                MissionId = request.MissionId,
                PeriodStartDate = request.PeriodStartDate,
                PeriodEndDate = request.PeriodEndDate
            };

            var evaluationResponse = await _deliveryStrategyEvaluatorService.EvaluateAsync(
                evaluationRequest,
                cancellationToken);

            if (evaluationResponse.EvaluatedStrategies.Count == 0)
            {
                throw new BusinessRuleException(
                    "At least one evaluated delivery strategy is required for ranking.");
            }

            var rankedStrategies = DeliveryStrategyRankingCalculation.RankStrategies(
                evaluationResponse.EvaluatedStrategies,
                decisionProfile);

            var response = new RankDeliveryStrategyResponse
            {
                ContractId = request.ContractId,
                MissionId = request.MissionId,
                CompanyDecisionProfileId = decisionProfile.Id,
                CompanyDecisionProfileName = decisionProfile.Name,
                CompanyDecisionProfileVersion = decisionProfile.Version,
                RankedStrategies = rankedStrategies,
                RankedStrategyCount = rankedStrategies.Count,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
            };

            // BR-1101 / BR-1102 / BR-1110: persist before publication; failures abort.
            var persisted = await _recommendationService.PersistRankedStrategiesAsync(
                request,
                response,
                cancellationToken);

            var byStrategyId = persisted.ToDictionary(
                recommendation => recommendation.DeliveryStrategyId,
                recommendation => recommendation.Id);

            foreach (var ranked in rankedStrategies)
            {
                if (byStrategyId.TryGetValue(ranked.EvaluatedStrategy.Strategy.StrategyId, out var recommendationId))
                {
                    ranked.RecommendationId = recommendationId;
                }
            }

            stopwatch.Stop();
            response.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Delivery strategy ranking completed for Contract {ContractId} with {StrategyCount} strategies ranked and persisted using Decision Profile {DecisionProfileName} in {ElapsedMilliseconds} ms",
                request.ContractId,
                rankedStrategies.Count,
                decisionProfile.Name,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex) when (ex is not BusinessRuleException and not NotFoundException)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Delivery strategy ranking failed for Contract {ContractId}, Mission {MissionId}, and Decision Profile {DecisionProfileId} after {ElapsedMilliseconds} ms",
                request.ContractId,
                request.MissionId,
                request.CompanyDecisionProfileId,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
