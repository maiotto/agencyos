using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

public class AIRecommendationComparisonService : IAIRecommendationComparisonService
{
    public AIRecommendationComparisonResponse Compare(
        AIRecommendation aiRecommendation,
        Recommendation recommendation)
    {
        var comparison = aiRecommendation.Compare(recommendation);
        return new AIRecommendationComparisonResponse
        {
            AIRecommendation = AIRecommendationService.MapToResponse(aiRecommendation),
            RecommendationId = recommendation.Id,
            RecommendationTitle = recommendation.Title,
            RecommendationScore = recommendation.Score,
            RecommendationRank = recommendation.Rank,
            RecommendationVersion = recommendation.Version,
            HasDifferences = comparison.HasDifferences,
            Differences = comparison.Differences
                .Select(difference => new AIRecommendationComparisonFieldResponse
                {
                    Path = difference.Path,
                    LeftValue = difference.LeftValue,
                    RightValue = difference.RightValue,
                    Changed = difference.Changed
                })
                .ToList()
        };
    }
}
